using Microsoft.AspNetCore.Identity;

namespace LivraisonApp.Models;

public class ApplicationUser : IdentityUser
{
    public string Role { get; set; } = "Client";

    // Profil
    public string? Nom { get; set; }
    public string? Prenom { get; set; }
    public DateTime? DateNaissance { get; set; }
    public string? PhotoUrl { get; set; }
    public string? Adresse { get; set; }

    // Lien vers entité métier (selon rôle)
    public int? ClientId { get; set; }
    public Client? Client { get; set; }

    public int? LivreurId { get; set; }
    public Livreur? Livreur { get; set; }
}
