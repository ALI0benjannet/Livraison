using Microsoft.AspNetCore.Identity;

namespace LivraisonApp.Models;

public class ApplicationUser : IdentityUser
{
    public string Role { get; set; } = "User";
}
