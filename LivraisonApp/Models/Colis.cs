using System.ComponentModel.DataAnnotations;

namespace LivraisonApp.Models;

public class Colis
{
    public int Id { get; set; }
    public DateTime DateLivraison { get; set; }

    [Range(0, double.MaxValue)]
    public double Montant { get; set; }

    public double Poids { get; set; }
    public double Volume { get; set; }
    public string? Libelle { get; set; }

    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public int LivreurId { get; set; }
    public Livreur? Livreur { get; set; }
}
