using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAllowanceBilling.Web.Data;
using RentAllowanceBilling.Web.Models;

namespace RentAllowanceBilling.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class BranchesController : Controller
{
    private readonly ApplicationDbContext _context;

    public BranchesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Branches.OrderBy(b => b.Name).ToListAsync());
    }

    public IActionResult Create() => View(new Branch());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Branch branch)
    {
        if (!ModelState.IsValid) return View(branch);

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch is null) return NotFound();
        return View(branch);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Branch branch)
    {
        if (id != branch.Id) return BadRequest();
        if (!ModelState.IsValid) return View(branch);

        _context.Update(branch);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var branch = await _context.Branches.FindAsync(id);
        if (branch is null) return NotFound();
        branch.IsActive = !branch.IsActive;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
