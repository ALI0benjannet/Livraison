using System.ComponentModel.DataAnnotations;

namespace LivraisonApp.Models;

public class Client
{
    public int Id { get; set; }
    public string? CodePostal { get; set; }

    [Required]
    public string Nom { get; set; } = string.Empty;

    public string? Prenom { get; set; }
    public string? Ville { get; set; }
    public List<Colis> Colis { get; set; } = new();
}
