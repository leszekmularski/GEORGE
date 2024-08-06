using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GEORGE.Shared.ViewModels;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZleceniaNaLiniiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZleceniaNaLiniiController> _logger;

        public ZleceniaNaLiniiController(ApplicationDbContext context, ILogger<ZleceniaNaLiniiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ZleceniaNaLinii>>> GetLinieProdukcyjneAsync()
        {
            return await _context.ZleceniaNaLinii.OrderByDescending(e => e.DataZapisu).ToListAsync();
        }

        [HttpGet("GetDaneDoPlanowania/{rowIdLinii}/{juzZapisane}")]
        public List<DaneDoPlanowaniaViewModel> GetDaneDoPlanowania(string rowIdLinii, string juzZapisane)
        {
            // Pobierz wszystkie zlecenia produkcyjne
            var zleceniaProdukcyjne = _context.ZleceniaProdukcyjne
            .Where(zp => zp.JednostkiNaZlecenie > 0)
            .ToList();

            // Pobierz wszystkie zlecenia na linii
            var zleceniaNaLinii = _context.ZleceniaNaLinii.ToList();

            // Pobierz linię produkcyjną na podstawie rowIdLinii
            var liniaProdukcyjna = _context.LinieProdukcyjne.FirstOrDefault(lp => lp.RowId == rowIdLinii);
            if (liniaProdukcyjna == null)
            {
                return new List<DaneDoPlanowaniaViewModel>();
            }

            if(juzZapisane == "NIE")
            {
                // Filtruj zlecenia produkcyjne, aby wykluczyć te, które mają RowId równe RowIdZleceniaProdukcyjne w ZleceniaNaLinii
                var filteredZleceniaProdukcyjne = zleceniaProdukcyjne
                    .Where(zp => !zleceniaNaLinii.Any(znl => znl.RowIdZleceniaProdukcyjne == zp.RowId && znl.RowIdLinieProdukcyjne == rowIdLinii))
                    .ToList();

                // Mapuj przefiltrowane zlecenia produkcyjne do modelu widoku
                var daneDoPlanowania = filteredZleceniaProdukcyjne.Select(zp =>
                {
                    // Oblicz czas na wykonanie zlecenia
                    var czasProdukcjiWDniach = (double)zp.JednostkiNaZlecenie / liniaProdukcyjna.DziennaZdolnoscProdukcyjna;
                    var czasProdukcjiWHours = czasProdukcjiWDniach * 24;

                    return new DaneDoPlanowaniaViewModel
                    {
                        PlanowanaDataRozpoczeciaProdukcji = DateTime.Now.AddDays(7 * 8), // Możesz dostosować to pole w zależności od wymagań
                        ZleceniaProdukcyjne = zp,
                        Wyrob = zp.NazwaProduktu,
                        NumerZlecenia = zp.NumerZamowienia,
                        DomyslnyCzasProdukcji = (int)czasProdukcjiWHours, // Przypisz czas produkcji w godzinach
                        RowIdLiniiProdukcyjnej = rowIdLinii // Możesz dostosować to pole w zależności od wymagań
                    };
                }).ToList();

                return daneDoPlanowania;
            }
            else
            {
                // Filtruj zlecenia produkcyjne, aby wykluczyć te, które mają RowId równe RowIdZleceniaProdukcyjne w ZleceniaNaLinii
                var filteredZleceniaProdukcyjne = zleceniaProdukcyjne
                    .Where(zp => zleceniaNaLinii.Any(znl => znl.RowIdLinieProdukcyjne == rowIdLinii))
                    .ToList();

                // Mapuj przefiltrowane zlecenia produkcyjne do modelu widoku
                var daneDoPlanowania = filteredZleceniaProdukcyjne.Select(zp =>
                {
                    // Oblicz czas na wykonanie zlecenia
                    var czasProdukcjiWDniach = (double)zp.JednostkiNaZlecenie / liniaProdukcyjna.DziennaZdolnoscProdukcyjna;
                    var czasProdukcjiWHours = czasProdukcjiWDniach * 24;

                    return new DaneDoPlanowaniaViewModel
                    {
                        PlanowanaDataRozpoczeciaProdukcji = DateTime.Now.AddDays(7 * 8), // Możesz dostosować to pole w zależności od wymagań
                        ZleceniaProdukcyjne = zp,
                        Wyrob = zp.NazwaProduktu,
                        NumerZlecenia = zp.NumerZamowienia,
                        DomyslnyCzasProdukcji = (int)czasProdukcjiWHours, // Przypisz czas produkcji w godzinach
                        RowIdLiniiProdukcyjnej = rowIdLinii // Możesz dostosować to pole w zależności od wymagań
                    };
                }).ToList();

                return daneDoPlanowania;
            }

        }


        [HttpPost]
        public async Task<IActionResult> AddZleceniaNaLinii([FromBody] List<ZleceniaNaLinii> zleceniaNaLinii)
        {
            if (zleceniaNaLinii == null || !zleceniaNaLinii.Any())
            {
                return BadRequest("Lista zleceń jest pusta.");
            }

            foreach (var zlecenie in zleceniaNaLinii)
            {
                _context.ZleceniaNaLinii.Add(zlecenie);
            }

            await _context.SaveChangesAsync();

            return Ok("Zlecenia zostały dodane.");
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateLinieProdukcyjneAsync(long id, ZleceniaNaLinii zl)
        {
            if (id != zl.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(zl).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!LinieProdukcyjneExists(id))
                {
                    return NotFound("Nie znaleziono zlecenia na linii o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji ZleceniaNaLinii .");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        private bool LinieProdukcyjneExists(long id)
        {
            return _context.ZleceniaNaLinii.Any(e => e.Id == id);
        }

        // Możesz dodać dodatkowe metody, np. delete, jeśli to konieczne

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteLinieProdukcyjneAsync(long id)
        {
            var zl = await _context.ZleceniaNaLinii.FindAsync(id);
            if (zl == null)
            {
                return NotFound("Nie znaleziono zlecenia o podanym ID.");
            }

            _context.ZleceniaNaLinii.Remove(zl);
            await _context.SaveChangesAsync();

            // Wyświetl numer zamówienia w konsoli
            Console.WriteLine("Usunołem zlec z dnia " + zl.DataZapisu);

            return Ok("Zlec. na linii usunięto.");
        }
    }
}
