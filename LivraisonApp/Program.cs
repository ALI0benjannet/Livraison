using LivraisonApp.Data;
using LivraisonApp.Interfaces;
using LivraisonApp.Models;
using LivraisonApp.Repositories;
using LivraisonApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlite("Data Source=livraison.db"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(connectionString));
}

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath        = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan   = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.Name      = "LivraisonApp.Auth";
    options.Cookie.HttpOnly  = true;
    options.Cookie.SameSite  = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    // En dev (HTTP), ne pas exiger Secure sinon le cookie n'est jamais envoyé.
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? Microsoft.AspNetCore.Http.CookieSecurePolicy.None
        : Microsoft.AspNetCore.Http.CookieSecurePolicy.Always;
});

builder.Services.AddScoped<IColisRepository, ColisRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ILivreurRepository, LivreurRepository>();
builder.Services.AddScoped<IVehiculeRepository, VehiculeRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));
});

builder.Services.AddControllersWithViews();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    // Add ImageUrl column if it doesn't exist yet
    if (db.Database.ProviderName?.Contains("SqlServer") == true)
    {
        db.Database.ExecuteSqlRaw(
            "IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Colis') AND name = N'ImageUrl') " +
            "ALTER TABLE [Colis] ADD [ImageUrl] NVARCHAR(300) NULL");
        db.Database.ExecuteSqlRaw(
            "IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Livreurs') AND name = N'Nom') " +
            "ALTER TABLE [Livreurs] ADD [Nom] NVARCHAR(MAX) NULL");
        db.Database.ExecuteSqlRaw(
            "IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'Livreurs') AND name = N'Prenom') " +
            "ALTER TABLE [Livreurs] ADD [Prenom] NVARCHAR(MAX) NULL");
        // Renomme villes algériennes -> tunisiennes (idempotent)
        db.Database.ExecuteSqlRaw(@"
            UPDATE [Clients]  SET [Ville]='Tunis',       [CodePostal]='1000' WHERE [Ville]='Alger';
            UPDATE [Clients]  SET [Ville]='Sfax',        [CodePostal]='3000' WHERE [Ville]='Oran';
            UPDATE [Clients]  SET [Ville]='Sousse',      [CodePostal]='4000' WHERE [Ville]='Annaba';
            UPDATE [Clients]  SET [Ville]='Bizerte',     [CodePostal]='7000' WHERE [Ville]='Constantine';
            UPDATE [Clients]  SET [Ville]='Gabès',       [CodePostal]='6000' WHERE [Ville]='Blida';
            UPDATE [Livreurs] SET [Ville]='Tunis',       [CodePostal]='1000' WHERE [Ville]='Alger';
            UPDATE [Livreurs] SET [Ville]='Sfax',        [CodePostal]='3000' WHERE [Ville]='Oran';
            UPDATE [Livreurs] SET [Ville]='Sousse',      [CodePostal]='4000' WHERE [Ville]='Annaba';
            UPDATE [Livreurs] SET [Ville]='Bizerte',     [CodePostal]='7000' WHERE [Ville]='Constantine';
            UPDATE [Livreurs] SET [Ville]='Gabès',       [CodePostal]='6000' WHERE [Ville]='Blida';
            UPDATE [Vehicules] SET [Matricule]=REPLACE(REPLACE(REPLACE(REPLACE([Matricule],'-ALG-','-TUN-'),'-ORA-','-TUN-'),'-ANN-','-TUN-'),'-CON-','-TUN-')
                WHERE [Matricule] LIKE '%-ALG-%' OR [Matricule] LIKE '%-ORA-%' OR [Matricule] LIKE '%-ANN-%' OR [Matricule] LIKE '%-CON-%';
        ");
    }
    else
    {
        try { db.Database.ExecuteSqlRaw("ALTER TABLE \"Colis\" ADD COLUMN \"ImageUrl\" TEXT"); } catch { }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE \"Livreurs\" ADD COLUMN \"Nom\" TEXT"); } catch { }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE \"Livreurs\" ADD COLUMN \"Prenom\" TEXT"); } catch { }
    }
}
await SeedData.InitializeAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Account}/{action=Login}/{id?}");
app.Run();
