using GEORGE.Shared.Models;            // <-- lub odpowiednią dla Schody
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchodyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SchodyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Schody>>> GetAll()
        {
            return await _context.Schody.OrderBy(e => e.Typ).ToListAsync();
        }

        // ✅ GET: api/Schody/find-by-all
        [HttpGet("find-by-all/{wysokosc:double}/{szerokosc:double}/{glebokosc:double}/{typ}")]
        public async Task<ActionResult<List<Schody>>> GetByElementIds(
            double wysokosc,
            double szerokosc,
            double glebokosc,
            string typ)
        {
            try
            {
                const double tolerancja = 20.0;

                var records = await _context.Schody
                    .Where(p =>
                        Math.Abs(p.Wysokosc - wysokosc) <= tolerancja &&
                        Math.Abs(p.Szerokosc - szerokosc) <= tolerancja &&
                        Math.Abs(p.Glebokosc - glebokosc) <= tolerancja &&
                        p.Typ == typ)
                    .ToListAsync();

                if (records.Count == 0)
                    return NotFound();

                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Błąd serwera: {ex.Message}");
            }
        }

        // ✅ POST: api/Schody
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Schody newEntry)
        {
            if (newEntry == null) return BadRequest();

            newEntry.Id = 0; // upewnij się, że EF utworzy nowy wpis
            newEntry.RowId = Guid.NewGuid();

            _context.Schody.Add(newEntry);
            await _context.SaveChangesAsync();

            return Ok(newEntry);
        }

        [HttpDelete("{rowId}")]
        public async Task<ActionResult> DeleteAsync(string rowId)
        {
            var konfP = await _context.Schody.SingleOrDefaultAsync(b => b.RowId.ToString() == rowId);

            if (konfP == null)
            {
                return NotFound();
            }

            _context.Schody.Remove(konfP);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ PUT: api/Schody
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] Schody updated)
        {
            if (updated == null || updated.Id == 0)
                return BadRequest("Nieprawidłowe dane");

            var existing = await _context.Schody
                .FirstOrDefaultAsync(p => p.Id == updated.Id);

            if (existing == null)
                return NotFound();

           existing.Typ = updated.Typ;
            existing.Uwagi = updated.Uwagi;
            existing.Wysokosc = updated.Wysokosc;
            existing.Szerokosc = updated.Szerokosc;
            existing.Glebokosc = updated.Glebokosc;
            existing.GlebokoscZabieg1 = updated.GlebokoscZabieg1;   
            existing.GlebokoscZabieg2 = updated.GlebokoscZabieg2;
            existing.SzerokoscZabieg1 = updated.SzerokoscZabieg1;
            existing.SzerokoscZabieg2 = updated.SzerokoscZabieg2;
            existing.RysunekPogladowy = updated.RysunekPogladowy;
            existing.RowIdPliku = updated.RowIdPliku;
            existing.Wycofany_z_produkcji = updated.Wycofany_z_produkcji;
            existing.DataZapisu = updated.DataZapisu;
            existing.KtoZapisal = updated.KtoZapisal;

        await _context.SaveChangesAsync();

            return Ok(existing);
        }

    }
}
