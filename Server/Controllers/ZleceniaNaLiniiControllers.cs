using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GEORGE.Shared.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;

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
                .Where(zp => zp.JednostkiNaZlecenie > 0 && zp.DataProdukcji > DateTime.Now.AddMonths(-6) && !zp.ZlecZrealizowane)
                .OrderBy(x => x.DataZapisu)
                .ToList();

            // Pobierz wszystkie zlecenia produkcyjne wewnętrzne
            var zleceniaProdukcyjneWew = _context.ZleceniaProdukcyjneWew
                .Where(zw => zw.JednostkiNaZlecenie > 0 && zw.DataProdukcji > DateTime.Now.AddMonths(-6) && !zw.ZlecZrealizowane)
                .OrderBy(x => x.DataZapisu)
                .ToList();

            var zleceniaProdukcyjneDto = zleceniaProdukcyjne.Select(zp => new ZleceniaProdukcyjneDto
            {

                Id = zp.Id,
                RowId = zp.RowId,
                TypZamowienia = zp.TypZamowienia,
                NumerZamowienia = zp.NumerZamowienia,
                NumerUmowy = zp.NumerUmowy,
                DataProdukcji = zp.DataProdukcji,
                DataWysylki = zp.DataWysylki,
                DataMontazu = zp.DataMontazu,
                Klient = zp.Klient,
                Adres = zp.Adres,
                Miejscowosc = zp.Miejscowosc,
                Telefon = zp.Telefon,
                Email = zp.Email,
                NazwaProduktu = zp.NazwaProduktu,
                NazwaProduktu2 = zp.NazwaProduktu2,
                KodProduktu = zp.KodProduktu,
                Ilosc = zp.Ilosc,
                Wartosc = zp.Wartosc,
                DataZapisu = zp.DataZapisu,
                KtoZapisal = zp.KtoZapisal,
                OstatniaZmiana = zp.OstatniaZmiana,
                Tags = zp.Tags,
                JednostkiNaZlecenie = zp.JednostkiNaZlecenie,
                ZlecZrealizowane = zp.ZlecZrealizowane,
                ZlecenieWewnetrzne = false  // Ustawiamy false, ponieważ to nie jest zlecenie wewnętrzne
            }).ToList();

            var zleceniaProdukcyjneWewDto = zleceniaProdukcyjneWew.Select(zw => new ZleceniaProdukcyjneDto
            {
                Id = zw.Id,
                RowId = zw.RowId,
                TypZamowienia = zw.TypZamowienia,
                NumerZamowienia = zw.NumerZamowienia,
                NumerUmowy = zw.NumerUmowy,
                DataProdukcji = zw.DataProdukcji,
                DataWysylki = zw.DataWysylki,
                DataMontazu = zw.DataMontazu,
                Klient = zw.Klient,
                Adres = zw.Adres,
                Miejscowosc = zw.Miejscowosc,
                Telefon = zw.Telefon,
                Email = "biuro@george.pl",
                NazwaProduktu = zw.NazwaProduktu,
                NazwaProduktu2 = zw.NazwaProduktu2,
                KodProduktu = zw.KodProduktu,
                Ilosc = zw.Ilosc,
                Wartosc = zw.Wartosc,
                DataZapisu = zw.DataZapisu,
                KtoZapisal = zw.KtoZapisal,
                OstatniaZmiana = zw.OstatniaZmiana,
                Tags = zw.Tags,
                JednostkiNaZlecenie = zw.JednostkiNaZlecenie,
                ZlecZrealizowane = zw.ZlecZrealizowane,
                ZlecenieWewnetrzne = true  // Ustawiamy true, ponieważ to jest zlecenie wewnętrzne
            }).ToList();

            // Połącz zlecenia produkcyjne z wewnętrznymi
            var wszystkieZleceniaProdukcyjneDto = zleceniaProdukcyjneDto.Concat(zleceniaProdukcyjneWewDto).ToList();


            // Pobierz wszystkie zlecenia na linii
            var zleceniaNaLinii = _context.ZleceniaNaLinii.ToList();

            // Pobierz linię produkcyjną na podstawie rowIdLinii
            var liniaProdukcyjna = _context.LinieProdukcyjne.FirstOrDefault(lp => lp.RowId == rowIdLinii);
            if (liniaProdukcyjna == null)
            {
                return new List<DaneDoPlanowaniaViewModel>();
            }

            if (juzZapisane == "NIE")
            {
                // Filtruj zlecenia produkcyjne, aby wykluczyć te, które mają RowId równe RowIdZleceniaProdukcyjne w ZleceniaNaLinii
                var filteredZleceniaProdukcyjne = wszystkieZleceniaProdukcyjneDto
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
                        ZleceniaProdukcyjneDto = zp,
                        Wyrob = zp.NazwaProduktu,
                        NumerZlecenia = zp.NumerZamowienia,
                        DomyslnyCzasProdukcji = (int)czasProdukcjiWHours, // Przypisz czas produkcji w godzinach
                        RowIdLiniiProdukcyjnej = rowIdLinii, // Możesz dostosować to pole w zależności od wymagań
                        ZlecenieWewnetrzne = zp.ZlecenieWewnetrzne
                    };
                }).ToList();

                return daneDoPlanowania;
            }
            else
            {
                // Filtruj zlecenia produkcyjne, aby wykluczyć te, które mają RowId równe RowIdZleceniaProdukcyjne w ZleceniaNaLinii
                var filteredZleceniaProdukcyjne = wszystkieZleceniaProdukcyjneDto
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
                        PlanowanaDataRozpoczeciaProdukcji = zp.DataProdukcji < DateTime.Now ? DateTime.Now.AddDays(7 * 8) : zp.DataProdukcji, // Możesz dostosować to pole w zależności od wymagań
                        ZleceniaProdukcyjneDto = zp,
                        Wyrob = zp.NazwaProduktu,
                        NumerZlecenia = zp.NumerZamowienia,
                        DomyslnyCzasProdukcji = (int)czasProdukcjiWHours, // Przypisz czas produkcji w godzinach
                        RowIdLiniiProdukcyjnej = rowIdLinii, // Możesz dostosować to pole w zależności od wymagań
                        ZlecenieWewnetrzne = zp.ZlecenieWewnetrzne
                    };
                }).ToList();

                return daneDoPlanowania;
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddZleceniaNaLinii([FromBody] List<ZleceniaNaLinii> zleceniaNaLinii)
        {
            try
            {
                if (zleceniaNaLinii == null || !zleceniaNaLinii.Any())
                {
                    return BadRequest("Lista zleceń na linii jest pusta.");
                }

                var existingZlecenia = await _context.ZleceniaNaLinii
                    .ToListAsync();  // Pobierz wszystkie rekordy na raz

                // Porównanie z wykorzystaniem Linq po stronie klienta
                var newZlecenia = zleceniaNaLinii
                    .Where(z => !existingZlecenia.Any(ez => ez.RowIdLinieProdukcyjne == z.RowIdLinieProdukcyjne && ez.RowIdZleceniaProdukcyjne == z.RowIdZleceniaProdukcyjne))
                    .ToList();

                if (!newZlecenia.Any())
                {
                    return BadRequest("Żadne z nowych zleceń nie zostało dodane, ponieważ wszystkie już istnieją.");
                }

                await _context.ZleceniaNaLinii.AddRangeAsync(newZlecenia);
                await _context.SaveChangesAsync();

                return Ok("Zlecenia zostały dodane.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Błąd. {ex.Message}");
            }
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
