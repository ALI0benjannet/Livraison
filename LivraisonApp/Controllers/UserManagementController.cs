using LivraisonApp.Models;
using LivraisonApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Controllers;

[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserManagementController(UserManager<ApplicationUser> userManager)
        => _userManager = userManager;

    public async Task<IActionResult> Index()
    {
        var users = await _userManager.Users.ToListAsync();
        var vms = new List<UserManagementViewModel>();
        foreach (var u in users)
        {
            var roles = await _userManager.GetRolesAsync(u);
            vms.Add(new UserManagementViewModel
            {
                Id = u.Id,
                Email = u.Email ?? "",
                Role = roles.FirstOrDefault() ?? "—",
                EmailConfirmed = u.EmailConfirmed,
                LockoutEnd = u.LockoutEnd
            });
        }
        return View(vms);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null || user.Email == "admin@livraison.local")
        {
            TempData["Error"] = "Action impossible sur ce compte.";
            return RedirectToAction(nameof(Index));
        }
        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            await _userManager.SetLockoutEndDateAsync(user, null);
            TempData["Success"] = $"Compte {user.Email} déverrouillé.";
        }
        else
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            TempData["Success"] = $"Compte {user.Email} verrouillé.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null || user.Email == "admin@livraison.local")
        {
            TempData["Error"] = "Impossible de supprimer ce compte.";
            return RedirectToAction(nameof(Index));
        }
        await _userManager.DeleteAsync(user);
        TempData["Success"] = $"Utilisateur {user.Email} supprimé.";
        return RedirectToAction(nameof(Index));
    }
}
