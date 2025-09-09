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
    public class WzorceKompletacjiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WzorceKompletacjiController> _logger;

        public WzorceKompletacjiController(ApplicationDbContext context, ILogger<WzorceKompletacjiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<WzorceKompletacji>>> GetWzorceKompltacjiControllerAsync()
        {
            return await _context.WzorceKompltacji.OrderBy(e => e.Id).ToListAsync();
        }

        [HttpGet("rowidwzorca/{rowidwzorca}")]
        public async Task<ActionResult<List<WzorceKompletacji>>> GetWzorceKompltacjiRowIdAsync(string rowidwzorca)
        {
            return await _context.WzorceKompltacji.Where(x=>x.RowIdWzorca.ToString() == rowidwzorca).OrderBy(e => e.NazwaProduktu).ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult> AddWzorceKompltacjiAsync([FromBody] WzorceKompletacji wzorzec)
        {
            try
            {
                if (wzorzec is null)
                    return BadRequest("Wysłany obiekt jest pusty!");

                _context.WzorceKompltacji.Add(wzorzec);
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
        public async Task<ActionResult> UpdateWzorceKompltacjiAsync(long id, [FromBody] WzorceKompletacji pozEl)
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
                    _logger.LogError(ex, "Błąd podczas aktualizacji WzorceKompltacji.");
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

        [HttpDelete("by-rowid/{rowId}")]
        public async Task<ActionResult> DeleteByRowIdAsync(string rowId)
        {
            if (!Guid.TryParse(rowId, out var guid))
            {
                return BadRequest($"Niepoprawny format GUID: {rowId}");
            }

            var records = _context.WzorceKompltacji
                .Where(x => x.RowIdWzorca == guid);

            if (!records.Any())
            {
                return NotFound($"Nie znaleziono rekordów z RowIdWzorca = {guid}");
            }

            _context.WzorceKompltacji.RemoveRange(records);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("clone/{rowId}/{nowaNazwa}")]
        public async Task<ActionResult> CloneByRowIdAsync(string rowId, string nowaNazwa)
        {
            if (!Guid.TryParse(rowId, out var guid))
            {
                return BadRequest($"Niepoprawny format GUID: {rowId}");
            }

            var records = await _context.WzorceKompltacji
                .Where(x => x.RowIdWzorca == guid)
                .ToListAsync();

            if (!records.Any())
            {
                return NotFound($"Nie znaleziono rekordów z RowIdWzorca = {guid}");
            }

            nowaNazwa = WebUtility.UrlDecode(nowaNazwa);

            // Nowy klucz grupy
            var newRowId = Guid.NewGuid();

            // Tworzymy kopie rekordów
            var clonedRecords = records.Select(r => new WzorceKompletacji
            {
                // zakładam, że masz np. Id jako klucz główny -> zostanie nadany przez bazę
                RowIdWzorca = newRowId,
                NazwaWzorca = nowaNazwa,
                NazwaProduktu = r.NazwaProduktu,
                NumerKatalogowy = r.NumerKatalogowy,
                Typ = r.Typ,
                Jednostka = r.Jednostka,
                CenaNetto = r.CenaNetto,
                Objetosc = r.Objetosc,
                Powierzchnia = r.Powierzchnia,
                Uwagi = r.Uwagi,
                CzasRealizacjiZamowienia = r.CzasRealizacjiZamowienia,
                Ilosc = r.Ilosc,
                Kolor = r.Kolor,
                Opis = r.Opis,

            }).ToList();

            _context.WzorceKompltacji.AddRange(clonedRecords);
            await _context.SaveChangesAsync();

            return Ok(new { NewRowId = newRowId, Count = clonedRecords.Count });
        }

    }

    public class UpdateWzorzecRequest
    {
        public string NowaNazwaGrupy { get; set; } = string.Empty;
    }

}
