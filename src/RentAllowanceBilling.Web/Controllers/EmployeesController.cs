using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentAllowanceBilling.Web.Data;
using RentAllowanceBilling.Web.Models;

namespace RentAllowanceBilling.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class EmployeesController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var employees = await _context.Employees.Include(e => e.Branch).OrderBy(e => e.FullName).ToListAsync();
        return View(employees);
    }

    private async Task PopulateBranchesAsync()
    {
        ViewBag.Branches = new SelectList(await _context.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync(), "Id", "Name");
    }

    public async Task<IActionResult> Create()
    {
        await PopulateBranchesAsync();
        return View(new Employee());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        ModelState.Remove(nameof(Employee.Branch));
        ModelState.Remove(nameof(Employee.ApplicationUserId));
        if (!ModelState.IsValid)
        {
            await PopulateBranchesAsync();
            return View(employee);
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee is null) return NotFound();
        await PopulateBranchesAsync();
        return View(employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee employee)
    {
        if (id != employee.Id) return BadRequest();
        ModelState.Remove(nameof(Employee.Branch));
        ModelState.Remove(nameof(Employee.ApplicationUserId));
        if (!ModelState.IsValid)
        {
            await PopulateBranchesAsync();
            return View(employee);
        }

        var existing = await _context.Employees.FindAsync(id);
        if (existing is null) return NotFound();

        existing.FullName = employee.FullName;
        existing.PfNumber = employee.PfNumber;
        existing.Designation = employee.Designation;
        existing.BasicSalary = employee.BasicSalary;
        existing.BranchId = employee.BranchId;
        existing.IsActive = employee.IsActive;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
