using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using netDxf.Entities;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZleceniaCzasNaLinieProdController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZleceniaCzasNaLinieProdController> _logger;

        public ZleceniaCzasNaLinieProdController(ApplicationDbContext context, ILogger<ZleceniaCzasNaLinieProdController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("rowid/{rowid}")]
        public async Task<ActionResult<List<LinieProdukcyjneWithCzasViewModel>>> GetZleceniaProdukcyjneAsync(string rowid)
        {
            var result = await (from linia in _context.LinieProdukcyjne
                                join zlecenie in _context.ZleceniaCzasNaLinieProd
                                on linia.RowId equals zlecenie.RowIdLinieProdukcyjne into gj
                                from subzlecenie in gj.Where(z => z.RowIdZleceniaProdukcyjne == rowid).DefaultIfEmpty()
                                select new LinieProdukcyjneWithCzasViewModel
                                {
                                    Id = linia.Id,
                                    RowIdLinieProdukcyjne = linia.RowId,
                                    RowIdZlecnia = rowid,
                                    IdLiniiProdukcyjnej = linia.IdLiniiProdukcyjnej,
                                    NazwaLiniiProdukcyjnej = linia.NazwaLiniiProdukcyjnej,
                                    DziennaZdolnoscProdukcyjna = linia.DziennaZdolnoscProdukcyjna,
                                    Uwagi = linia.Uwagi,
                                    KtoZapisal = linia.KtoZapisal,
                                    OstatniaZmiana = linia.OstatniaZmiana,
                                    CzasNaZlecenie = subzlecenie != null ? subzlecenie.CzasNaZlecenie : 0,
                                    ZlecenieWewnetrzne = subzlecenie != null ? subzlecenie.ZlecenieWewnetrzne : false
                                })
                                .OrderBy(e => e.IdLiniiProdukcyjnej) // Sortowanie według IdLiniiProdukcyjnej, jeśli chcesz sortować według innego pola, zmień tutaj
                                .ToListAsync();

            return Ok(result);
        }


        [HttpPost]
        public async Task<ActionResult> AddZleceniaCzasNaLProdAsync(ZleceniaCzasNaLinieProd zlecCzasLinia)
        {
            try
            {
                _context.ZleceniaCzasNaLinieProd.Add(zlecCzasLinia);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania Czasy na linie.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }


        [HttpPut("{rowId}")]
        public async Task<IActionResult> UpdateZlecenieCzasNaLinieProd(string rowId, [FromBody] LinieProdukcyjneWithCzasViewModel czaszmiana)
        {

            //// METODA USUNIĘCIA WSZYTKICH DANYCH!!!
            //// Pobierz wszystkie rekordy z tabeli
            //var wszystkieZleceniaNaLinii = _context.ZleceniaCzasNaLinieProd.ToList();
            //// Usuń wszystkie rekordy
            //_context.ZleceniaCzasNaLinieProd.RemoveRange(wszystkieZleceniaNaLinii);
            //// Zapisz zmiany do bazy danych
            //await _context.SaveChangesAsync();

            // Znalezienie istniejącego zlecenia
            var existingZlecenie = await _context.ZleceniaCzasNaLinieProd
                .FirstOrDefaultAsync(z => z.RowIdZleceniaProdukcyjne == rowId && z.RowIdLinieProdukcyjne == czaszmiana.RowIdLinieProdukcyjne);

            if (existingZlecenie == null)
            {
                // Jeśli rekord nie istnieje, utwórz nowy
                var newZlecenie = new ZleceniaCzasNaLinieProd
                {
                    RowIdZleceniaProdukcyjne = rowId,
                    RowIdLinieProdukcyjne = czaszmiana.RowIdLinieProdukcyjne,
                    CzasNaZlecenie = czaszmiana.CzasNaZlecenie,
                    ZlecenieWewnetrzne = czaszmiana.ZlecenieWewnetrzne,
                    Uwagi = czaszmiana.Uwagi,
                    DataZapisu = DateTime.Now,
                    KtoZapisal = czaszmiana.KtoZapisal
                };

                // Dodaj nowy rekord do kontekstu
                await _context.ZleceniaCzasNaLinieProd.AddAsync(newZlecenie);

                try
                {
                    // Zapisz zmiany w bazie danych
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    // Obsłuż ewentualne wyjątki podczas zapisu
                    return StatusCode(StatusCodes.Status500InternalServerError, $"Błąd podczas dodawania nowego rekordu: {ex.Message}");
                }

                // Zwróć odpowiedź z kodem 201 Created
                return CreatedAtAction(nameof(UpdateZlecenieCzasNaLinieProd), new { rowId = newZlecenie.RowIdZleceniaProdukcyjne }, newZlecenie);
            }
            else
            {
                // Jeśli rekord istnieje, zaktualizuj go
                existingZlecenie.CzasNaZlecenie = czaszmiana.CzasNaZlecenie;
                existingZlecenie.RowIdLinieProdukcyjne = czaszmiana.RowIdLinieProdukcyjne;
                existingZlecenie.ZlecenieWewnetrzne = czaszmiana.ZlecenieWewnetrzne;
                existingZlecenie.Uwagi = czaszmiana.Uwagi;
                existingZlecenie.OstatniaZmiana = "Zmiana: " + DateTime.Now.ToLongDateString();
                existingZlecenie.KtoZapisal = czaszmiana.KtoZapisal;

                try
                {
                    // Zapisz zmiany w bazie danych
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Jeśli rekord nie istnieje podczas zapisu (np. został usunięty), zwróć kod 404 Not Found
                    if (!ZlecenieCzasNaLinieProdExists(rowId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        // Ponowne zgłoszenie wyjątku
                        throw;
                    }
                }

                // Zwróć odpowiedź z kodem 204 No Content
                return NoContent();
            }
        }

        private bool ZlecenieCzasNaLinieProdExists(string rowId)
        {
            return _context.ZleceniaCzasNaLinieProd.Any(e => e.RowIdZleceniaProdukcyjne == rowId);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteZlecenieProdukcyjneAsync(long id)
        {
            var zlecCzasLinia = await _context.ZleceniaCzasNaLinieProd.FindAsync(id);
            if (zlecCzasLinia == null)
            {
                return NotFound("Nie znaleziono rekordu o podanym ID.");
            }

            _context.ZleceniaCzasNaLinieProd.Remove(zlecCzasLinia);
            await _context.SaveChangesAsync();

            return NoContent(); // lub Ok() jeśli chcesz zwrócić jakąś informację
        }

    }
}
