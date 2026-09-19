using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAllowanceBilling.Web.Data;
using RentAllowanceBilling.Web.Models;
using RentAllowanceBilling.Web.Models.ViewModels;

namespace RentAllowanceBilling.Web.Controllers;

[Authorize]
public class BillsController : Controller
{
    private static readonly string[] AllowedLeaveReportExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
    private const long MaxLeaveReportSizeBytes = 5 * 1024 * 1024;

    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public BillsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    private IQueryable<RentAllowanceBill> BillsWithIncludes() =>
        _context.RentAllowanceBills
            .Include(b => b.Employee)
            .Include(b => b.Branch)
            .Include(b => b.Trips);

    private async Task<Employee?> GetCurrentEmployeeAsync()
    {
        var userId = _userManager.GetUserId(User);
        return await _context.Employees.Include(e => e.Branch).FirstOrDefaultAsync(e => e.ApplicationUserId == userId);
    }

    public IActionResult Index()
    {
        if (User.IsInRole(Roles.BranchManager))
        {
            return RedirectToAction(nameof(PendingBranchManager));
        }
        if (User.IsInRole(Roles.Accountant))
        {
            return RedirectToAction(nameof(PendingAccountant));
        }
        if (User.IsInRole(Roles.ApprovingAuthority))
        {
            return RedirectToAction(nameof(PendingApproval));
        }
        return RedirectToAction(nameof(MyBills));
    }

    // ---------- Employee ----------

    [Authorize(Roles = Roles.Employee)]
    public async Task<IActionResult> MyBills()
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is null) return Forbid();

        var bills = await BillsWithIncludes()
            .Where(b => b.EmployeeId == employee.Id)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(bills);
    }

    [Authorize(Roles = Roles.Employee)]
    public async Task<IActionResult> Create()
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is null)
        {
            TempData["Error"] = "Your login is not linked to an employee record. Contact the Administrator.";
            return RedirectToAction(nameof(MyBills));
        }

        var model = new BillFormViewModel
        {
            EmployeeId = employee.Id,
            EmployeeName = employee.FullName,
            Designation = employee.Designation,
            BasicSalary = employee.BasicSalary,
            BranchId = employee.BranchId
        };
        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Employee)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BillFormViewModel model)
    {
        var employee = await GetCurrentEmployeeAsync();
        if (employee is null) return Forbid();

        var trips = (model.Trips ?? new()).Where(t => t.TravelDate.HasValue && !string.IsNullOrWhiteSpace(t.FromPlace)).ToList();
        if (trips.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Add at least one travel row with date, from and to place.");
        }

        var leaveReportFile = model.LeaveReportCopy;
        if (leaveReportFile is not null)
        {
            var extension = Path.GetExtension(leaveReportFile.FileName).ToLowerInvariant();
            if (!AllowedLeaveReportExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.LeaveReportCopy), "Leave report copy must be a PDF, JPG or PNG file.");
            }
            else if (leaveReportFile.Length > MaxLeaveReportSizeBytes)
            {
                ModelState.AddModelError(nameof(model.LeaveReportCopy), "Leave report copy must be 5 MB or smaller.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.EmployeeId = employee.Id;
            model.EmployeeName = employee.FullName;
            model.Designation = employee.Designation;
            return View(model);
        }

        var bill = new RentAllowanceBill
        {
            EmployeeId = employee.Id,
            BranchId = employee.BranchId,
            BasicSalary = employee.BasicSalary,
            PeriodFrom = model.PeriodFrom,
            PeriodTo = model.PeriodTo,
            PurposeOfVisit = model.PurposeOfVisit,
            ManagerOnLeaveFrom = model.ManagerOnLeaveFrom,
            ManagerOnLeaveTo = model.ManagerOnLeaveTo,
            ChargeHandedOverDate = model.ChargeHandedOverDate,
            Status = BillStatus.Draft,
            CreatedByUserId = _userManager.GetUserId(User),
            Trips = trips.Select(t => new RentAllowanceBillTrip
            {
                TravelDate = t.TravelDate!.Value,
                FromPlace = t.FromPlace ?? string.Empty,
                ToPlace = t.ToPlace ?? string.Empty,
                DepartureTime = ParseTime(t.DepartureTime),
                ReturnTime = ParseTime(t.ReturnTime),
                Fare = t.Fare,
                Allowance = t.Allowance
            }).ToList()
        };

        if (leaveReportFile is not null)
        {
            bill.LeaveReportCopyPath = await SaveLeaveReportCopyAsync(leaveReportFile);
        }

        _context.RentAllowanceBills.Add(bill);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = bill.Id });
    }

    private async Task<string> SaveLeaveReportCopyAsync(IFormFile file)
    {
        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "leave-reports");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName).ToLowerInvariant()}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/leave-reports/{fileName}";
    }

    private static TimeSpan? ParseTime(string? value) =>
        TimeSpan.TryParse(value, out var ts) ? ts : null;

    [Authorize(Roles = Roles.Employee)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id)
    {
        var employee = await GetCurrentEmployeeAsync();
        var bill = await _context.RentAllowanceBills.FirstOrDefaultAsync(b => b.Id == id);
        if (bill is null || employee is null || bill.EmployeeId != employee.Id) return Forbid();
        if (bill.Status != BillStatus.Draft) return BadRequest("Only draft bills can be submitted.");

        bill.Status = BillStatus.PendingBranchManager;
        bill.SubmittedAt = DateTime.UtcNow;
        bill.BillNumber = $"RAB-{DateTime.UtcNow:yyyyMMdd}-{bill.Id:D5}";
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id });
    }

    // ---------- Branch Manager ----------

    [Authorize(Roles = Roles.BranchManager)]
    public async Task<IActionResult> PendingBranchManager()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var query = BillsWithIncludes().Where(b => b.Status == BillStatus.PendingBranchManager);
        if (currentUser?.BranchId is int branchId)
        {
            query = query.Where(b => b.BranchId == branchId);
        }
        var bills = await query.OrderBy(b => b.SubmittedAt).ToListAsync();
        return View(bills);
    }

    [Authorize(Roles = Roles.BranchManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Recommend(WorkflowActionViewModel model)
    {
        var bill = await _context.RentAllowanceBills.FirstOrDefaultAsync(b => b.Id == model.BillId);
        if (bill is null) return NotFound();
        if (bill.Status != BillStatus.PendingBranchManager) return BadRequest();

        bill.BranchManagerUserId = _userManager.GetUserId(User);
        bill.BranchManagerActionAt = DateTime.UtcNow;
        bill.BranchManagerRemark = model.Remark;
        bill.Status = BillStatus.PendingAccountant;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(PendingBranchManager));
    }

    // ---------- Accountant ----------

    [Authorize(Roles = Roles.Accountant)]
    public async Task<IActionResult> PendingAccountant()
    {
        var bills = await BillsWithIncludes()
            .Where(b => b.Status == BillStatus.PendingAccountant)
            .OrderBy(b => b.BranchManagerActionAt)
            .ToListAsync();
        return View(bills);
    }

    [Authorize(Roles = Roles.Accountant)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(WorkflowActionViewModel model)
    {
        var bill = await _context.RentAllowanceBills.Include(b => b.Trips).FirstOrDefaultAsync(b => b.Id == model.BillId);
        if (bill is null) return NotFound();
        if (bill.Status != BillStatus.PendingAccountant) return BadRequest();

        bill.AccountantUserId = _userManager.GetUserId(User);
        bill.AccountantActionAt = DateTime.UtcNow;
        bill.AccountantRemark = model.Remark;
        bill.VerifiedAmount = model.Amount ?? bill.TotalAmount;
        bill.Status = BillStatus.PendingApproval;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(PendingAccountant));
    }

    // ---------- Approving Authority ----------

    [Authorize(Roles = Roles.ApprovingAuthority)]
    public async Task<IActionResult> PendingApproval()
    {
        var bills = await BillsWithIncludes()
            .Where(b => b.Status == BillStatus.PendingApproval)
            .OrderBy(b => b.AccountantActionAt)
            .ToListAsync();
        return View(bills);
    }

    [Authorize(Roles = Roles.ApprovingAuthority)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(WorkflowActionViewModel model)
    {
        var bill = await _context.RentAllowanceBills.FirstOrDefaultAsync(b => b.Id == model.BillId);
        if (bill is null) return NotFound();
        if (bill.Status != BillStatus.PendingApproval) return BadRequest();

        bill.ApprovingAuthorityUserId = _userManager.GetUserId(User);
        bill.ApprovingAuthorityActionAt = DateTime.UtcNow;
        bill.ApprovingAuthorityRemark = model.Remark;
        bill.ApprovedAmount = model.Amount ?? bill.VerifiedAmount;
        bill.ApprovedAmountInWords = model.AmountInWords;
        bill.Status = BillStatus.Approved;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(PendingApproval));
    }

    // ---------- Shared ----------

    [Authorize(Roles = $"{Roles.BranchManager},{Roles.Accountant},{Roles.ApprovingAuthority}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(RejectBillViewModel model)
    {
        var bill = await _context.RentAllowanceBills.FirstOrDefaultAsync(b => b.Id == model.BillId);
        if (bill is null) return NotFound();
        if (bill.Status is BillStatus.Draft or BillStatus.Approved or BillStatus.Rejected) return BadRequest();

        bill.Status = BillStatus.Rejected;
        bill.RejectionReason = model.Reason;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = bill.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var bill = await BillsWithIncludes().FirstOrDefaultAsync(b => b.Id == id);
        if (bill is null) return NotFound();

        if (User.IsInRole(Roles.Employee) && !User.IsInRole(Roles.Admin))
        {
            var employee = await GetCurrentEmployeeAsync();
            if (employee is null || bill.EmployeeId != employee.Id) return Forbid();
        }

        return View(bill);
    }

    public async Task<IActionResult> Print(int id)
    {
        var bill = await BillsWithIncludes().FirstOrDefaultAsync(b => b.Id == id);
        if (bill is null) return NotFound();
        return View(bill);
    }
}
