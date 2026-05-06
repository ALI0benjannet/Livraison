using LivraisonApp.Data;
using LivraisonApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Controllers;

[Authorize(Roles = "User")]
public class UserController : Controller
{
    private readonly AppDbContext _context;
    public UserController(AppDbContext context) => _context = context;

    public IActionResult Index() => RedirectToAction(nameof(Colis));

    public async Task<IActionResult> Colis(ColisSearchViewModel search)
    {
        var query = _context.Colis.Include(c => c.Client).Include(c => c.Livreur).AsQueryable();
        if (search.PrixMin.HasValue)         query = query.Where(c => c.Montant >= search.PrixMin);
        if (search.PrixMax.HasValue)         query = query.Where(c => c.Montant <= search.PrixMax);
        if (search.DateLivraison.HasValue)   query = query.Where(c => c.DateLivraison.Date == search.DateLivraison.Value.Date);
        if (!string.IsNullOrWhiteSpace(search.Libelle))  query = query.Where(c => c.Libelle != null && c.Libelle.Contains(search.Libelle));
        if (!string.IsNullOrWhiteSpace(search.Ville))    query = query.Where(c => c.Client != null && c.Client.Ville == search.Ville);
        if (!string.IsNullOrWhiteSpace(search.Livreur))  query = query.Where(c => c.Livreur != null && c.Livreur.CIN.Contains(search.Livreur));
        if (search.Statut.HasValue)          query = query.Where(c => c.Statut == search.Statut.Value);
        ViewBag.Search = search;
        return View(await query.ToListAsync());
    }

    public async Task<IActionResult> Details(int id) => View(await _context.Colis.Include(c => c.Client).Include(c => c.Livreur).FirstOrDefaultAsync(c => c.Id == id));
}
