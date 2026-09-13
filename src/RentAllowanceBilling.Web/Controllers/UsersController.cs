using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentAllowanceBilling.Web.Data;
using RentAllowanceBilling.Web.Models;
using RentAllowanceBilling.Web.Models.ViewModels;

namespace RentAllowanceBilling.Web.Controllers;

[Authorize(Roles = Roles.Admin)]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.Include(u => u.Branch).OrderBy(u => u.Email).ToListAsync();
        var result = new List<UserListItemViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName,
                BranchName = user.Branch?.Name,
                Roles = roles
            });
        }
        return View(result);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var model = new EditUserRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            BranchId = user.BranchId,
            SelectedRoles = (await _userManager.GetRolesAsync(user)).ToList(),
            Branches = await _context.Branches.Where(b => b.IsActive).OrderBy(b => b.Name).ToListAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserRolesViewModel model, string[] selectedRoles)
    {
        var user = await _userManager.FindByIdAsync(model.UserId);
        if (user is null) return NotFound();

        user.BranchId = model.BranchId;
        await _userManager.UpdateAsync(user);

        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = selectedRoles.Except(currentRoles);
        var rolesToRemove = currentRoles.Except(selectedRoles);

        await _userManager.AddToRolesAsync(user, rolesToAdd);
        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

        return RedirectToAction(nameof(Index));
    }
}
