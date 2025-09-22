using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using GEORGE.Shared.Models;            // <-- lub odpowiednią dla KonfPolaczenie

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
                var records = await _context.KonfPolaczenie
                    .Where(p =>
                        (p.ElementZewnetrznyId == zewId && p.ElementWewnetrznyId == wewId ||
                         p.ElementZewnetrznyId == wewId && p.ElementWewnetrznyId == zewId) &&
                        p.StronaPolaczenia.ToLower() == strona.ToLower()
                    )
                    .Select(p => new PrzesuniecieDto
                    {
                        PrzesuniecieX = p.PrzesuniecieX,
                        PrzesuniecieY = p.PrzesuniecieY
                    })
                    .ToListAsync();

                if (records == null || records.Count == 0)
                {
                    // Brak wyników → zwracamy domyślną wartość 0,0
                    records = new List<PrzesuniecieDto>
            {
                new PrzesuniecieDto
                {
                    PrzesuniecieX = 0,
                    PrzesuniecieY = 0
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
            existing.ZapisanyKat = updated.ZapisanyKat;
            existing.StronaPolaczenia = updated.StronaPolaczenia;
            existing.KatOd = updated.KatOd;
            existing.KatDo = updated.KatDo;
            existing.DodatkowyWarunek = updated.DodatkowyWarunek;
            existing.Uwagi = updated.Uwagi;
            existing.RysunekPrzekroju = updated.RysunekPrzekroju;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        public class PrzesuniecieDto
        {
            public double PrzesuniecieX { get; set; }
            public double PrzesuniecieY { get; set; }
        }

    }
}
