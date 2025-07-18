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

        // ✅ GET: api/konfpolaczenie/row-id-system/{rowidSystem}
        [HttpGet("row-id-model/{rowidModel:guid}")]
        public async Task<ActionResult<List<KonfPolaczenie>>> GetByRowIdModel(Guid rowidModel)
        {
            try
            {
                var records = await _context.KonfPolaczenie
                    .Where(p => p.RowIdModelu == rowidModel)
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
    }
}
