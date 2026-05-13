using LivraisonApp.Data;
using LivraisonApp.Models;
using LivraisonApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LivraisonApp.Controllers;

[Authorize]
public class ColisController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _env;

    public ColisController(IUnitOfWork uow, AppDbContext context, IWebHostEnvironment env)
    {
        _uow = uow;
        _context = context;
        _env = env;
    }

    public async Task<IActionResult> Index() => View(await _uow.Colis.GetAllAsync());
    public async Task<IActionResult> Details(int id) => View(await _uow.Colis.GetByIdAsync(id));

    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        ViewBag.ClientId = new SelectList(_context.Clients, "Id", "Nom");
        ViewBag.LivreurId = new SelectList(_context.Livreurs, "Id", "CIN");
        return View();
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Colis colis, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ClientId = new SelectList(_context.Clients, "Id", "Nom");
            ViewBag.LivreurId = new SelectList(_context.Livreurs, "Id", "CIN");
            return View(colis);
        }
        if (imageFile != null && imageFile.Length > 0)
        {
            var imageUrl = await SaveImageAsync(imageFile);
            if (imageUrl == null)
            {
                ModelState.AddModelError("imageFile", "Format invalide ou fichier trop volumineux (max 5 Mo, jpg/png/gif/webp).");
                ViewBag.ClientId = new SelectList(_context.Clients, "Id", "Nom");
                ViewBag.LivreurId = new SelectList(_context.Livreurs, "Id", "CIN");
                return View(colis);
            }
            colis.ImageUrl = imageUrl;
        }
        await _uow.Colis.AddAsync(colis);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Colis ajouté.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _uow.Colis.GetByIdAsync(id);
        ViewBag.ClientId = new SelectList(_context.Clients, "Id", "Nom", entity?.ClientId);
        ViewBag.LivreurId = new SelectList(_context.Livreurs, "Id", "CIN", entity?.LivreurId);
        return View(entity);
    }

    [HttpPost, Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Colis colis, IFormFile? imageFile)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.ClientId = new SelectList(_context.Clients, "Id", "Nom", colis.ClientId);
            ViewBag.LivreurId = new SelectList(_context.Livreurs, "Id", "CIN", colis.LivreurId);
            return View(colis);
        }
        if (imageFile != null && imageFile.Length > 0)
        {
            var newUrl = await SaveImageAsync(imageFile);
            if (newUrl == null)
            {
                ModelState.AddModelError("imageFile", "Format invalide ou fichier trop volumineux (max 5 Mo, jpg/png/gif/webp).");
                ViewBag.ClientId = new SelectList(_context.Clients, "Id", "Nom", colis.ClientId);
                ViewBag.LivreurId = new SelectList(_context.Livreurs, "Id", "CIN", colis.LivreurId);
                return View(colis);
            }
            DeleteImage(colis.ImageUrl);
            colis.ImageUrl = newUrl;
        }
        _uow.Colis.Update(colis);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Colis modifié.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id) => View(await _uow.Colis.GetByIdAsync(id));

    [HttpPost, ActionName("Delete"), Authorize(Roles = "Admin"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _uow.Colis.GetByIdAsync(id);
        if (entity is not null)
        {
            DeleteImage(entity.ImageUrl);
            _uow.Colis.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        TempData["Success"] = "Colis supprimé.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SaveImageAsync(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext) || file.Length > 5 * 1024 * 1024)
            return null;
        var folder = Path.Combine(_env.WebRootPath, "uploads", "colis");
        Directory.CreateDirectory(folder);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(folder, fileName);
        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/uploads/colis/{fileName}";
    }

    private void DeleteImage(string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;
        var path = Path.Combine(_env.WebRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
    }
}
