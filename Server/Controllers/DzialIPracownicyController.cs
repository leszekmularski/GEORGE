using GEORGE.Server;
using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace PSSE.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class DzialIPracownicyController : Controller
    {

        private readonly ApplicationDbContext context;

        public DzialIPracownicyController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public Task<ActionResult<List<DzialPracownikaViewModel>>> GetUprawnienia()
        {
            try
            {
                var pracownicyWDzialach = context.RodzajeDzialow
                    .Join(
                        context.Pracownicy,
                        rodzajDzialu => rodzajDzialu.RowId,
                        pracownik => pracownik.RowIdDzialu,
                        (rodzajDzialu, pracownik) => new { RodzajDzialu = rodzajDzialu, Pracownik = pracownik }
                    )
                    .Select(result => new DzialPracownikaViewModel
                    {
                        Id = result.Pracownik.Id,
                        RowId = result.Pracownik.RowId,
                        RowIdPracownicy = result.Pracownik.RowIdDzialu,
                        Imie = result.Pracownik.Imie,
                        Nazwisko = result.Pracownik.Nazwisko,
                        Uwagi = result.Pracownik.Uwagi,
                        Telefon = result.Pracownik.Telefon,
                        Datautowrzenia = result.Pracownik.Datautowrzenia,
                        RowIdDzialu = result.RodzajDzialu.RowId,
                        NazwaDzialu = result.RodzajDzialu.NazwaDzialu,
                        ImieNazwisko = result.Pracownik.Imie + " " + result.Pracownik.Nazwisko,
                        UserSQL = result.Pracownik.UzytkownikSQL,
                    })
                    .ToList();


                return Task.FromResult<ActionResult<List<DzialPracownikaViewModel>>>(new JsonResult(pracownicyWDzialach));


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Task.FromResult<ActionResult<List<DzialPracownikaViewModel>>>(NotFound());
            }

        }

        [HttpGet("{RowIdDzialu}")]
        public ActionResult<DzialPracownikaViewModel> GetAsyncDzial(string RowIdDzialu)
        {
            var pracownicyWDzialach = context.RodzajeDzialow
                .Join(
                    context.Pracownicy,
                    rodzajDzialu => rodzajDzialu.RowId,
                    pracownik => pracownik.RowIdDzialu,
                    (rodzajDzialu, pracownik) => new { RodzajDzialu = rodzajDzialu, Pracownik = pracownik }
                )
                .Where(result => result.RodzajDzialu.RowId == RowIdDzialu)
                .Select(result => new DzialPracownikaViewModel
                {
                    Id = result.Pracownik.Id,
                    RowId = result.Pracownik.RowId,
                    RowIdPracownicy = result.Pracownik.RowIdDzialu,
                    Imie = result.Pracownik.Imie,
                    Nazwisko = result.Pracownik.Nazwisko,
                    Uwagi = result.Pracownik.Uwagi,
                    Telefon = result.Pracownik.Telefon,
                    Datautowrzenia = result.Pracownik.Datautowrzenia,
                    RowIdDzialu = result.RodzajDzialu.RowId,
                    NazwaDzialu = result.RodzajDzialu.NazwaDzialu,
                    ImieNazwisko = result.Pracownik.Imie + " " + result.Pracownik.Nazwisko,
                    UserSQL = result.Pracownik.UzytkownikSQL,
                })
                .ToList();

            if (pracownicyWDzialach == null || pracownicyWDzialach.Count == 0)
            {
                return NotFound();
            }

            return Ok(pracownicyWDzialach);
        }

    }
}
