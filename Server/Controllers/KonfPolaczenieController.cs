using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using GEORGE.Shared.Models;            // <-- lub odpowiednią dla KonfPolaczenie
using GEORGE.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KonfPolaczenieController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KonfPolaczenieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/konfpolaczenie/find-by-elements/{zewId}/{wewId}/{strona}
        [HttpGet("find-by-elements/{zewId:guid}/{wewId:guid}/{stronaPolaczenia}")]
        public async Task<ActionResult<KonfPolaczenie?>> GetByElementIdsAndSide(Guid zewId, Guid wewId, string stronaPolaczenia)
        {
            try
            {
                if (stronaPolaczenia == "ALL")
                {
                    var record = await _context.KonfPolaczenie
                    .FirstOrDefaultAsync(p =>
                        p.ElementZewnetrznyId == zewId &&
                        p.ElementWewnetrznyId == wewId &&
                        p.StronaPolaczenia != null);

                    if (record is null)
                        return NotFound();

                    return record;
                }
                else
                {
                    var record = await _context.KonfPolaczenie
                    .FirstOrDefaultAsync(p =>
                        p.ElementZewnetrznyId == zewId &&
                        p.ElementWewnetrznyId == wewId &&
                        p.StronaPolaczenia != null &&
                        p.StronaPolaczenia.ToLower() == stronaPolaczenia.ToLower());

                    if (record is null)
                        return NotFound();

                    return record;
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd serwera: {ex.Message}");
            }
        }

        // ✅ GET: api/konfpolaczenie/find-by-elements/{zewId}/{wewId}}
        [HttpGet("find-by-elements-all/{zewId:guid}/{wewId:guid}")]
        public async Task<ActionResult<List<KonfPolaczenie>>> GetByElementIds(Guid zewId, Guid wewId)
        {
            try
            {
                var records = await _context.KonfPolaczenie
                    .Where(p =>
                        p.ElementZewnetrznyId == zewId &&
                        p.ElementWewnetrznyId == wewId)
                    .ToListAsync();

                if (records == null || records.Count == 0)
                    return NotFound();

                return records;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd serwera: {ex.Message}");
            }
        }

        [HttpGet("find-shifts/{zewId:guid}/{wewId:guid}/{strona}")]
        public async Task<ActionResult<List<PrzesuniecieDto>>> GetShifts(Guid zewId, Guid wewId, string strona)
        {
            try
            {
                var query = _context.KonfPolaczenie
                .Where(p =>
                    p.ElementZewnetrznyId == zewId && p.ElementWewnetrznyId == wewId ||
                    p.ElementZewnetrznyId == wewId && p.ElementWewnetrznyId == zewId);

                    // Dodajemy warunek na stronę tylko jeśli strona NIE jest "ALL"
                    if (!string.Equals(strona, "ALL", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(p => p.StronaPolaczenia.ToLower() == strona.ToLower());
                    }

                var records = await query
                    .Select(p => new PrzesuniecieDto
                    {
                        PrzesuniecieX = p.PrzesuniecieX == 0 ? 1 : p.PrzesuniecieX,
                        PrzesuniecieY = p.PrzesuniecieY == 0 ? 1 : p.PrzesuniecieY,
                        PrzesuniecieXStycznej = p.PrzesuniecieXStycznej == 0 ? 1 : p.PrzesuniecieXStycznej,
                        PrzesuniecieYStycznej = p.PrzesuniecieYStycznej == 0 ? 1 : p.PrzesuniecieYStycznej,
                        Strona = p.StronaPolaczenia ?? "BRAK DANYCH W BAZIE" // Jeśli StronaPolaczenia jest null, ustawiamy "NaN"
                    })
                    .ToListAsync();

                if (records == null || records.Count == 0)
                {
                    // Brak wyników → zwracamy domyślną wartość 0,0
                    records = new List<PrzesuniecieDto>
                {
                    new PrzesuniecieDto
                    {
                        PrzesuniecieX = 1,
                        PrzesuniecieY = 1,
                        PrzesuniecieXStycznej = 1,
                        PrzesuniecieYStycznej = 1,
                        Strona = "NaN"
                    }
                };
                    Console.WriteLine($"Brak wyników, zwracam domyślną wartość 0,0 zew: {zewId} wew: {wewId} strona: {strona}");
                }

                return records;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd serwera: {ex.Message}");
            }
        }

        // ✅ GET: api/konfpolaczenie/row-id-system/{rowidSystem}
        [HttpGet("row-id-system/{rowidSystem:guid}")]
        public async Task<ActionResult<List<KonfPolaczenie>>> GetByRowIdSystem(Guid rowidSystem)
        {
            try
            {
                var records = await _context.KonfPolaczenie
                    .Where(p => p.RowIdSystem == rowidSystem)
                    .ToListAsync();

                if (records == null || records.Count == 0)
                    return NotFound("Nie znaleziono połączeń dla podanego systemu");

                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd serwera: {ex.Message}");
            }
        }

        // ✅ GET: api/konfpolaczenie/row-id-model/{rowidModelSelect}
        [HttpGet("row-id-model/{rowidModelSelect:guid}")]
        public async Task<ActionResult<List<KonfPolaczenie>>> GetByRowIdModel(Guid rowidModelSelect)
        {
            try
            {
                var records = await _context.KonfPolaczenie
                    .Where(p => p.RowIdModelu == rowidModelSelect)
                    .ToListAsync();

                if (records == null || records.Count == 0)
                    return NotFound("Nie znaleziono połączeń dla podanego systemu");

                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd serwera: {ex.Message}");
            }
        }

        // ✅ POST: api/konfpolaczenie
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] KonfPolaczenie newEntry)
        {
            if (newEntry == null) return BadRequest();

            newEntry.Id = 0; // upewnij się, że EF utworzy nowy wpis
            newEntry.RowId = Guid.NewGuid();

            _context.KonfPolaczenie.Add(newEntry);
            await _context.SaveChangesAsync();

            return Ok(newEntry);
        }

        [HttpDelete("{rowId}")]
        public async Task<ActionResult> DeleteAsync(string rowId)
        {
            var konfP = await _context.KonfPolaczenie.SingleOrDefaultAsync(b => b.RowId.ToString() == rowId);

            if (konfP == null)
            {
                return NotFound();
            }

            _context.KonfPolaczenie.Remove(konfP);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ PUT: api/konfpolaczenie
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] KonfPolaczenie updated)
        {
            if (updated == null || updated.Id == 0)
                return BadRequest("Nieprawidłowe dane");

            var existing = await _context.KonfPolaczenie
                .FirstOrDefaultAsync(p => p.Id == updated.Id);

            if (existing == null)
                return NotFound();

            // Aktualizuj właściwości
            existing.PrzesuniecieX = updated.PrzesuniecieX;
            existing.PrzesuniecieY = updated.PrzesuniecieY;
            existing.PrzesuniecieXStycznej = updated.PrzesuniecieXStycznej;
            existing.PrzesuniecieYStycznej = updated.PrzesuniecieYStycznej;
            existing.ZapisanyKat = updated.ZapisanyKat;
            existing.StronaPolaczenia = updated.StronaPolaczenia;
            existing.KatOd = updated.KatOd;
            existing.KatDo = updated.KatDo;
            existing.DodatkowyWarunek = updated.DodatkowyWarunek;
            existing.Uwagi = updated.Uwagi;
            existing.RysunekPrzekroju = updated.RysunekPrzekroju;
            existing.RysunekPrzekrojuStyczny = updated.RysunekPrzekrojuStyczny;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

    }
}
