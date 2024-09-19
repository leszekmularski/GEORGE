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
    public class ElementyDoZlecenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ElementyDoZlecenController> _logger;

        public ElementyDoZlecenController(ApplicationDbContext context, ILogger<ElementyDoZlecenController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ElemetZamDoZlecen>>> GetZleceniaProdukcyjneAsync()
        {
            return await _context.ElemetZamDoZlecen.OrderBy(e => e.RowIdZlecenia).ToListAsync();
        }

        [HttpGet("rowid/{rowid}")]
        public async Task<ActionResult<List<ElemetZamDoZlecenWithProducent>>> GetZleceniaProdukcyjneRowIdAsync(string rowid)
        {
            var result = await (from elemet in _context.ElemetZamDoZlecen
                                join producent in _context.ProducenciPodwykonawcy
                                on elemet.RowIdProducent equals producent.RowId into gj
                                from subproducent in gj.DefaultIfEmpty() // Zwróci dane nawet, jeśli producent nie istnieje
                                where elemet.RowIdZlecenia == rowid
                                select new ElemetZamDoZlecenWithProducent
                                {
                                    ElemetZamDoZlecen = elemet,
                                    ProducenciPodwykonawcy = subproducent,
                                    DodatkowaInformacja = "Dane załadowano: " + DateTime.Now,
                                    ProducentIMiejscowosc = subproducent.NazwaProducenta + " " + subproducent.Miejscowosc
                                })
                                .OrderBy(e => e.ElemetZamDoZlecen.RowIdProducent)
                                .ThenBy(e => e.ElemetZamDoZlecen.NazwaProduktu)
                                .ThenBy(e => e.ElemetZamDoZlecen.IloscSztuk)
                                .ToListAsync();

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> AddElementyDoZlecenAsync(ElemetZamDoZlecen pozZlec)
        {
            try
            {
                _context.ElemetZamDoZlecen.Add(pozZlec);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania ElementyDoZlecen.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }

        [HttpPost("save-all/{kasujPrzedZapisem}")]
        public async Task<ActionResult> SaveAll(List<ElemetZamDoZlecen> kantowki, bool kasujPrzedZapisem)
        {
            try
            {
                // Sprawdź, czy są jakieś rekordy do usunięcia
                var rowIdZleceniaPattern = "ABCD---"; // Przykładowy wzorzec
                if(kantowki.Count > 0)
                {
                    rowIdZleceniaPattern = kantowki[0].RowIdZlecenia;
                }
                var recordsToDelete = await _context.ElemetZamDoZlecen
                    .Where(k => k.RowIdZlecenia == rowIdZleceniaPattern)
                    .ToListAsync();

                // Usuń istniejące rekordy pasujące do wzorca
                if(kasujPrzedZapisem) _context.ElemetZamDoZlecen.RemoveRange(recordsToDelete);

                // Dodaj nowe rekordy
                _context.ElemetZamDoZlecen.AddRange(kantowki);

                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas zapisywania ElemetZamDoZlecen.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateElementyDoZleceneAsync(long id, ElemetZamDoZlecen pozEl)
        {
            if (id != pozEl.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(pozEl).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ElementZamExists(id))
                {
                    return NotFound("Nie znaleziono Kantowke o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji ElemetZamDoZlecen .");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        // Metoda do aktualizacji stanu
        [HttpPut("{id}/updateCzyZamowiono")]
        public async Task<ActionResult> UpdateMaterialZeStanMagazynAsync(long id, [FromBody] bool czyZamowiono)
        {
            // Sprawdź, czy istnieje obiekt o podanym ID
            var pozZlec = await _context.ElemetZamDoZlecen.FindAsync(id);
            if (pozZlec == null)
            {
                return NotFound("Nie znaleziono ElemetZamDoZlecen o podanym ID.");
            }

            // Zaktualizuj wartość
            pozZlec.CzyZamowiono = czyZamowiono;
            pozZlec.DataZamowienia = DateTime.Now;

            // Oznacz obiekt jako zmodyfikowany
            _context.Entry(pozZlec).State = EntityState.Modified;

            try
            {
                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ElementZamExists(id))
                {
                    return NotFound("Nie znaleziono ElemetZamDoZlecen o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji ElemetZamDoZlecen.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpPut("{id}/updateCzyDostarczono")]
        public async Task<ActionResult> UpdateMaterialDostarczononMagazynAsync(long id, [FromBody] bool czyDostarczono)
        {
            // Sprawdź, czy istnieje obiekt o podanym ID
            var pozZlec = await _context.ElemetZamDoZlecen.FindAsync(id);
            if (pozZlec == null)
            {
                return NotFound("Nie znaleziono ElemetZamDoZlecen o podanym ID.");
            }

            // Zaktualizuj wartość
            pozZlec.PozDostarczono = czyDostarczono;
            pozZlec.DataDostarczenia = DateTime.Now;

            // Oznacz obiekt jako zmodyfikowany
            _context.Entry(pozZlec).State = EntityState.Modified;

            try
            {
                // Zapisz zmiany w bazie danych
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ElementZamExists(id))
                {
                    return NotFound("Nie znaleziono ElemetZamDoZlecen o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji ElemetZamDoZlecen.");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        private bool ElementZamExists(long id)
        {
            return _context.ElemetZamDoZlecen.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteElemntyDoZlecenAsync(long id)
        {
            var pozZlec = await _context.ElemetZamDoZlecen.FindAsync(id);
            if (pozZlec == null)
            {
                return NotFound("Nie znaleziono rekordu o podanym ID.");
            }

            _context.ElemetZamDoZlecen.Remove(pozZlec);
            await _context.SaveChangesAsync();

            return NoContent(); // lub Ok() jeśli chcesz zwrócić jakąś informację
        }

    }
}
