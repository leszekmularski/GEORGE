using GEORGE.Server;
using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/konfmodeleelementy")]
[ApiController]
public class KonfModeleElementyController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public KonfModeleElementyController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpGet("powiazania/{rowIdKonfModele}")]
    public async Task<ActionResult<List<KonfModeleElementy>>> GetPowiazania(Guid rowIdKonfModele)
    {
        return await _context.KonfModeleElementy
            .Where(k => k.RowIdKonfModele == rowIdKonfModele).ToListAsync();
    }

    [HttpPost("powiazania")]
    public async Task<IActionResult> DodajPowiazania(List<KonfModeleElementy> powiazania)
    {
        _context.KonfModeleElementy.AddRange(powiazania);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("powiazania/usun")]
    public async Task<IActionResult> UsunPowiazania(List<int> ids)
    {
        var powiazania = _context.KonfModeleElementy.Where(k => ids.Contains(k.Id));
        _context.KonfModeleElementy.RemoveRange(powiazania);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
