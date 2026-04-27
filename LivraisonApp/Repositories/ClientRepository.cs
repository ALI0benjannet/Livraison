using LivraisonApp.Data;
using LivraisonApp.Interfaces;
using LivraisonApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Repositories;

public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(AppDbContext context) : base(context) { }
    public override async Task<IEnumerable<Client>> GetAllAsync() => await Context.Clients.Include(c => c.Colis).ToListAsync();
}
