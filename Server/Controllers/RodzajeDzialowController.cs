using GEORGE.Server;
using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;


namespace PSSE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RodzajeDzialowController : Controller
    {
        private readonly ApplicationDbContext context;

        public RodzajeDzialowController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RodzajeDzialow>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<RodzajeDzialow>>> ListAsync()
        {
            try
            {
                // Sortowanie danych po polu NazwaDzialu przed pobraniem ich z bazy danych
                var rodzDzialy = await context.RodzajeDzialow
                    .OrderBy(rd => rd.NazwaDzialu)
                    .ToListAsync();

                return Ok(rodzDzialy);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return NotFound();
            }
        }


        [HttpGet("{RowIdDzialu}")]
        public async Task<ActionResult<RodzajeDzialow>> GetAsync(string RowIdDzialu)
        {
            var rodzDzialy = await context.RodzajeDzialow.SingleOrDefaultAsync(b => b.RowId.Equals(RowIdDzialu));

            if (rodzDzialy == null)
            {
                return NotFound();
            }

            return Ok(rodzDzialy);
        }

        [HttpPost]
        public async Task<ActionResult<RodzajeDzialow>> CreateAsync(RodzajeDzialow rodzDzialy)
        {
            if (rodzDzialy.Id != 0)
            {
                return BadRequest();
            }

            context.RodzajeDzialow.Add(rodzDzialy);

            await context.SaveChangesAsync();

            return CreatedAtAction("Get", new { rodzDzialy.Id }, rodzDzialy);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var rodzDzialy = await context.RodzajeDzialow.SingleOrDefaultAsync(b => b.Id.Equals(id));

            if (rodzDzialy == null)
            {
                return NotFound();
            }

            context.RodzajeDzialow.Remove(rodzDzialy);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> Update(RodzajeDzialow rodzDzialy)
        {
            if (await context.RodzajeDzialow.AsNoTracking().SingleOrDefaultAsync(b => b.Id == rodzDzialy.Id) == null)
            {
                return NotFound();
            }

            context.Entry(rodzDzialy).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
