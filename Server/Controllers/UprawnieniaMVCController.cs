using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GEORGE.Server;
using System.Net;


namespace GEORGE.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UprawnieniaMVCController : Controller
    {

        private readonly ApplicationDbContext context;

        public UprawnieniaMVCController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UprawnieniaPracownikaViewModel>), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<List<UprawnieniaPracownikaViewModel>>> List()
        {
            if (context == null)
            {
                return NotFound();
            }
            //var uprawnieniapracownika = await context.Uprawnieniapracownika.Where(x => x.RowId == "???????").ToListAsync();
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
                        x.Uprawnienie.RowId,
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

    }
}
