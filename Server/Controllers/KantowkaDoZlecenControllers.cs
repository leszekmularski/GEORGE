using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KantowkaDoZlecenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<KantowkaDoZlecenController> _logger;

        public KantowkaDoZlecenController(ApplicationDbContext context, ILogger<KantowkaDoZlecenController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<KantowkaDoZlecen>>> GetZleceniaProdukcyjneAsync()
        {
            return await _context.KantowkaDoZlecen.OrderBy(e => e.RowIdZlecenia).ToListAsync();
        }

        [HttpGet("rowid/{rowid}")]
        public async Task<ActionResult<List<KantowkaDoZlecen>>> GetZleceniaProdukcyjneRowIdAsync(string rowid)
        {
            var result = await _context.KantowkaDoZlecen
                .Where(x => x.RowIdZlecenia == rowid)
                .OrderBy(e => e.RowIdZlecenia).OrderBy(z => z.Przekroj).OrderBy(z => z.DlugoscZamawiana).OrderBy(z => z.DlugoscNaGotowo)
                .ToListAsync();

            //if (result == null || !result.Any())
            //{
            //    return NotFound("Brak danych dla podanego RowIdZlecenia.");
            //}

            return Ok(result);
        }


        [HttpPost]
        public async Task<ActionResult> AddKantowekDoZlecAsync(KantowkaDoZlecen kantowka)
        {
            try
            {
                _context.KantowkaDoZlecen.Add(kantowka);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania Karty Instrukcyjne.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPost("save-all/{usunAll}")]
        public async Task<ActionResult> SaveAll(List<KantowkaDoZlecen> kantowki, bool usunAll)
        {
            try
            {
                // Sprawdź, czy są jakieś rekordy do usunięcia
                var rowIdZleceniaPattern = "ABCD"; // Przykładowy wzorzec
                if(kantowki.Count > 0)
                {
                    rowIdZleceniaPattern = kantowki[0].RowIdZlecenia;
                }
                var recordsToDelete = await _context.KantowkaDoZlecen
                    .Where(k => k.RowIdZlecenia == rowIdZleceniaPattern)
                    .ToListAsync();

                if (usunAll)
                {
                    // Usuń istniejące rekordy pasujące do wzorca
                    _context.KantowkaDoZlecen.RemoveRange(recordsToDelete);
                }

                // Dodaj nowe rekordy
                _context.KantowkaDoZlecen.AddRange(kantowki);

                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zapisywania KantowkaDoZlecen.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateZlecenieProdukcyjneAsync(long id, KantowkaDoZlecen kantowka)
        {
            if (id != kantowka.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(kantowka).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!KantowkaExists(id))
                {
                    return NotFound("Nie znaleziono Kantowke o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji KantowkaDoZlecen .");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        // Metoda do aktualizacji stanu
        [HttpPut("{id}/updateMaterialZeStanMagazyn")]
        public async Task<ActionResult> UpdateMaterialZeStanMagazynAsync(long id, [FromBody] bool materialZeStanMagazyn)
        {
            // Sprawdź, czy istnieje obiekt o podanym ID
            var kantowka = await _context.KantowkaDoZlecen.FindAsync(id);
            if (kantowka == null)
            {
                return NotFound("Nie znaleziono KantowkaDoZlecen o podanym ID.");
            }

            // Zaktualizuj wartość
            kantowka.MaterialZeStanMagazyn = materialZeStanMagazyn;

            // Oznacz obiekt jako zmodyfikowany
            _context.Entry(kantowka).State = EntityState.Modified;

            try
            {
                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!KantowkaExists(id))
                {
                    return NotFound("Nie znaleziono KantowkaDoZlecen o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji KantowkaDoZlecen.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpPut("{rowIdZlecenia}/updateDateZamowienia")]
        public async Task<ActionResult> UpdateDateZamowienia(string rowIdZlecenia)
        {
            // Znajdź wszystkie rekordy spełniające warunek
            var kantowki = await _context.KantowkaDoZlecen
                .Where(k => k.RowIdZlecenia == rowIdZlecenia)
                .ToListAsync();

            // Jeśli nie znaleziono żadnych rekordów, zwróć NotFound
            if (kantowki == null || kantowki.Count == 0)
            {
                return NotFound("Nie znaleziono KantowkaDoZlecen o podanym ID.");
            }

            // Zaktualizuj pole DataZamowienia dla każdego rekordu
            foreach (var kantowka in kantowki)
            {
                kantowka.DataZamowienia = DateTime.Now;
                _context.Entry(kantowka).State = EntityState.Modified;
            }

            try
            {
                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
                return Ok("Zaktualizowano wszystkie zamówienia.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji KantowkaDoZlecen.");

                // Sprawdź, czy istnieją jakiekolwiek rekordy spełniające warunek
                var exists = await _context.KantowkaDoZlecen.AnyAsync(e => e.RowIdZlecenia == rowIdZlecenia);
                if (!exists)
                {
                    return NotFound("Nie znaleziono KantowkaDoZlecen o podanym ID.");
                }

                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPut("{rowIdZlecenia}/updateDateRealizacji")]
        public async Task<ActionResult> UpdateDateRealizacji(string rowIdZlecenia, [FromBody] bool czyDostarczono)
        {
            // Znajdź wszystkie rekordy spełniające warunek
            var kantowki = await _context.KantowkaDoZlecen
                .Where(k => k.RowIdZlecenia == rowIdZlecenia)
                .ToListAsync();

            // Jeśli nie znaleziono żadnych rekordów, zwróć NotFound
            if (kantowki == null || kantowki.Count == 0)
            {
                return NotFound("Nie znaleziono KantowkaDoZlecen o podanym ID.");
            }

            // Zaktualizuj pole DataZamowienia dla każdego rekordu
            foreach (var kantowka in kantowki)
            {
                if (czyDostarczono)
                {
                    kantowka.DataRealizacji = DateTime.Now;
                }
                else
                {
                    kantowka.DataRealizacji = DateTime.MinValue;
                }
      
                _context.Entry(kantowka).State = EntityState.Modified;
            }

            try
            {
                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
                Console.WriteLine($"Zaktualizowano wszystkie zamówienia - dotyczące zamówień kantówki {czyDostarczono}.");
                return Ok("Zaktualizowano wszystkie zamówienia - dotyczące zamówień kantówki.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji KantowkaDoZlecen.");

                // Sprawdź, czy istnieją jakiekolwiek rekordy spełniające warunek
                var exists = await _context.KantowkaDoZlecen.AnyAsync(e => e.RowIdZlecenia == rowIdZlecenia);
                if (!exists)
                {
                    return NotFound("Nie znaleziono KantowkaDoZlecen o podanym ID.");
                }

                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPut("{rowIdZlecenia}/updateDateZamowieniaStatusZamow")]
        public async Task<ActionResult> UpdateDateZamowieniaStatusZamow(string rowIdZlecenia)
        {
            // Znajdź wszystkie rekordy spełniające warunek
            var kantowki = await _context.KantowkaDoZlecen
                .Where(k => k.RowIdZlecenia == rowIdZlecenia)
                .ToListAsync();

            // Jeśli nie znaleziono żadnych rekordów, zwróć NotFound
            if (kantowki == null || kantowki.Count == 0)
            {
                return NotFound("Nie znaleziono KantowkaDoZlecen o podanym ID.");
            }

            // Zaktualizuj pole DataZamowienia dla każdego rekordu
            foreach (var kantowka in kantowki)
            {
                kantowka.DataZamowienia = DateTime.Now;
                kantowka.WyslanoDoZamowien =true;
                _context.Entry(kantowka).State = EntityState.Modified;
            }

            try
            {
                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
                return Ok("Zaktualizowano wszystkie zamówienia.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji KantowkaDoZlecen.");

                // Sprawdź, czy istnieją jakiekolwiek rekordy spełniające warunek
                var exists = await _context.KantowkaDoZlecen.AnyAsync(e => e.RowIdZlecenia == rowIdZlecenia);
                if (!exists)
                {
                    return NotFound("Nie znaleziono KantowkaDoZlecen o podanym ID.");
                }

                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        private bool KantowkaExists(long id)
        {
            return _context.KantowkaDoZlecen.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteZlecenieProdukcyjneAsync(long id)
        {
            var kantowka = await _context.KantowkaDoZlecen.FindAsync(id);
            if (kantowka == null)
            {
                return NotFound("Nie znaleziono rekordu o podanym ID.");
            }

            _context.KantowkaDoZlecen.Remove(kantowka);
            await _context.SaveChangesAsync();

            return NoContent(); // lub Ok() jeśli chcesz zwrócić jakąś informację
        }

    }
}
