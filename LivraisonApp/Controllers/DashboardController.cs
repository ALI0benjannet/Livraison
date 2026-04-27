using LivraisonApp.Data;
using LivraisonApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Controllers;

[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;
    public DashboardController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var vm = new DashboardViewModel
        {
            TotalColis = await _context.Colis.CountAsync(),
            TotalClients = await _context.Clients.CountAsync(),
            TotalLivreurs = await _context.Livreurs.CountAsync(),
            TotalVehicules = await _context.Vehicules.CountAsync()
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Stats()
    {
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, now.Month, 1).AddMonths(-11);

        var colisByMonth = await _context.Colis
            .Where(c => c.DateLivraison >= start)
            .GroupBy(c => new { c.DateLivraison.Year, c.DateLivraison.Month })
            .Select(g => new { label = $"{g.Key.Month:D2}/{g.Key.Year}", total = g.Count(), revenue = g.Sum(x => x.Montant) })
            .OrderBy(x => x.label)
            .ToListAsync();

        var camions = await _context.Camions.CountAsync();
        var voitures = await _context.Voitures.CountAsync();
        var topClients = await _context.Clients
            .Select(c => new { client = $"{c.Nom} {c.Prenom}", total = c.Colis.Count })
            .OrderByDescending(x => x.total)
            .Take(5)
            .ToListAsync();

        return Json(new { colisByMonth, camions, voitures, topClients });
    }
}
