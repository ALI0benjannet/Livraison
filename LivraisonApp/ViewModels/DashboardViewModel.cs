namespace LivraisonApp.ViewModels;

public class DashboardViewModel
{
    public int    TotalColis     { get; set; }
    public int    TotalClients   { get; set; }
    public int    TotalLivreurs  { get; set; }
    public int    TotalVehicules { get; set; }
    public float TotalRevenue   { get; set; }
    public int    ColisEnAttente { get; set; }
    public int    ColisEnCours   { get; set; }
    public int    ColisLivres    { get; set; }
}

