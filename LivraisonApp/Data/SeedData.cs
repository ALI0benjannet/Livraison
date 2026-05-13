using LivraisonApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Roles
        foreach (var role in new[] { "Admin", "Client", "Livreur" })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        // Admin account
        const string adminEmail = "admin@livraison.local";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail, Email = adminEmail, EmailConfirmed = true, Role = "Admin",
                Nom = "Admin", Prenom = "Système", PhoneNumber = "0000000000", Adresse = "Siège"
            };
            var r = await userManager.CreateAsync(admin, "Admin123!");
            if (r.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Demo data (only if tables are empty)
        if (await context.Vehicules.AnyAsync()) return;

        var camion1 = new Camion { Couleur = "Blanc", Marque = "Mercedes", Matricule = "123-TUN-16", VitesseLimite = 90, Capacite = 10, NbrEssieux = 3 };
        var camion2 = new Camion { Couleur = "Gris", Marque = "Volvo", Matricule = "456-TUN-31", VitesseLimite = 85, Capacite = 20, NbrEssieux = 4 };
        var voiture1 = new Voiture { Couleur = "Rouge", Marque = "Renault", Matricule = "789-TUN-06", VitesseLimite = 130, NbrPlaces = 5 };
        var voiture2 = new Voiture { Couleur = "Bleu", Marque = "Peugeot", Matricule = "321-TUN-25", VitesseLimite = 120, NbrPlaces = 5 };
        context.Camions.AddRange(camion1, camion2);
        context.Voitures.AddRange(voiture1, voiture2);
        await context.SaveChangesAsync();

        var livreur1 = new Livreur { CIN = "10000001", Nom = "Ben Salah", Prenom = "Ahmed", RaisonSocial = "Express Livraison", Ville = "Tunis", CodePostal = "1000", VehiculeId = camion1.Id };
        var livreur2 = new Livreur { CIN = "10000002", Nom = "Trabelsi", Prenom = "Mohamed", RaisonSocial = "Rapide Colis", Ville = "Sfax", CodePostal = "3000", VehiculeId = voiture1.Id };
        var livreur3 = new Livreur { CIN = "10000003", Nom = "Gharbi", Prenom = "Slim", RaisonSocial = "Trans Maghreb", Ville = "Bizerte", CodePostal = "7000", VehiculeId = camion2.Id };
        context.Livreurs.AddRange(livreur1, livreur2, livreur3);
        await context.SaveChangesAsync();

        var client1 = new Client { Nom = "Ben Ali",  Prenom = "Mohamed", Ville = "Tunis",   CodePostal = "1000" };
        var client2 = new Client { Nom = "Zaouali",  Prenom = "Fatma",   Ville = "Sfax",    CodePostal = "3000" };
        var client3 = new Client { Nom = "Hamdi",    Prenom = "Karim",   Ville = "Sousse",  CodePostal = "4000" };
        var client4 = new Client { Nom = "Bouazizi", Prenom = "Sara",    Ville = "Bizerte", CodePostal = "7000" };
        var client5 = new Client { Nom = "Mejri",    Prenom = "Yassine", Ville = "Gabès",   CodePostal = "6000" };
        context.Clients.AddRange(client1, client2, client3, client4, client5);
        await context.SaveChangesAsync();

        // Comptes Identity liés (démo)
        async Task CreateLinkedAsync(string email, string pwd, string role, string nom, string prenom, int? clientId, int? livreurId)
        {
            if (await userManager.FindByEmailAsync(email) is not null) return;
            var u = new ApplicationUser
            {
                UserName = email, Email = email, EmailConfirmed = true, Role = role,
                Nom = nom, Prenom = prenom, PhoneNumber = "0550000000",
                Adresse = "Adresse démo", DateNaissance = new DateTime(1990, 1, 1),
                ClientId = clientId, LivreurId = livreurId
            };
            var rr = await userManager.CreateAsync(u, pwd);
            if (rr.Succeeded) await userManager.AddToRoleAsync(u, role);
        }

        await CreateLinkedAsync("client@livraison.local", "Client123!", "Client", client1.Nom, client1.Prenom!, client1.Id, null);
        await CreateLinkedAsync("livreur@livraison.local", "Livreur123!", "Livreur", "Express", "Livraison", null, livreur1.Id);

        var rng = new Random(42);
        var libelles = new[] { "Électronique", "Vêtements", "Alimentaire", "Livres", "Mobilier", "Cosmétiques", "Médicaments", "Jouets" };
        var now = DateTime.Now;
        var colis = new List<Colis>();
        var clients = new[] { client1, client2, client3, client4, client5 };
        var livreurs = new[] { livreur1, livreur2, livreur3 };

        for (int m = -11; m <= 0; m++)
        {
            int count = rng.Next(3, 8);
            for (int i = 0; i < count; i++)
            {
                var date = new DateTime(now.Year, now.Month, 1).AddMonths(m).AddDays(rng.Next(0, 28));
                colis.Add(new Colis
                {
                    DateLivraison = date,
                    Montant = (float)Math.Round(rng.Next(500, 15000) + rng.NextDouble(), 2),
                    Poids   = (float)Math.Round(rng.Next(1, 50) + rng.NextDouble(), 2),
                    Volume  = (float)Math.Round(rng.NextDouble() * 2, 3),
                    Libelle = libelles[rng.Next(libelles.Length)],
                    Statut = (StatutColis)rng.Next(0, 3),
                    ClientId = clients[rng.Next(clients.Length)].Id,
                    LivreurId = livreurs[rng.Next(livreurs.Length)].Id
                });
            }
        }
        context.Colis.AddRange(colis);
        await context.SaveChangesAsync();
    }
}
