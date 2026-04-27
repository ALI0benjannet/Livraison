using LivraisonApp.Models;

namespace LivraisonApp.Interfaces;

public interface IColisRepository : IRepository<Colis>
{
    Task<IEnumerable<Colis>> SearchByPrix(double? min, double? max);
    Task<IEnumerable<Colis>> SearchByLibelle(string? libelle);
}
