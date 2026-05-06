using LivraisonApp.Models;
using LivraisonApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LivraisonApp.Controllers;

[Authorize(Roles = "Admin")]
public class ClientController : Controller
{
    private readonly IUnitOfWork _uow;
    public ClientController(IUnitOfWork uow) => _uow = uow;

    public async Task<IActionResult> Index() => View(await _uow.Clients.GetAllAsync());
    public async Task<IActionResult> Details(int id) => View(await _uow.Clients.GetByIdAsync(id));
    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client)
    {
        if (!ModelState.IsValid) return View(client);
        await _uow.Clients.AddAsync(client);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Client ajouté avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id) => View(await _uow.Clients.GetByIdAsync(id));

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Client client)
    {
        if (!ModelState.IsValid) return View(client);
        _uow.Clients.Update(client);
        await _uow.SaveChangesAsync();
        TempData["Success"] = "Client modifié avec succès.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id) => View(await _uow.Clients.GetByIdAsync(id));

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await _uow.Clients.GetByIdAsync(id);
        if (entity is not null)
        {
            _uow.Clients.Delete(entity);
            await _uow.SaveChangesAsync();
        }
        TempData["Success"] = "Client supprimé avec succès.";
        return RedirectToAction(nameof(Index));
    }
}
