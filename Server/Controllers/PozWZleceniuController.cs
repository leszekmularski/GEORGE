using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace GEORGE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PozWZleceniuController : Controller
    {
        private readonly ApplicationDbContext context;

        public PozWZleceniuController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PozDoZlecen>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<PozDoZlecen>>> ListAsync()
        {
            try
            {
                // Sortowanie danych po polu Nazwisko przed pobraniem ich z bazy danych
                var PozDoZlecen = await context.PozDoZlecen
                    .OrderBy(p => p.RowIdZlecenia)
                    .ToListAsync();

                return Ok(PozDoZlecen);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
        }

        [HttpGet("GetDaneDoZlecenia/{rowIdZlecenia}")]
        public async Task<ActionResult<PozDoZlecen>> GetAsync(string rowIdZlecenia)
        {
            //Uzytkownik może być tylko jeden
            //var XlistResult = await context.PozDoZlecen.Where(p => p.HasloSQL == pass && p.UzytkownikSQL.ToLower() == user.ToLower() && p.Nieaktywny == false).ToListAsync();
            var XlistResult = await context.PozDoZlecen.Where(p => p.RowIdZlecenia.ToLower() == rowIdZlecenia).ToListAsync();
            if (XlistResult == null)
            {
                return NotFound();
            }

            return Ok(XlistResult);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PozDoZlecen>> GetAsync(int id)
        {

            var PozDoZlecen = await context.PozDoZlecen.SingleOrDefaultAsync(b => b.Id.Equals(id));

            if (PozDoZlecen == null)
            {
                return NotFound();
            }

            return Ok(PozDoZlecen);
        }

        [HttpPost]
        public async Task<ActionResult<PozDoZlecen>> CreateAsync(PozDoZlecen PozDoZlecen)
        {
            if (PozDoZlecen.Id != 0)
            {
                return BadRequest();
            }

            context.PozDoZlecen.Add(PozDoZlecen);

            await context.SaveChangesAsync();

            return CreatedAtAction("Get", new { PozDoZlecen.Id }, PozDoZlecen);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var PozDoZlecen = await context.PozDoZlecen.SingleOrDefaultAsync(b => b.Id.Equals(id));

            if (PozDoZlecen == null)
            {
                return NotFound();
            }

            context.PozDoZlecen.Remove(PozDoZlecen);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("rowid/{rowid}")]
        public async Task<ActionResult> DeleteAllAsync(string rowid)
        {
            // Znajdź wszystkie rekordy, które mają RowIdZlecenia równe przekazanemu rowid
            var pozDoZlecenList = await context.PozDoZlecen.Where(p => p.RowIdZlecenia == rowid).ToListAsync();

            // Sprawdź, czy jakiekolwiek rekordy zostały znalezione
            if (pozDoZlecenList == null)
            {
                return NotFound();
            }

            if (pozDoZlecenList.Count == 0)
            {
                return NoContent();
            }

            // Usuń wszystkie znalezione rekordy
            context.PozDoZlecen.RemoveRange(pozDoZlecenList);
            await context.SaveChangesAsync();

            // Zwróć odpowiedź NoContent (204) po pomyślnym usunięciu
            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> Update(PozDoZlecen PozDoZlecen)
        {
            if (await context.PozDoZlecen.AsNoTracking().SingleOrDefaultAsync(b => b.Id == PozDoZlecen.Id) == null)
            {
                return NotFound();
            }

            context.Entry(PozDoZlecen).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PozDoZlecen>> Put(long id, PozDoZlecen pozWZleceniu)
        {
            if (id != pozWZleceniu.Id)
            {
                return BadRequest();
            }

            context.Entry(pozWZleceniu).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!pozWZleceniuExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(pozWZleceniu);
        }

        private bool pozWZleceniuExists(long id)
        {
            return context.PozDoZlecen.Any(e => e.Id == id);
        }

    }
}
