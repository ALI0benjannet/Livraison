using LivraisonApp.Data;
using LivraisonApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Controllers;

[Authorize(Roles = "Livreur")]
public class MesLivraisonsController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public MesLivraisonsController(AppDbContext db, UserManager<ApplicationUser> um)
    {
        _db = db;
        _userManager = um;
    }

    public async Task<IActionResult> Index(string? search, StatutColis? statut)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.LivreurId is null)
        {
            TempData["Error"] = "Aucun livreur lié à votre compte. Contactez l'administrateur.";
            return View(Enumerable.Empty<Colis>());
        }

        var q = _db.Colis
            .Include(c => c.Client)
            .Where(c => c.LivreurId == user.LivreurId);

        if (statut.HasValue) q = q.Where(c => c.Statut == statut.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            q = q.Where(c => (c.Libelle ?? "").Contains(search)
                          || (c.Client!.Nom ?? "").Contains(search)
                          || (c.Client!.Prenom ?? "").Contains(search));
        }

        ViewBag.Search = search;
        ViewBag.Statut = statut;
        ViewBag.Total = await _db.Colis.CountAsync(c => c.LivreurId == user.LivreurId);
        ViewBag.EnAttente = await _db.Colis.CountAsync(c => c.LivreurId == user.LivreurId && c.Statut == StatutColis.EnAttente);
        ViewBag.EnCours   = await _db.Colis.CountAsync(c => c.LivreurId == user.LivreurId && c.Statut == StatutColis.EnCours);
        ViewBag.Livres    = await _db.Colis.CountAsync(c => c.LivreurId == user.LivreurId && c.Statut == StatutColis.Livre);

        var list = await q.OrderByDescending(c => c.DateLivraison).ToListAsync();

        // Coordonnées des clients (téléphone / adresse) via ApplicationUser
        var clientIds = list.Select(c => c.ClientId).Distinct().ToList();
        var clientInfos = await _db.Users
            .Where(u => u.ClientId != null && clientIds.Contains(u.ClientId.Value))
            .Select(u => new { u.ClientId, u.PhoneNumber, u.Adresse })
            .ToListAsync();
        ViewBag.ClientInfos = clientInfos.ToDictionary(x => x.ClientId!.Value, x => (object)x);

        return View(list);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangerStatut(int id, StatutColis statut)
    {
        var user = await _userManager.GetUserAsync(User);
        var colis = await _db.Colis.FirstOrDefaultAsync(c => c.Id == id && c.LivreurId == user!.LivreurId);
        if (colis is null) return NotFound();

        colis.Statut = statut;
        await _db.SaveChangesAsync();

        // Recalcul des compteurs pour le rendu AJAX
        var lid = user!.LivreurId;
        var total     = await _db.Colis.CountAsync(c => c.LivreurId == lid);
        var enAttente = await _db.Colis.CountAsync(c => c.LivreurId == lid && c.Statut == StatutColis.EnAttente);
        var enCours   = await _db.Colis.CountAsync(c => c.LivreurId == lid && c.Statut == StatutColis.EnCours);
        var livres    = await _db.Colis.CountAsync(c => c.LivreurId == lid && c.Statut == StatutColis.Livre);

        // Si appel AJAX -> on renvoie du JSON
        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return Json(new {
                success = true,
                message = $"Statut du colis #{id} mis à jour.",
                statut  = statut.ToString(),
                counts  = new { total, enAttente, enCours, livres }
            });
        }

        TempData["Success"] = $"Statut du colis #{id} mis à jour.";
        return RedirectToAction(nameof(Index));
    }
}
