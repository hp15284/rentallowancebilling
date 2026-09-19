using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        return RedirectToAction(nameof(MyBills));
    }

    // ---------- Employee (view-only) ----------

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

    // ---------- Branch Manager: raise & submit bills for employees ----------

    private async Task PopulateEmployeesAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Branch)
            .Where(e => e.IsActive)
            .OrderBy(e => e.FullName)
            .ToListAsync();

        ViewBag.Employees = new SelectList(
            employees.Select(e => new { e.Id, Display = $"{e.FullName} ({e.Branch?.Name})" }),
            "Id", "Display");
        ViewBag.EmployeeDataJson = JsonSerializer.Serialize(employees.Select(e => new
        {
            id = e.Id,
            pfNumber = e.PfNumber ?? "",
            designation = e.Designation,
            basicSalary = e.BasicSalary
        }));
    }

    [Authorize(Roles = Roles.BranchManager)]
    public async Task<IActionResult> Drafts()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.BranchId is not int branchId) return Forbid();

        var bills = await BillsWithIncludes()
            .Where(b => b.BranchId == branchId && b.Status == BillStatus.Draft)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return View(bills);
    }

    [Authorize(Roles = Roles.BranchManager)]
    public async Task<IActionResult> Create()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.BranchId is not int branchId)
        {
            TempData["Error"] = "Your login is not linked to a branch. Contact the Administrator.";
            return RedirectToAction(nameof(PendingBranchManager));
        }

        await PopulateEmployeesAsync();
        return View(new BillFormViewModel { BranchId = branchId });
    }

    [HttpPost]
    [Authorize(Roles = Roles.BranchManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BillFormViewModel model)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser?.BranchId is not int branchId) return Forbid();

        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == model.EmployeeId && e.IsActive);
        if (employee is null)
        {
            ModelState.AddModelError(nameof(model.EmployeeId), "Select a valid employee.");
        }

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
            if (employee is not null)
            {
                model.EmployeeName = employee.FullName;
                model.Designation = employee.Designation;
                model.BasicSalary = employee.BasicSalary;
            }
            await PopulateEmployeesAsync();
            return View(model);
        }

        var bill = new RentAllowanceBill
        {
            EmployeeId = employee!.Id,
            BranchId = branchId,
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

    [Authorize(Roles = Roles.BranchManager)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        var bill = await _context.RentAllowanceBills.FirstOrDefaultAsync(b => b.Id == id);
        if (bill is null || currentUser?.BranchId is not int branchId || bill.BranchId != branchId) return Forbid();
        if (bill.Status != BillStatus.Draft) return BadRequest("Only draft bills can be submitted.");

        bill.Status = BillStatus.PendingBranchManager;
        bill.SubmittedAt = DateTime.UtcNow;
        bill.BillNumber = $"RAB-{DateTime.UtcNow:yyyyMMdd}-{bill.Id:D5}";
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Drafts));
    }

    // ---------- Branch Manager: recommend submitted bills ----------

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
        bill.ApprovedAmount = model.Amount ?? bill.TotalAmount;
        bill.ApprovedAmountInWords = model.AmountInWords;
        bill.Status = BillStatus.Approved;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(PendingBranchManager));
    }

    // ---------- Shared ----------

    [Authorize(Roles = Roles.BranchManager)]
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
