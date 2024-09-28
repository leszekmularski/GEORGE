using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProducenciPodwykonawcyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProducenciPodwykonawcyController> _logger;

        public ProducenciPodwykonawcyController(ApplicationDbContext context, ILogger<ProducenciPodwykonawcyController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProducenciPodwykonawcy>>> GetProducenciAsync()
        {
            return await _context.ProducenciPodwykonawcy.ToListAsync();
        }

        [HttpGet("{RowIdProducenta}")]
        public async Task<ActionResult<List<ElemetZamDoZlecenWithProducent>>> GetZamowieniaWithProducentAsync(string RowIdProducenta)
        {
            var zamowieniaWithProducent = await _context.ElemetZamDoZlecen
                .Where(e => e.RowIdProducent == RowIdProducenta) // Filtr na RowIdProducenta
                .Select(e => new ElemetZamDoZlecenWithProducent
                {
                    ElemetZamDoZlecen = e, // Przypisanie danych zamówienia

                    // Pobranie producenta (może być null)
                    ProducenciPodwykonawcy = _context.ProducenciPodwykonawcy
                        .FirstOrDefault(p => p.RowId == e.RowIdProducent),

                    // Pobranie producenta i miejscowości
                    ProducentIMiejscowosc = _context.ProducenciPodwykonawcy
                        .Where(p => p.RowId == e.RowIdProducent)
                        .Select(p => p.NazwaProducenta + " - " + p.Miejscowosc)
                        .FirstOrDefault(),

                    // Pobranie informacji o kliencie, zaczynając od ZleceniaProdukcyjne, a jeśli brak, szukamy w ZleceniaProdukcyjneWew
                    Klient = _context.ZleceniaProdukcyjne
                                .Where(z => z.RowId == e.RowIdZlecenia)
                                .Select(z => z.Klient)
                                .FirstOrDefault()
                                ?? _context.ZleceniaProdukcyjneWew
                                .Where(zw => zw.RowId == e.RowIdZlecenia)
                                .Select(zw => zw.Klient)
                                .FirstOrDefault(),

                    // Pobranie NumerZamowienia podobnie jak Klient
                    NumerZlecenia = _context.ZleceniaProdukcyjne
                                .Where(z => z.RowId == e.RowIdZlecenia)
                                .Select(z => z.NumerZamowienia)
                                .FirstOrDefault()
                                ?? _context.ZleceniaProdukcyjneWew
                                .Where(zw => zw.RowId == e.RowIdZlecenia)
                                .Select(zw => zw.NumerZamowienia)
                                .FirstOrDefault(),

                    DodatkowaInformacja = "Dane z dnia: " + DateTime.Now
                })
                .ToListAsync();

            if (zamowieniaWithProducent == null || zamowieniaWithProducent.Count == 0)
            {
                return NotFound(); // Zwróć 404 jeśli brak danych
            }

            return zamowieniaWithProducent;
        }

        [HttpPost]
        public async Task<ActionResult> AddProducencihAsync(ProducenciPodwykonawcy producenci)
        {
            try
            {
                _context.ProducenciPodwykonawcy.Add(producenci);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania Producenta.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateProducenciAsync(long id, ProducenciPodwykonawcy producenci)
        {
            if (id != producenci.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(producenci).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProducentExists(id))
                {
                    return NotFound("Nie znaleziono Producenta o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji ProducenciPodwykonawcy.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteProcentaAsync(long id)
        {
            var producenci = await _context.ProducenciPodwykonawcy.FindAsync(id);
            if (producenci == null)
            {
                return NotFound("Nie znaleziono Producenta o podanym ID.");
            }

            _context.ProducenciPodwykonawcy.Remove(producenci);

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas usuwania ProducenciPodwykonawcy.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        private bool ProducentExists(long id)
        {
            return _context.ProducenciPodwykonawcy.Any(e => e.Id == id);
        }

    }
}
