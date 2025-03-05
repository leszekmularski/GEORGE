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

    [HttpGet("powiazaniajuzzaznaczone/{rowIdKonfModele}")]
    public async Task<ActionResult<MVCKonfModele>> GetPowiazaniajuzaznaczone(Guid rowIdKonfModele)
    {
        // Pobranie modelu z KonfModele
        var model = await _context.KonfModele
            .FirstOrDefaultAsync(m => m.RowId == rowIdKonfModele);

        // Pobranie powiązanych elementów z KonfModeleElementy
        var powiazaneElementy = await _context.KonfModeleElementy
            .Where(k => k.RowIdKonfModele == rowIdKonfModele)
            .ToListAsync();

        // Pobranie RowIdKonfModele z powiązanych elementów
        var rowIdsKonfModele = powiazaneElementy.Select(k => k.RowIdElement).ToList();

        // Pobranie wszystkich KonfSystem, które pasują do RowIdKonfModele
        var konfSystem = await _context.KonfSystem
            .Where(k => rowIdsKonfModele.Contains(k.RowId)) // Sprawdzamy, czy RowId KonfSystem znajduje się w rowIdsKonfModele
            .ToListAsync();

        // Sprawdzamy, czy model istnieje
        if (model == null)
            return NotFound();

        // Zwracamy obiekt MVCKonfModele z odpowiednimi danymi
        return new MVCKonfModele
        {
            KonfModele = new List<KonfModele> { model }, // Pojedynczy obiekt zamieniony na listę
            KonfModeleElementy = powiazaneElementy,
            KonfSystem = konfSystem, // Dodajemy listę KonfSystem
        };
    }

    [HttpGet("powiazania/{rowIdKonfModele}")]
    public async Task<ActionResult<List<KonfModeleElementy>>> GetPowiazaniaAll(string rowIdKonfModele)
    {
        var powiazania = await _context.KonfModeleElementy
            .Where(k => k.RowIdKonfModele.ToString() == rowIdKonfModele)
            .ToListAsync();

        //if (powiazania == null || powiazania.Count == 0)
        //{
        //    return NotFound($"Brak powiązań dla RowIdKonfModele: {rowIdKonfModele}");
        //}

        return Ok(powiazania);
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
