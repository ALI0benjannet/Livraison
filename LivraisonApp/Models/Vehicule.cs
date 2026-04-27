using System.ComponentModel.DataAnnotations;

namespace LivraisonApp.Models;

public abstract class Vehicule
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Couleur { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Marque { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Matricule { get; set; } = string.Empty;

    public int VitesseLimite { get; set; }
}

public class Camion : Vehicule
{
    public int Capacite { get; set; }
    public int NbrEssieux { get; set; }
}

public class Voiture : Vehicule
{
    public int NbrPlaces { get; set; }
}
