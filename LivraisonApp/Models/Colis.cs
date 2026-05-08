using System.ComponentModel.DataAnnotations;

namespace LivraisonApp.Models;

public enum StatutColis
{
    [Display(Name = "En attente")] EnAttente = 0,
    [Display(Name = "En cours")]   EnCours   = 1,
    [Display(Name = "Livr�")]      Livre     = 2
}

public class Colis
{
    public int Id { get; set; }

    [Required]
    public DateTime DateLivraison { get; set; }

    [Range(0, float.MaxValue, ErrorMessage = "Le montant doit être positif.")]
    public float Montant { get; set; }

    [Range(0, float.MaxValue, ErrorMessage = "Le poids doit être positif.")]
    public float Poids { get; set; }

    [Range(0, float.MaxValue, ErrorMessage = "Le volume doit être positif.")]
    public float Volume { get; set; }

    [MaxLength(200)]
    public string? Libelle { get; set; }

    public StatutColis Statut { get; set; } = StatutColis.EnAttente;

    [Required]
    public int ClientId { get; set; }
    public Client? Client { get; set; }

    [Required]
    public int LivreurId { get; set; }
    public Livreur? Livreur { get; set; }
}
