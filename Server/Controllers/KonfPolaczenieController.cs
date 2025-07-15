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

        // ✅ GET: api/konfpolaczenie/find-by-elements/{zewId}/{wewId}
        [HttpGet("find-by-elements/{zewId:guid}/{wewId:guid}")]
        public async Task<ActionResult<KonfPolaczenie?>> GetByElementIds(Guid zewId, Guid wewId)
        {
            var record = await _context.KonfPolaczenie
                .FirstOrDefaultAsync(p =>
                    p.ElementZewnetrznyId == zewId &&
                    p.ElementWewnetrznyId == wewId);

            if (record is null)
                return NotFound();

            return record;
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
