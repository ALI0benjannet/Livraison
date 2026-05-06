using LivraisonApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Vehicule> Vehicules => Set<Vehicule>();
    public DbSet<Camion> Camions => Set<Camion>();
    public DbSet<Voiture> Voitures => Set<Voiture>();
    public DbSet<Livreur> Livreurs => Set<Livreur>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Colis> Colis => Set<Colis>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Vehicule>()
            .HasDiscriminator<string>("VehiculeType")
            .HasValue<Camion>("Camion")
            .HasValue<Voiture>("Voiture");

        builder.Entity<Vehicule>(entity =>
        {
            entity.Property(v => v.Couleur).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Marque).IsRequired().HasMaxLength(50);
            entity.Property(v => v.Matricule).IsRequired().HasMaxLength(20);
            entity.HasIndex(v => v.Matricule).IsUnique();
        });

        builder.Entity<Livreur>(entity =>
        {
            entity.Property(l => l.CIN).IsRequired();
            entity.HasIndex(l => l.CIN).IsUnique();
            entity.HasOne(l => l.Vehicule)
                .WithMany()
                .HasForeignKey(l => l.VehiculeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        builder.Entity<Client>(entity =>
        {
            entity.Property(c => c.Nom).IsRequired();
        });

        builder.Entity<Colis>(entity =>
        {
            entity.HasOne(c => c.Client)
                .WithMany(c => c.Colis)
                .HasForeignKey(c => c.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Livreur)
                .WithMany(l => l.Colis)
                .HasForeignKey(c => c.LivreurId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(c => c.Statut)
                .HasConversion<string>()
                .HasDefaultValue(StatutColis.EnAttente)
                .IsRequired();

            entity.Property(c => c.Libelle).HasMaxLength(200);
            entity.Property(c => c.Montant).HasPrecision(18, 2);
            entity.Property(c => c.Poids).HasPrecision(10, 3);
            entity.Property(c => c.Volume).HasPrecision(10, 3);
        });
    }
}
