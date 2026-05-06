using LivraisonApp.Models;
using LivraisonApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LivraisonApp.Controllers;

[Authorize(Roles = "Admin")]
public class VehiculeController : Controller
{
    private readonly IUnitOfWork _uow;
    public VehiculeController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index() => View(await _uow.Vehicules.GetAllAsync());
    public async Task<IActionResult> Details(int id) => View(await _uow.Vehicules.GetByIdAsync(id));
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string type, Camion camion, Voiture voiture)
    {
        if (type == "Camion")
            await _uow.Vehicules.AddAsync(camion);
        else
            await _uow.Vehicules.AddAsync(voiture);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Véhicule ajouté avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id) => View(await _uow.Vehicules.GetByIdAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string type, Camion camion, Voiture voiture)
    {
        if (type == "Camion")
            _uow.Vehicules.Update(camion);
        else
            _uow.Vehicules.Update(voiture);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Véhicule modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id) => View(await _uow.Vehicules.GetByIdAsync(id));

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _uow.Vehicules.GetByIdAsync(id);
        if (entity is not null)
        {
            _uow.Vehicules.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        TempData["Success"] = "Véhicule supprimé avec succès.";
        return RedirectToAction(nameof(Index));
    }
}
