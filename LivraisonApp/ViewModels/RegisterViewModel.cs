using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace LivraisonApp.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "L'e-mail est requis"), EmailAddress]
    [Display(Name = "E-mail")]
    public string Login { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    [Display(Name = "Mot de passe")]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password), ErrorMessage = "Les mots de passe ne correspondent pas")]
    [Display(Name = "Confirmer le mot de passe")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le rôle est requis")]
    public string Role { get; set; } = "Client";

    [Required(ErrorMessage = "Le nom est requis")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le prénom est requis")]
    [Display(Name = "Prénom")]
    public string Prenom { get; set; } = string.Empty;

    [Required(ErrorMessage = "La date de naissance est requise"), DataType(DataType.Date)]
    [Display(Name = "Date de naissance")]
    public DateTime? DateNaissance { get; set; }

    [Required(ErrorMessage = "L'adresse est requise")]
    public string Adresse { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le numéro de téléphone est requis"), Phone]
    [Display(Name = "Téléphone")]
    public string Telephone { get; set; } = string.Empty;

    [Display(Name = "Photo de profil")]
    public IFormFile? Photo { get; set; }

    // Champs spécifiques Livreur
    [Display(Name = "CIN (livreur)")]
    public string? CIN { get; set; }

    [Display(Name = "Ville")]
    public string? Ville { get; set; }
}
