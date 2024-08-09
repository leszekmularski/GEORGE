using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
                                    RowIdZlecnia = subzlecenie.RowId,
                                    IdLiniiProdukcyjnej = linia.IdLiniiProdukcyjnej,
                                    NazwaLiniiProdukcyjnej = linia.NazwaLiniiProdukcyjnej,
                                    DziennaZdolnoscProdukcyjna = linia.DziennaZdolnoscProdukcyjna,
                                    Uwagi = linia.Uwagi,
                                    KtoZapisal = linia.KtoZapisal,
                                    OstatniaZmiana = linia.OstatniaZmiana,
                                    CzasNaZlecenie = subzlecenie != null ? subzlecenie.CzasNaZlecenie : 0
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
        public async Task<IActionResult> UpdateZlecenieCzasNaLinieProd(string rowId, [FromBody] LinieProdukcyjneWithCzasViewModel updatedZlecenie)
        {
            if (rowId != updatedZlecenie.RowIdZlecnia)
            {
                return BadRequest();
            }

            var existingZlecenie = await _context.ZleceniaCzasNaLinieProd
                .FirstOrDefaultAsync(z => z.RowIdZleceniaProdukcyjne == rowId);

            if (existingZlecenie == null)
            {
                return NotFound();
            }

            existingZlecenie.CzasNaZlecenie = updatedZlecenie.CzasNaZlecenie;
            // Update other fields if necessary

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ZlecenieCzasNaLinieProdExists(rowId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
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
