using LivraisonApp.Models;
using LivraisonApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LivraisonApp.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var user = new ApplicationUser { UserName = vm.Login, Email = vm.Login, Role = vm.Role };
        var result = await _userManager.CreateAsync(user, vm.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, vm.Role);
            return RedirectToAction(nameof(Login));
        }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return View(vm);
    }

    [AllowAnonymous]
    public IActionResult Login() => View();

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await _signInManager.PasswordSignInAsync(vm.Login, vm.Password, vm.RememberMe, false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Identifiants invalides.");
            return View(vm);
        }

        var user = await _userManager.FindByNameAsync(vm.Login);
        if (user is not null && await _userManager.IsInRoleAsync(user, "Admin"))
            return RedirectToAction("Index", "Dashboard");

        return RedirectToAction("Index", "Colis");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
