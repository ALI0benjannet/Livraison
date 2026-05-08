using LivraisonApp.Data;
using LivraisonApp.Models;
using LivraisonApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Controllers;

[Authorize(Roles = "Client")]
public class UserController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public UserController(AppDbContext context, UserManager<ApplicationUser> um)
    {
        _context = context;
        _userManager = um;
    }

    public IActionResult Index() => RedirectToAction(nameof(Colis));

    public async Task<IActionResult> Colis(ColisSearchViewModel search)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user?.ClientId is null)
        {
            TempData["Error"] = "Aucun client lié à votre compte.";
            return View(Enumerable.Empty<Colis>());
        }

        var query = _context.Colis
            .Include(c => c.Client)
            .Include(c => c.Livreur)
            .Where(c => c.ClientId == user.ClientId)
            .AsQueryable();

        if (search.PrixMin.HasValue)         query = query.Where(c => c.Montant >= search.PrixMin);
        if (search.PrixMax.HasValue)         query = query.Where(c => c.Montant <= search.PrixMax);
        if (search.DateLivraison.HasValue)   query = query.Where(c => c.DateLivraison.Date == search.DateLivraison.Value.Date);
        if (!string.IsNullOrWhiteSpace(search.Libelle))  query = query.Where(c => c.Libelle != null && c.Libelle.Contains(search.Libelle));
        if (!string.IsNullOrWhiteSpace(search.Livreur))
            query = query.Where(c => c.Livreur != null &&
                ((c.Livreur.RaisonSocial ?? "").Contains(search.Livreur) || c.Livreur.CIN.Contains(search.Livreur)));
        if (search.Statut.HasValue)          query = query.Where(c => c.Statut == search.Statut.Value);

        var list = await query.OrderByDescending(c => c.DateLivraison).ToListAsync();

        // Coordonnées des livreurs (téléphone / adresse) via ApplicationUser
        var livreurIds = list.Where(c => c.LivreurId > 0).Select(c => c.LivreurId).Distinct().ToList();
        var livreurInfos = await _context.Users
            .Where(u => u.LivreurId != null && livreurIds.Contains(u.LivreurId.Value))
            .Select(u => new { u.LivreurId, u.PhoneNumber, u.Adresse, u.Nom, u.Prenom })
            .ToListAsync();
        ViewBag.LivreurInfos = livreurInfos.ToDictionary(x => x.LivreurId!.Value, x => (object)x);

        ViewBag.Search = search;
        return View(list);
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var c = await _context.Colis
            .Include(x => x.Client).Include(x => x.Livreur)
            .FirstOrDefaultAsync(x => x.Id == id && x.ClientId == user!.ClientId);
        if (c is null) return NotFound();

        if (c.LivreurId > 0)
        {
            var u = await _context.Users.FirstOrDefaultAsync(x => x.LivreurId == c.LivreurId);
            ViewBag.LivreurUser = u;
        }
        return View(c);
    }
}

