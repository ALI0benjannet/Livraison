using LivraisonApp.Interfaces;

namespace LivraisonApp.Services;

public interface IUnitOfWork
{
    IColisRepository Colis { get; }
    IClientRepository Clients { get; }
    ILivreurRepository Livreurs { get; }
    IVehiculeRepository Vehicules { get; }
    Task<int> SaveChangesAsync();
}
