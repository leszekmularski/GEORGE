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
    public class SzybyDoZlecenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SzybyDoZlecenController> _logger;

        public SzybyDoZlecenController(ApplicationDbContext context, ILogger<SzybyDoZlecenController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<SzybyDoZlecen>>> GetSzybyDoZlecenAsync()
        {
            return await _context.SzybyDoZlecen.OrderBy(e => e.RowIdZlecenia).ToListAsync();
        }

        [HttpGet("rowid/{rowid}")]
        public async Task<ActionResult<List<SzybyDoZlecen>>> GetSzybyDoZlecenRowIdAsync(string rowid)
        {
            var result = await _context.SzybyDoZlecen
                .Where(x => x.RowIdZlecenia == rowid)
                .OrderBy(e => e.RowIdZlecenia).OrderBy(z => z.RodzajSzyby).OrderBy(z => z.Szerokosc).OrderBy(z => z.Wysokosc)
                .ToListAsync();

            //if (result == null || !result.Any())
            //{
            //    return NotFound("Brak danych dla podanego RowIdZlecenia.");
            //}

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> AddSzybyDoZleceneAsync(SzybyDoZlecen szyba)
        {
            try
            {
                _ = _context.SzybyDoZlecen.Add(szyba);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania Karty Instrukcyjne.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPost("save-all/{kasujPrzedZapisem}")]
        public async Task<ActionResult> SaveAll(List<SzybyDoZlecen> kantowki, bool kasujPrzedZapisem)
        {
            try
            {
                // Sprawdź, czy są jakieś rekordy do usunięcia
                var rowIdZleceniaPattern = "ABCD"; // Przykładowy wzorzec
                if(kantowki.Count > 0)
                {
                    rowIdZleceniaPattern = kantowki[0].RowIdZlecenia;
                }
                var recordsToDelete = await _context.SzybyDoZlecen
                    .Where(k => k.RowIdZlecenia == rowIdZleceniaPattern)
                    .ToListAsync();

                // Usuń istniejące rekordy pasujące do wzorca
                if(kasujPrzedZapisem) _context.SzybyDoZlecen.RemoveRange(recordsToDelete);

                // Dodaj nowe rekordy
                _context.SzybyDoZlecen.AddRange(kantowki);

                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zapisywania SzybyDoZlecen.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateSzybyDoZlecenAsync(long id, SzybyDoZlecen szyba)
        {
            if (id != szyba.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(szyba).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SzybaExists(id))
                {
                    return NotFound("Nie znaleziono Kantowke o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji SzybyDoZlecen .");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        // Metoda do aktualizacji stanu
        [HttpPut("{id}/updateCzyKsztalt")]
        public async Task<ActionResult> UpdateMaterialZeStanMagazynAsync(long id, [FromBody] bool czyKsztalt)
        {
            // Sprawdź, czy istnieje obiekt o podanym ID
            var szyba = await _context.SzybyDoZlecen.FindAsync(id);
            if (szyba == null)
            {
                return NotFound("Nie znaleziono SzybyDoZlecen o podanym ID.");
            }

            // Zaktualizuj wartość
            szyba.CzyKsztalt = czyKsztalt;

            // Oznacz obiekt jako zmodyfikowany
            _context.Entry(szyba).State = EntityState.Modified;

            try
            {
                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SzybaExists(id))
                {
                    return NotFound("Nie znaleziono SzybyDoZlecen o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji SzybyDoZlecen.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpPut("{rowId}/updateDostarczonoPoRowId")]
        public async Task<ActionResult> UpdateMaterialDostarczonoAsync(string rowId, [FromBody] bool czyDostarczono)
        {
            // Sprawdź, czy istnieje obiekt o podanym ID
            // Znajdź obiekt na podstawie RowIdZlecenia
            var szyba = await _context.SzybyDoZlecen
                .FirstOrDefaultAsync(s => s.RowIdZlecenia == rowId);

            if (szyba == null)
            {
                return NotFound("Nie znaleziono SzybyDoZlecen o podanym ID.");
            }

            // Zaktualizuj wartość
            szyba.PozDostarczono = czyDostarczono;

            if (szyba.DataDostarczenia == DateTime.MinValue && czyDostarczono)  szyba.DataDostarczenia = DateTime.Now;
            if(!czyDostarczono) szyba.DataDostarczenia = DateTime.MinValue;

            // Oznacz obiekt jako zmodyfikowany
            _context.Entry(szyba).State = EntityState.Modified;

            try
            {
                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SzybaExistRowId(rowId))
                {
                    return NotFound("Nie znaleziono SzybyDoZlecen o podanym rowId.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji SzybyDoZlecen.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpPut("{id}/updateDostarczono")]
        public async Task<ActionResult> UpdateMaterialDostarczonoAsync(long id, [FromBody] bool czyDostarczono)
        {
            // Sprawdź, czy istnieje obiekt o podanym ID
            var szyba = await _context.SzybyDoZlecen.FindAsync(id);
            if (szyba == null)
            {
                return NotFound("Nie znaleziono SzybyDoZlecen o podanym ID.");
            }

            // Zaktualizuj wartość
            szyba.PozDostarczono = czyDostarczono;

            if(szyba.DataDostarczenia == DateTime.MinValue && czyDostarczono)
            szyba.DataDostarczenia = DateTime.Now;

            // Oznacz obiekt jako zmodyfikowany
            _context.Entry(szyba).State = EntityState.Modified;

            try
            {
                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SzybaExists(id))
                {
                    return NotFound("Nie znaleziono SzybyDoZlecen o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji SzybyDoZlecen.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpPut("{id}/updateZamowiono")]
        public async Task<ActionResult> UpdateMaterialZamowionoAsync(long id, [FromBody] bool czyDostarczono)
        {
            // Sprawdź, czy istnieje obiekt o podanym ID
            var szyba = await _context.SzybyDoZlecen.FindAsync(id);
            if (szyba == null)
            {
                return NotFound("Nie znaleziono SzybyDoZlecen o podanym ID.");
            }

            // Zaktualizuj wartość
            szyba.CzyZamowiono = czyDostarczono;

            if (szyba.DataZamowienia == DateTime.MinValue && czyDostarczono)
                szyba.DataZamowienia = DateTime.Now;

            // Oznacz obiekt jako zmodyfikowany
            _context.Entry(szyba).State = EntityState.Modified;

            try
            {
                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!SzybaExists(id))
                {
                    return NotFound("Nie znaleziono SzybyDoZlecen o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji SzybyDoZlecen.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpPut("{rowIdZlecenia}/updateDateZamowieniaStatusZamow")]
        public async Task<ActionResult> UpdateDateZamowieniaStatusZamow(string rowIdZlecenia)
        {
            // Znajdź wszystkie rekordy spełniające warunek
            var szyby = await _context.SzybyDoZlecen
                .Where(k => k.RowIdZlecenia == rowIdZlecenia)
                .ToListAsync();

            // Jeśli nie znaleziono żadnych rekordów, zwróć NotFound
            if (szyby == null || szyby.Count == 0)
            {
                return NotFound("Nie znaleziono KantowkaDoZlecen o podanym ID.");
            }

            // Zaktualizuj pole DataZamowienia dla każdego rekordu
            foreach (var szyba in szyby)
            {
                szyba.DataZamowienia = DateTime.Now;
                szyba.CzyZamowiono = true;
                _context.Entry(szyba).State = EntityState.Modified;
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
                var exists = await _context.SzybyDoZlecen.AnyAsync(e => e.RowIdZlecenia == rowIdZlecenia);
                if (!exists)
                {
                    return NotFound("Nie znaleziono KantowkaDoZlecen o podanym ID.");
                }

                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        private bool SzybaExists(long id)
        {
            return _context.SzybyDoZlecen.Any(e => e.Id == id);
        }

        private bool SzybaExistRowId(string roIdZlec)
        {
            return _context.SzybyDoZlecen.Any(s => s.RowIdZlecenia == roIdZlec);

        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteZlecenieProdukcyjneAsync(long id)
        {
            var szyba = await _context.SzybyDoZlecen.FindAsync(id);
            if (szyba == null)
            {
                return NotFound("Nie znaleziono rekordu o podanym ID.");
            }

            _context.SzybyDoZlecen.Remove(szyba);
            await _context.SaveChangesAsync();

            return NoContent(); // lub Ok() jeśli chcesz zwrócić jakąś informację
        }

    }
}
