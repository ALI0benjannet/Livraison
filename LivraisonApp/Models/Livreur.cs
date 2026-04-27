using System.ComponentModel.DataAnnotations;

namespace LivraisonApp.Models;

public class Livreur
{
    public int Id { get; set; }

    [Required]
    public string CIN { get; set; } = string.Empty;

    public string? CodePostal { get; set; }
    public string? RaisonSocial { get; set; }
    public string? Ville { get; set; }

    public int? VehiculeId { get; set; }
    public Vehicule? Vehicule { get; set; }

    public List<Colis> Colis { get; set; } = new();
}
