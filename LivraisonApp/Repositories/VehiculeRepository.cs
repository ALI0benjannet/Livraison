using LivraisonApp.Data;
using LivraisonApp.Interfaces;
using LivraisonApp.Models;

namespace LivraisonApp.Repositories;

public class VehiculeRepository : Repository<Vehicule>, IVehiculeRepository
{
    public VehiculeRepository(AppDbContext context) : base(context) { }
}
