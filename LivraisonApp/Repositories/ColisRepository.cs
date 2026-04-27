using LivraisonApp.Data;
using LivraisonApp.Interfaces;
using LivraisonApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Repositories;

public class ColisRepository : Repository<Colis>, IColisRepository
{
    public ColisRepository(AppDbContext context) : base(context) { }

    public override async Task<IEnumerable<Colis>> GetAllAsync()
        => await Context.Colis.Include(c => c.Client).Include(c => c.Livreur).ToListAsync();

    public override async Task<Colis?> GetByIdAsync(int id)
        => await Context.Colis.Include(c => c.Client).Include(c => c.Livreur).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Colis>> SearchByPrix(double? min, double? max)
    {
        var query = Context.Colis.Include(c => c.Client).Include(c => c.Livreur).AsQueryable();
        if (min.HasValue) query = query.Where(c => c.Montant >= min.Value);
        if (max.HasValue) query = query.Where(c => c.Montant <= max.Value);
        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Colis>> SearchByLibelle(string? libelle)
    {
        if (string.IsNullOrWhiteSpace(libelle)) return await GetAllAsync();
        return await Context.Colis.Include(c => c.Client).Include(c => c.Livreur)
            .Where(c => c.Libelle != null && c.Libelle.Contains(libelle))
            .ToListAsync();
    }
}
