using GEORGE.Shared.Models;            // <-- lub odpowiednią dla Drzwi
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrzwiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DrzwiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Drzwi>>> GetAll()
        {
            return await _context.Drzwi.OrderBy(e => e.Typ).ToListAsync();
        }

        // ✅ GET: api/Drzwi/find-by-all
        [HttpGet("find-by-all/{wysokosc:double}/{szerokosc:double}/{grubosc:double}/{typ}")]
        public async Task<ActionResult<List<Drzwi>>> GetByElementIds(
            double wysokosc,
            double szerokosc,
            double grubosc,
            string typ)
        {
            try
            {
                const double tolerancja = 20.0;

                var records = await _context.Drzwi
                    .Where(p =>
                        Math.Abs(p.Wysokosc - wysokosc) <= tolerancja &&
                        Math.Abs(p.Szerokosc - szerokosc) <= tolerancja &&
                        Math.Abs(p.Grubosc - grubosc) <= tolerancja &&
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

        // ✅ POST: api/Drzwi
        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Drzwi newEntry)
        {
            if (newEntry == null) return BadRequest();

            newEntry.Id = 0; // upewnij się, że EF utworzy nowy wpis
            newEntry.RowId = Guid.NewGuid();

            _context.Drzwi.Add(newEntry);
            await _context.SaveChangesAsync();

            return Ok(newEntry);
        }

        [HttpDelete("{rowId}")]
        public async Task<ActionResult> DeleteAsync(string rowId)
        {
            var konfP = await _context.Drzwi.SingleOrDefaultAsync(b => b.RowId.ToString() == rowId);

            if (konfP == null)
            {
                return NotFound();
            }

            _context.Drzwi.Remove(konfP);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ PUT: api/Drzwi
        [HttpPut]
        public async Task<ActionResult> Update([FromBody] Drzwi updated)
        {
            if (updated == null || updated.Id == 0)
                return BadRequest("Nieprawidłowe dane");

            var existing = await _context.Drzwi
                .FirstOrDefaultAsync(p => p.Id == updated.Id);

            if (existing == null)
                return NotFound();

           existing.Typ = updated.Typ;
            existing.Uwagi = updated.Uwagi;
            existing.Wysokosc = updated.Wysokosc;
            existing.Szerokosc = updated.Szerokosc;
            existing.Grubosc = updated.Grubosc;
            existing.WysokoscProgu = updated.WysokoscProgu;   
            existing.RodzajWypelnienia = updated.RodzajWypelnienia;
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
