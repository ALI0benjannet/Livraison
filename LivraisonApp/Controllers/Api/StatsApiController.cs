using LivraisonApp.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Controllers.Api;

[Route("api/stats")]
[ApiController]
[Authorize]
public class StatsApiController : ControllerBase
{
    private readonly AppDbContext _context;
    public StatsApiController(AppDbContext context) => _context = context;

    [HttpGet("total-colis")]
    public async Task<IActionResult> TotalColis() => Ok(new { total = await _context.Colis.CountAsync() });

    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue() => Ok(new { revenue = await _context.Colis.SumAsync(c => c.Montant) });
}
