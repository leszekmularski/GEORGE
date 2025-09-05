using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WzorceKompltacjiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WzorceKompltacjiController> _logger;

        public WzorceKompltacjiController(ApplicationDbContext context, ILogger<WzorceKompltacjiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<WzorceKompltacji>>> GetWzorceKompltacjiControllerAsync()
        {
            return await _context.WzorceKompltacji.OrderBy(e => e.Id).ToListAsync();
        }

        [HttpGet("rowidwzorca/{rowidwzorca}")]
        public async Task<ActionResult<List<WzorceKompltacji>>> GetWzorceKompltacjiRowIdAsync(string rowidwzorca)
        {
            return await _context.WzorceKompltacji.Where(x=>x.RowIdWzorca.ToString() == rowidwzorca).OrderBy(e => e.NazwaProduktu).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> AddWzorceKompltacjiAsync(WzorceKompltacji pozPozWzorca)
        {
            try
            {
                _context.WzorceKompltacji.Add(pozPozWzorca);
                await _context.SaveChangesAsync();
                return Ok();
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas dodawania WzorceKompltacji.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania w WzorceKompltacji.");
            }
        }

        [HttpPut("{id:long}")]
        public async Task<ActionResult> UpdateWzorceKompltacjiAsync(long id, WzorceKompltacji pozEl)
        {
            if (id != pozEl.Id)
            {
                return BadRequest("ID WzorceKompltacji nie pasuje do ID w żądaniu.");
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
                    return NotFound("Nie znaleziono WzorceKompltacji o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji WzorceKompltacji .");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        [HttpPut("all/{rowIdALL}")]
        public async Task<ActionResult> UpdateWzorceKompltacjiAsync(string rowIdALL, [FromBody] UpdateWzorzecRequest request)
        {
            if (string.IsNullOrWhiteSpace(rowIdALL))
            {
                return BadRequest("RowID grupy nie pasuje do RowID w żądaniu.");
            }

            var nowaNazwaGrupy = WebUtility.UrlDecode(request.NowaNazwaGrupy);

            try
            {
                var rekordyDoZmiany = await _context.WzorceKompltacji
                    .Where(x => x.RowIdWzorca.ToString() == rowIdALL)
                    .ToListAsync();

                if (!rekordyDoZmiany.Any())
                    return NotFound("Nie znaleziono rekordów dla podanego RowID.");

                foreach (var rekord in rekordyDoZmiany)
                {
                    rekord.NazwaWzorca = nowaNazwaGrupy;
                }

                await _context.SaveChangesAsync();

                return Ok($"Zaktualizowano {rekordyDoZmiany.Count} rekordów.");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Błąd podczas aktualizacji WzorceKompltacji.");
                return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
            }
        }


        private bool ElementZamExists(long id)
        {
            return _context.ElemetZamDoZlecen.Any(e => e.Id == id);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteWzorceKompltacjiAsync(long id)
        {
            var pozWzorca = await _context.WzorceKompltacji.FindAsync(id);
            if (pozWzorca == null)
            {
                return NotFound("Nie znaleziono rekordu o podanym ID w pozWzorca.");
            }

            _context.WzorceKompltacji.Remove(pozWzorca);
            await _context.SaveChangesAsync();

            return NoContent(); // lub Ok() jeśli chcesz zwrócić jakąś informację
        }

    }
    public class UpdateWzorzecRequest
    {
        public string NowaNazwaGrupy { get; set; } = string.Empty;
    }

}
