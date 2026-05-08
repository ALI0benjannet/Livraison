using LivraisonApp.Models;

namespace LivraisonApp.ViewModels;

public class ColisSearchViewModel
{
    public float?       PrixMin       { get; set; }
    public float?       PrixMax       { get; set; }
    public DateTime?    DateLivraison { get; set; }
    public string?      Libelle       { get; set; }
    public string?      Ville         { get; set; }
    public string?      Livreur       { get; set; }
    public StatutColis? Statut        { get; set; }
}

