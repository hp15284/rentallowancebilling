using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAllowanceBilling.Web.Data;
using RentAllowanceBilling.Web.Models;

namespace RentAllowanceBilling.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class BasicPayWithAllowancesController : Controller
{
    private readonly ApplicationDbContext _context;

    public BasicPayWithAllowancesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var slabs = await _context.BasicPayWithAllowances
            .OrderBy(s => s.MinBasicPay)
            .ToListAsync();
        return View(slabs);
    }

    public async Task<IActionResult> Details(int id)
    {
        var slab = await _context.BasicPayWithAllowances.FindAsync(id);
        if (slab is null) return NotFound();
        return View(slab);
    }

    public IActionResult Create() => View(new BasicPayWithAllowance());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BasicPayWithAllowance slab)
    {
        ValidateSlab(slab);
        if (!ModelState.IsValid) return View(slab);

        _context.BasicPayWithAllowances.Add(slab);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var slab = await _context.BasicPayWithAllowances.FindAsync(id);
        if (slab is null) return NotFound();
        return View(slab);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BasicPayWithAllowance slab)
    {
        if (id != slab.Id) return BadRequest();

        ValidateSlab(slab);
        if (!ModelState.IsValid) return View(slab);

        var existing = await _context.BasicPayWithAllowances.FindAsync(id);
        if (existing is null) return NotFound();

        existing.MinBasicPay = slab.MinBasicPay;
        existing.MaxBasicPay = slab.MaxBasicPay;
        existing.AllowanceAmount = slab.AllowanceAmount;
        existing.IsActive = slab.IsActive;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var slab = await _context.BasicPayWithAllowances.FindAsync(id);
        if (slab is null) return NotFound();
        return View(slab);
    }

    [HttpPost, ActionName(nameof(Delete))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var slab = await _context.BasicPayWithAllowances.FindAsync(id);
        if (slab is null) return NotFound();

        _context.BasicPayWithAllowances.Remove(slab);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private void ValidateSlab(BasicPayWithAllowance slab)
    {
        if (slab.MaxBasicPay is not null && slab.MaxBasicPay < slab.MinBasicPay)
        {
            ModelState.AddModelError(nameof(BasicPayWithAllowance.MaxBasicPay),
                "Max basic pay must be greater than or equal to min basic pay.");
        }
    }
}
