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

    public ColisController(IUnitOfWork uow, AppDbContext context)
    {
        _uow = uow;
        _context = context;
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
    public async Task<IActionResult> Create(Colis colis)
    {
        if (!ModelState.IsValid) return View(colis);
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
    public async Task<IActionResult> Edit(Colis colis)
    {
        if (!ModelState.IsValid) return View(colis);
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
            _uow.Colis.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        TempData["Success"] = "Colis supprimé.";
        return RedirectToAction(nameof(Index));
    }
}
