using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CommunityLibrary.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> _roleManager;

    public RolesController(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        var roles = _roleManager.Roles.OrderBy(r => r.Name).ToList();
        return View(roles);
    }

   [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            TempData["Error"] = "Role name is required.";
            return RedirectToAction(nameof(Index));
        }

        var name = roleName.Trim();
        if (await _roleManager.RoleExistsAsync(name))
        {
            TempData["Error"] = $"Role '{name}' already exists.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _roleManager.CreateAsync(new IdentityRole(name));
        if (result.Succeeded)
        {
            TempData["Message"] = $"Role '{name}' created.";
        }
        else
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string? id)
    {
        if (string.IsNullOrEmpty(id))
            return NotFound();

        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound();

        return View(role);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return RedirectToAction(nameof(Index));

        var result = await _roleManager.DeleteAsync(role);
        if (result.Succeeded)
            TempData["Message"] = $"Role '{role.Name}' deleted.";
        else
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));

        return RedirectToAction(nameof(Index));
    }
}
