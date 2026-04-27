using System.ComponentModel.DataAnnotations;

namespace LivraisonApp.ViewModels;

public class RegisterViewModel
{
    [Required]
    public string Login { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "User";
}
