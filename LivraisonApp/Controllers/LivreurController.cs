using LivraisonApp.Data;
using LivraisonApp.Models;
using LivraisonApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LivraisonApp.Controllers;

[Authorize(Roles = "Admin")]
public class LivreurController : Controller
{
    private readonly IUnitOfWork _uow;
    private readonly AppDbContext _context;
    public LivreurController(IUnitOfWork uow, AppDbContext context) { _uow = uow; _context = context; }

    public async Task<IActionResult> Index() => View(await _uow.Livreurs.GetAllAsync());
    public async Task<IActionResult> Details(int id) => View(await _uow.Livreurs.GetByIdAsync(id));
    public IActionResult Create() { ViewBag.VehiculeId = new SelectList(_context.Vehicules, "Id", "Matricule"); return View(); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Livreur livreur)
    {
        if (!ModelState.IsValid) return View(livreur);
        await _uow.Livreurs.AddAsync(livreur);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Livreur ajouté avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _uow.Livreurs.GetByIdAsync(id);
        ViewBag.VehiculeId = new SelectList(_context.Vehicules, "Id", "Matricule", entity?.VehiculeId);
        return View(entity);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Livreur livreur)
    {
        if (!ModelState.IsValid) return View(livreur);
        _uow.Livreurs.Update(livreur);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Livreur modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id) => View(await _uow.Livreurs.GetByIdAsync(id));

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _uow.Livreurs.GetByIdAsync(id);
        if (entity is not null)
        {
            _uow.Livreurs.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        TempData["Success"] = "Livreur supprimé avec succès.";
        return RedirectToAction(nameof(Index));
    }
}
