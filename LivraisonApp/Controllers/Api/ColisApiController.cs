using LivraisonApp.Data;
using LivraisonApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Controllers.Api;

[Route("api/colis")]
[ApiController]
[Authorize]
public class ColisApiController : ControllerBase
{
    private readonly AppDbContext _context;
    public ColisApiController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _context.Colis.Include(c => c.Client).Include(c => c.Livreur).ToListAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var entity = await _context.Colis.Include(c => c.Client).Include(c => c.Livreur).FirstOrDefaultAsync(c => c.Id == id);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchByPrix([FromQuery] double prix)
        => Ok(await _context.Colis.Where(c => c.Montant >= prix).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Colis colis)
    {
        _context.Colis.Add(colis);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = colis.Id }, colis);
    }
}
