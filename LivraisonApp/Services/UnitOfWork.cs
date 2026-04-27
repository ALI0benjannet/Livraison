using LivraisonApp.Data;
using LivraisonApp.Interfaces;

namespace LivraisonApp.Services;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    public IColisRepository Colis { get; }
    public IClientRepository Clients { get; }
    public ILivreurRepository Livreurs { get; }
    public IVehiculeRepository Vehicules { get; }

    public UnitOfWork(
        AppDbContext context,
        IColisRepository colisRepository,
        IClientRepository clientRepository,
        ILivreurRepository livreurRepository,
        IVehiculeRepository vehiculeRepository)
    {
        _context = context;
        Colis = colisRepository;
        Clients = clientRepository;
        Livreurs = livreurRepository;
        Vehicules = vehiculeRepository;
    }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
}
