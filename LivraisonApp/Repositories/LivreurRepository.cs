using LivraisonApp.Data;
using LivraisonApp.Interfaces;
using LivraisonApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Repositories;

public class LivreurRepository : Repository<Livreur>, ILivreurRepository
{
    public LivreurRepository(AppDbContext context) : base(context) { }
    public override async Task<IEnumerable<Livreur>> GetAllAsync()
        => await Context.Livreurs.Include(l => l.Vehicule).Include(l => l.Colis).ToListAsync();
}
