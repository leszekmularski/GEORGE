using GEORGE.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/[controller]")]
[ApiController]
public class ZwrocSatusController : Controller
{
    private readonly ApplicationDbContext context;

    public ZwrocSatusController(ApplicationDbContext context)
    {
        this.context = context;
    }

    [HttpGet("{UzytkownikSQL}/{NazwaTabeli}")]
    public async Task<ActionResult<List<UprawnieniaPracownikaViewModel>>> GetUprawnienia(string UzytkownikSQL, string NazwaTabeli)
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
                .Where(x => x.Pracownik.UzytkownikSQL == UzytkownikSQL && x.Pracownik.Nieaktywny == false && x.Uprawnienie.TableName == NazwaTabeli)
                .OrderBy(x => x.Uprawnienie.Id)
                .Select(x => new UprawnieniaPracownikaViewModel
                {
                    Id = x.Uprawnienie.Id,
                    RowIdPracownicy = x.Uprawnienie.RowIdPracownicy,
                    TableName = x.Uprawnienie.TableName,
                    Odczyt = x.Uprawnienie.Odczyt,
                    Zapis = x.Uprawnienie.Zapis,
                    Zmiana = x.Uprawnienie.Zmiana,
                    Usuniecie = x.Uprawnienie.Usuniecie,
                    Administrator = x.Uprawnienie.Administrator,
                    RowIdRejestrejestrow = x.Uprawnienie.RowIdRejestrejestrow,
                    Uwagi = x.Uprawnienie.Uwagi,
                    Datautowrzenia = x.Uprawnienie.Datautowrzenia,
                    Imie = x.Pracownik.Imie,
                    Nazwisko = x.Pracownik.Nazwisko,
                    UzytkownikSQL = x.Pracownik.UzytkownikSQL,
                    HasloSQL = x.Pracownik.HasloSQL,
                    RowId = x.Pracownik.RowId,
                    RowIdDzialu = x.Pracownik.RowIdDzialu,
                    StanowiskoSystem = x.Pracownik.StanowiskoSystem,
                }).ToListAsync();

            if (uprawnienia == null || !uprawnienia.Any())
            {
                return NotFound();
            }

            return Ok(uprawnienia);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, ex.Message);
        }
    }
}
