using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace GEORGE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PracownicyController : Controller
    {
        private readonly ApplicationDbContext context;

        public PracownicyController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Pracownicy>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<Pracownicy>>> ListAsync()
        {
            try
            {
                // Sortowanie danych po polu Nazwisko przed pobraniem ich z bazy danych
                var pracownicy = await context.Pracownicy
                    .OrderBy(p => p.Nazwisko)
                    .ToListAsync();

                return Ok(pracownicy);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
        }


        [HttpGet("{user}/{add}")]
        public async Task<ActionResult<Pracownicy>> GetAsync(string user, string add)
        {
            //Uzytkownik może być tylko jeden
            //var XlistResult = await context.Pracownicy.Where(p => p.HasloSQL == pass && p.UzytkownikSQL.ToLower() == user.ToLower() && p.Nieaktywny == false).ToListAsync();
            var XlistResult = await context.Pracownicy.Where(p => p.UzytkownikSQL.ToLower() == user.ToLower() && p.Nieaktywny == false).ToListAsync();
            if (XlistResult == null)
            {
                return NotFound();
            }

            return Ok(XlistResult);

        }

        [HttpGet("{user}/{DayOfYear}/{pass}")]
        public async Task<ActionResult<Pracownicy>> GetAsynclog(string user, string DayOfYear, string pass)
        {
            //if(DayOfYear != DateTime.Now.DayOfYear.ToString())
            // {
            //    return NotFound();
            // }
            //Uzytkownik może być tylko jeden
            var XlistResult = await context.Pracownicy.Where(p => p.HasloSQL == pass && p.UzytkownikSQL.ToLower() == user.ToLower() && p.Nieaktywny == false).ToListAsync();
            //var XlistResult = await context.Pracownicy.Where(p => p.UzytkownikSQL.ToLower() == user.ToLower() && p.Nieaktywny == false).ToListAsync();
            if (XlistResult == null)
            {
                return NotFound();
            }

            return Ok(XlistResult);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pracownicy>> GetAsync(int id)
        {

            var Pracownicy = await context.Pracownicy.SingleOrDefaultAsync(b => b.Id.Equals(id));

            if (Pracownicy == null)
            {
                return NotFound();
            }

            return Ok(Pracownicy);
        }

        [HttpPost]
        public async Task<ActionResult<Pracownicy>> CreateAsync(Pracownicy Pracownicy)
        {
            if (Pracownicy.Id != 0)
            {
                return BadRequest();
            }

            context.Pracownicy.Add(Pracownicy);

            await context.SaveChangesAsync();

            return CreatedAtAction("Get", new { Pracownicy.Id }, Pracownicy);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var Pracownicy = await context.Pracownicy.SingleOrDefaultAsync(b => b.Id.Equals(id));

            if (Pracownicy == null)
            {
                return NotFound();
            }

            context.Pracownicy.Remove(Pracownicy);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> Update(Pracownicy Pracownicy)
        {
            if (await context.Pracownicy.AsNoTracking().SingleOrDefaultAsync(b => b.Id == Pracownicy.Id) == null)
            {
                return NotFound();
            }

            context.Entry(Pracownicy).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Pracownicy>> Put(long id, Pracownicy pracownik)
        {
            if (id != pracownik.Id)
            {
                return BadRequest();
            }

            context.Entry(pracownik).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PracownikExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(pracownik);
        }

        private bool PracownikExists(long id)
        {
            return context.Pracownicy.Any(e => e.Id == id);
        }

    }
}
