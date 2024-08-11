using Microsoft.AspNetCore.Mvc;
using GEORGE.Shared.Models;
using Microsoft.EntityFrameworkCore;
using GEORGE.Server;
using System.Net;


namespace GEORGE.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UprawnieniaController : Controller
    {

        private readonly ApplicationDbContext context;

        public UprawnieniaController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Uprawnieniapracownika>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<Uprawnieniapracownika>>> List()
        {
            if (context == null)
            {
                return NotFound();
            }

            var uprawnieniapracownika = await context.Uprawnieniapracownika.ToListAsync();
            return Ok(uprawnieniapracownika);
        }

        [HttpGet("{RowId}")]
        public async Task<ActionResult<List<UprawnieniaPracownikaViewModel>>> GetUprawnienia(string RowId)
        {
            try
            {

                var uprawnienia = await context.Uprawnieniapracownika
                    .Join(context.Pracownicy,
                        uprawnienie => uprawnienie.RowIdPracownicy,
                        pracownik => pracownik.RowId,
                        (uprawnienie, pracownik) => new
                        {
                            Uprawnienie = uprawnienie,
                            Pracownik = pracownik
                        })
                    .Where(x => x.Pracownik.RowId == RowId)
                    .Select(x => new
                    {
                        x.Uprawnienie.Id,
                        x.Uprawnienie.RowIdPracownicy,
                        x.Uprawnienie.TableName,
                        x.Uprawnienie.Odczyt,
                        x.Uprawnienie.Zapis,
                        x.Uprawnienie.Zmiana,
                        x.Uprawnienie.Usuniecie,
                        x.Uprawnienie.Administrator,
                        x.Uprawnienie.RowIdRejestrejestrow,
                        x.Uprawnienie.Uwagi,
                        x.Uprawnienie.Datautowrzenia,
                        x.Uprawnienie.Autorzmiany,
                        x.Pracownik.Imie,
                        x.Pracownik.Nazwisko,
                        x.Pracownik.UzytkownikSQL,
                        x.Pracownik.HasloSQL,
                        x.Pracownik.Nieaktywny,
                        x.Pracownik.RowId,
                    })
                    .ToListAsync();

                var pracownik = await context.Pracownicy
                    .FirstOrDefaultAsync(p => p.RowId == RowId);

                if (pracownik == null)
                {
                    return NotFound();
                }

                return Ok(uprawnienia);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }

        }

        [HttpPost]
        public async Task<ActionResult<Uprawnieniapracownika>> CreateAsync(Uprawnieniapracownika uprPacownika)
        {
            if (uprPacownika.Id != 0)
            {
                return BadRequest();
            }

            context.Uprawnieniapracownika.Add(uprPacownika);

            await context.SaveChangesAsync();

            return CreatedAtAction("Get", new { uprPacownika.Id }, uprPacownika);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var uprPacownika = await context.Uprawnieniapracownika.SingleOrDefaultAsync(b => b.Id.Equals(id));

            if (uprPacownika == null)
            {
                return NotFound();
            }

            context.Uprawnieniapracownika.Remove(uprPacownika);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut]
        public async Task<ActionResult> Update(Uprawnieniapracownika uprPacownika)
        {
            if (await context.Uprawnieniapracownika.AsNoTracking().SingleOrDefaultAsync(b => b.Id == uprPacownika.Id) == null)
            {
                return NotFound();
            }

            context.Entry(uprPacownika).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Pracownicy>> Put(long id, Uprawnieniapracownika uprPacownika)
        {
            if (id != uprPacownika.Id)
            {
                return BadRequest();
            }

            context.Entry(uprPacownika).State = EntityState.Modified;

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

            return Ok(uprPacownika);
        }

        private bool PracownikExists(long id)
        {
            return context.Pracownicy.Any(e => e.Id == id);
        }
    }
}
