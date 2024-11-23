using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZleceniaProdukcyjneController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZleceniaProdukcyjneController> _logger;

        public ZleceniaProdukcyjneController(ApplicationDbContext context, ILogger<ZleceniaProdukcyjneController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ZleceniaProdukcyjne>>> GetZleceniaProdukcyjneAsync()
        {
            return await _context.ZleceniaProdukcyjne.OrderByDescending(e => e.DataZapisu).ToListAsync();
        }

        [HttpGet("ostrok")]
        public async Task<ActionResult<List<ZleceniaProdukcyjne>>> GetZleceniaProdukcyjneYearAsync()
        {
            var rokTemu = DateTime.Now.AddMonths(-12);  // Ostatnie 12 miesięcy
            var data = await _context.ZleceniaProdukcyjne
                                 .Where(c => c.DataZapisu > rokTemu)  // Filtruj zlecenia z ostatniego roku
                                 .OrderByDescending(e => e.DataZapisu)
                                 .ToListAsync();
            return data;
        }

        [HttpPost]
        public async Task<ActionResult> AddZlecenieProdukcyjneAsync(ZleceniaProdukcyjne zlecenie)
        {
            try
            {
                _context.ZleceniaProdukcyjne.Add(zlecenie);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania zlecenia produkcyjnego.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateZlecenieProdukcyjneAsync(long id, ZleceniaProdukcyjne zlecenie)
        {
            if (id != zlecenie.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(zlecenie).State = EntityState.Modified;

            try
            {
                // Pobierz aktualny stan zlecenia z bazy danych
                var existingZlecenie = await _context.ZleceniaProdukcyjne.AsNoTracking().FirstOrDefaultAsync(z => z.Id == id);
                if (existingZlecenie != null)
                {
                    if (!AreZleceniaEqual(existingZlecenie, zlecenie))
                    {
                        // Utwórz nowy rekord w tabeli ZleceniaProdukcyjneZmianyStatusu
                        var zmianaStatusu = new ZleceniaProdukcyjneZmianyStatusu
                        {
                            RowId = Guid.NewGuid().ToString(),
                            RowIdZlecenia = zlecenie.RowId.ToString(),
                            TypZamowienia = zlecenie.TypZamowienia,
                            NumerZamowienia = zlecenie.NumerZamowienia,
                            DataZapisu = DateTime.Now,
                            OstatniaZmiana = "Zmiana: " + DateTime.Now.ToLongDateString(),
                            KtoZapisal = zlecenie.KtoZapisal,
                            NumerZlecenia = zlecenie.NumerZlecenia,
                            Klient = zlecenie.Klient,
                            Miejscowosc = zlecenie.Miejscowosc,
                            Adres = zlecenie.Adres,
                            Telefon = zlecenie.Telefon,
                            Email = zlecenie.Email,
                            NazwaProduktu = zlecenie.NazwaProduktu,
                            Ilosc = zlecenie.Ilosc,
                            Wartosc = zlecenie.Wartosc,
                            DataProdukcji = zlecenie.DataProdukcji,
                            DataWysylki = zlecenie.DataWysylki,
                            DataMontazu = zlecenie.DataMontazu,
                            ZlecenieWewnatrzne = false,
                            DataGotowosci = zlecenie.DataGotowosci,
                            DataRozpProdukcji = zlecenie.DataRozpProdukcji,
                            NumerUmowy = zlecenie.NumerUmowy,
                            JednostkiNaZlecenie = zlecenie.JednostkiNaZlecenie,
                            KodProduktu = zlecenie.KodProduktu,
                            Tags = zlecenie.Tags,
                            NazwaProduktu2 = zlecenie.NazwaProduktu2,
                        };

                        // Dodaj rekord do kontekstu
                        _context.ZleceniaProdukcyjneZmianyStatusu.Add(zmianaStatusu);

                    }
                }

                // Zapisz zmiany
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ZlecenieProdukcyjneExists(id))
                {
                    return NotFound("Nie znaleziono zlecenia o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji zlecenia produkcyjnego.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        private bool AreZleceniaEqual(ZleceniaProdukcyjne existing, ZleceniaProdukcyjne updated)
        {
            return existing.TypZamowienia == updated.TypZamowienia &&
                   existing.NumerZamowienia == updated.NumerZamowienia &&
                   existing.Klient == updated.Klient &&
                   existing.Miejscowosc == updated.Miejscowosc &&
                   existing.Adres == updated.Adres &&
                   existing.Telefon == updated.Telefon &&
                   existing.Email == updated.Email &&
                   existing.NazwaProduktu == updated.NazwaProduktu &&
                   existing.Ilosc == updated.Ilosc &&
                   existing.Wartosc == updated.Wartosc &&
                   existing.DataProdukcji == updated.DataProdukcji &&
                   existing.DataWysylki == updated.DataWysylki &&
                   existing.DataMontazu == updated.DataMontazu &&
                   existing.DataGotowosci == updated.DataGotowosci &&
                   existing.DataRozpProdukcji == updated.DataRozpProdukcji &&
                   existing.NumerUmowy == updated.NumerUmowy &&
                   existing.JednostkiNaZlecenie == updated.JednostkiNaZlecenie &&
                   existing.KodProduktu == updated.KodProduktu &&
                   existing.Tags == updated.Tags &&
                   existing.NazwaProduktu2 == updated.NazwaProduktu2;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteZlecenieProdukcyjneAsync(long id)
        {
            var zlecenie = await _context.ZleceniaProdukcyjne.FindAsync(id);
            if (zlecenie == null)
            {
                return NotFound("Nie znaleziono zlecenia o podanym ID.");
            }

            _context.ZleceniaProdukcyjne.Remove(zlecenie);
            await _context.SaveChangesAsync();

            // Wyświetl numer zamówienia w konsoli
            Console.WriteLine(zlecenie.NumerZamowienia);

            return Ok("Zlecenie zostało pomyślnie usunięte.");
        }

        private bool ZlecenieProdukcyjneExists(long id)
        {
            return _context.ZleceniaProdukcyjne.Any(e => e.Id == id);
        }

        [HttpPost("zmien-date-produkcji")]
        public async Task<IActionResult> ZmienDateProdukcji(string rowId, [FromBody] DateTime nowaDataProdukcji)
        {
            var result = await _context.ZmienDateProdukcji(rowId, nowaDataProdukcji);
            if (result)
            {
                return Ok("Uwagi zaktualizowane pomyślnie.");
            }
            else
            {
                return NotFound("Nie znaleziono rekordu o podanym ID.");
            }
        }

        [HttpPost("zmien-date-rozpoczecia-produkcji")]
        public async Task<IActionResult> ZmienDateRozpoczeciaProdukcji(string rowId, string rowIdLinia, [FromBody] DateTime nowaDataProdukcji)
        {
            var result = await _context.ZmienDateRozpoczeciaProdukcji(rowId, rowIdLinia, nowaDataProdukcji);
            if (result)
            {
                return Ok("Uwagi zaktualizowane pomyślnie.");
            }
            else
            {
                return NotFound("Nie znaleziono rekordu o podanym ID.");
            }
        }
        // Możesz dodać dodatkowe metody, np. delete, jeśli to konieczne
    }

}
