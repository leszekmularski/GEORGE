using GEORGE.Server;
using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
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


    [HttpGet("powiazaniajuzzaznaczone/{rowIdKonfModele}")]
    public async Task<ActionResult<MVCKonfModele>> GetPowiazaniajuzaznaczone(Guid rowIdKonfModele)
    {
        var model = await _context.KonfModele
            .FirstOrDefaultAsync(m => m.RowId == rowIdKonfModele);

        var powiazaneElementy = await _context.KonfModeleElementy
            .Where(k => k.RowIdKonfModele == rowIdKonfModele)
            .ToListAsync();

        var konfSystem = await _context.KonfSystem
            .Where(k => k.RowId == rowIdKonfModele)  // Zmodyfikuj w zależności od zależności w bazie
            .ToListAsync();

        if (model == null)
            return NotFound();

        return new MVCKonfModele
        {
            KonfModele = new List<KonfModele> { model }, // Pojedynczy obiekt zamieniony na listę
            KonfModeleElementy = powiazaneElementy,
            KonfSystem = konfSystem, // Dodajemy listę KonfSystem
        };
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
