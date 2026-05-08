using LivraisonApp.Data;
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
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
                             AppDbContext db, IWebHostEnvironment env)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _env = env;
    }

    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost, AllowAnonymous, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (vm.Role != "Client" && vm.Role != "Livreur")
        {
            ModelState.AddModelError(nameof(vm.Role), "Rôle invalide.");
            return View(vm);
        }

        // Upload photo
        string? photoUrl = null;
        if (vm.Photo is { Length: > 0 })
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(vm.Photo.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError(nameof(vm.Photo), "Format d'image non supporté.");
                return View(vm);
            }
            var dir = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(dir);
            var fname = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(dir, fname);
            await using (var fs = System.IO.File.Create(fullPath))
                await vm.Photo.CopyToAsync(fs);
            photoUrl = $"/uploads/profiles/{fname}";
        }

        // Création entité métier liée
        int? clientId = null, livreurId = null;
        if (vm.Role == "Client")
        {
            var c = new Client { Nom = vm.Nom, Prenom = vm.Prenom, Ville = vm.Ville };
            _db.Clients.Add(c);
            await _db.SaveChangesAsync();
            clientId = c.Id;
        }
        else // Livreur
        {
            if (string.IsNullOrWhiteSpace(vm.CIN))
            {
                ModelState.AddModelError(nameof(vm.CIN), "Le CIN est requis pour un livreur.");
                return View(vm);
            }
            var l = new Livreur { CIN = vm.CIN, RaisonSocial = $"{vm.Nom} {vm.Prenom}", Ville = vm.Ville };
            _db.Livreurs.Add(l);
            await _db.SaveChangesAsync();
            livreurId = l.Id;
        }

        var user = new ApplicationUser
        {
            UserName = vm.Login,
            Email = vm.Login,
            EmailConfirmed = true,
            Role = vm.Role,
            Nom = vm.Nom,
            Prenom = vm.Prenom,
            DateNaissance = vm.DateNaissance,
            PhoneNumber = vm.Telephone,
            Adresse = vm.Adresse,
            PhotoUrl = photoUrl,
            ClientId = clientId,
            LivreurId = livreurId
        };
        var result = await _userManager.CreateAsync(user, vm.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, vm.Role);
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return View(vm);
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        // Si déjà connecté, ne JAMAIS afficher la page de login : rediriger vers la home selon le rôle.
        if (User?.Identity?.IsAuthenticated == true)
        {
            if (User.IsInRole("Admin"))   return RedirectToAction("Index", "Dashboard");
            if (User.IsInRole("Livreur")) return RedirectToAction("Index", "MesLivraisons");
            if (User.IsInRole("Client"))  return RedirectToAction("Colis", "User");
            return Redirect("/");
        }
        return View();
    }

    [AllowAnonymous]
    public IActionResult AccessDenied(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

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
        if (user is null) return RedirectToAction(nameof(Login));

        if (await _userManager.IsInRoleAsync(user, "Admin"))
            return RedirectToAction("Index", "Dashboard");
        if (await _userManager.IsInRoleAsync(user, "Livreur"))
            return RedirectToAction("Index", "MesLivraisons");

        // Client par défaut
        return RedirectToAction("Colis", "User");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }
}
