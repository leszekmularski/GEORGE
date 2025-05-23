﻿using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZleceniaProdukcyjneWewController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZleceniaProdukcyjneWewController> _logger;

        public ZleceniaProdukcyjneWewController(ApplicationDbContext context, ILogger<ZleceniaProdukcyjneWewController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ZleceniaProdukcyjneWew>>> GetZleceniaProdukcyjneWewAsync()
        {
            return await _context.ZleceniaProdukcyjneWew.OrderByDescending(e => e.DataZapisu).ToListAsync();
        }

        [HttpGet("ostrok")]
        public async Task<ActionResult<List<ZleceniaProdukcyjneWew>>> GetZleceniaProdukcyjneYearAsync()
        {
            var rokTemu = DateTime.Now.AddMonths(-12);  // Ostatnie 12 miesięcy
            return await _context.ZleceniaProdukcyjneWew
                                 .Where(c => c.DataZapisu > rokTemu)  // Filtruj zlecenia z ostatniego roku
                                 .OrderByDescending(e => e.DataZapisu)
                                 .ToListAsync();
        }


        [HttpPost]
        public async Task<ActionResult> AddZlecenieProdukcyjneWewAsync(ZleceniaProdukcyjneWew zlecenieWew)
        {
            try
            {
                _context.ZleceniaProdukcyjneWew.Add(zlecenieWew);
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
        public async Task<ActionResult> UpdateZlecenieProdukcyjneWewAsync(long id, ZleceniaProdukcyjneWew zlecenieWew)
        {
            if (id != zlecenieWew.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(zlecenieWew).State = EntityState.Modified;

            try
            {
                // Utwórz nowy rekord w tabeli ZleceniaProdukcyjneZmianyStatusu
                var existingZlecenie = await _context.ZleceniaProdukcyjneWew.AsNoTracking().FirstOrDefaultAsync(z => z.Id == id);
                if (existingZlecenie != null)
                {
                    if (!AreZleceniaEqual(existingZlecenie, zlecenieWew))
                    {
                        // Utwórz nowy rekord w tabeli ZleceniaProdukcyjneZmianyStatusu
                        var zmianaStatusu = new ZleceniaProdukcyjneZmianyStatusu
                        {
                            RowId = Guid.NewGuid().ToString(),
                            RowIdZlecenia = zlecenieWew.RowId.ToString(),
                            TypZamowienia = zlecenieWew.TypZamowienia,
                            NumerZamowienia = zlecenieWew.NumerZamowienia,
                            DataZapisu = DateTime.Now,
                            OstatniaZmiana = "Zmiana: " + DateTime.Now.ToLongDateString(),
                            KtoZapisal = zlecenieWew.KtoZapisal,
                            NumerZlecenia = zlecenieWew.NumerZlecenia,
                            Klient = zlecenieWew.Klient,
                            Miejscowosc = zlecenieWew.Miejscowosc,
                            Adres = zlecenieWew.Adres,
                            Telefon = zlecenieWew.Telefon,
                            Email = "",
                            NazwaProduktu = zlecenieWew.NazwaProduktu,
                            Ilosc = zlecenieWew.Ilosc,
                            Wartosc = zlecenieWew.Wartosc,
                            DataProdukcji = zlecenieWew.DataProdukcji,
                            DataWysylki = zlecenieWew.DataWysylki,
                            DataMontazu = zlecenieWew.DataMontazu,
                            ZlecenieWewnatrzne = true,
                            DataGotowosci = zlecenieWew.DataGotowosci,
                            DataRozpProdukcji = zlecenieWew.DataRozpProdukcji,
                            NumerUmowy = zlecenieWew.NumerUmowy,
                            JednostkiNaZlecenie = zlecenieWew.JednostkiNaZlecenie,
                            KodProduktu = zlecenieWew.KodProduktu,
                            Tags = zlecenieWew.Tags,
                            NazwaProduktu2 = zlecenieWew.NazwaProduktu2,
                        };

                        // Dodaj rekord do kontekstu
                        _context.ZleceniaProdukcyjneZmianyStatusu.Add(zmianaStatusu);

                    }
                }

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ZlecenieProdukcyjneWewExists(id))
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

        private bool AreZleceniaEqual(ZleceniaProdukcyjneWew existing, ZleceniaProdukcyjneWew updated)
        {
            return existing.TypZamowienia == updated.TypZamowienia &&
                   existing.NumerZamowienia == updated.NumerZamowienia &&
                   existing.Klient == updated.Klient &&
                   existing.Miejscowosc == updated.Miejscowosc &&
                   existing.Adres == updated.Adres &&
                   existing.Telefon == updated.Telefon &&
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
        public async Task<ActionResult> DeleteZlecenieProdukcyjneWewAsync(long id)
        {
            var zlecenieWew = await _context.ZleceniaProdukcyjneWew.FindAsync(id);
            if (zlecenieWew == null)
            {
                return NotFound("Nie znaleziono zlecenia o podanym ID.");
            }

            _context.ZleceniaProdukcyjneWew.Remove(zlecenieWew);
            await _context.SaveChangesAsync();

            // Wyświetl numer zamówienia w konsoli
            Console.WriteLine(zlecenieWew.NumerZamowienia);

            return Ok("Zlecenie zostało pomyślnie usunięte.");
        }

        private bool ZlecenieProdukcyjneWewExists(long id)
        {
            return _context.ZleceniaProdukcyjneWew.Any(e => e.Id == id);
        }

        [HttpPost("zmien-date-produkcji")]
        public async Task<IActionResult> ZmienDateProdukcjiWew(string rowid, [FromBody] DateTime nowaDataProdukcji)
        {
            var result = await _context.ZmienDateProdukcjiWew(rowid, nowaDataProdukcji);
            if (result)
            {
                return Ok("Uwagi zaktualizowane pomyślnie.");
            }
            else
            {
                return NotFound("Nie znaleziono rekordu o podanym rowid.");
            }
        }

        [HttpPost("zmien-date-rozpoczecia-produkcji")]
        public async Task<IActionResult> ZmienDateRozpoczeciaProdukcji(string rowId, string rowIdLinia, [FromBody] DateTime nowaDataProdukcji)
        {
            var result = await _context.ZmienDateRozpoczeciaProdukcjiWew(rowId, rowIdLinia, nowaDataProdukcji);
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
