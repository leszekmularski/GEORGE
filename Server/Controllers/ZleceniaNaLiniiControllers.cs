using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using GEORGE.Shared.ViewModels;
using System.ComponentModel.DataAnnotations.Schema;
using AntDesign;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZleceniaNaLiniiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ZleceniaNaLiniiController> _logger;

        public ZleceniaNaLiniiController(ApplicationDbContext context, ILogger<ZleceniaNaLiniiController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<ZleceniaNaLinii>>> GetLinieProdukcyjneAsync()
        {
            return await _context.ZleceniaNaLinii.OrderByDescending(e => e.DataZapisu).ToListAsync();
        }

        [HttpGet("GetDaneDoPlanowania/{rowIdLinii}/{juzZapisane}")]
        public async Task<List<DaneDoPlanowaniaViewModel>> GetDaneDoPlanowania(string rowIdLinii, string juzZapisane)
        {
            //var TabelaPowiazan = _context.ZleceniaNaLinii
            //    .OrderBy(x => x.DataZapisu)
            //    .ToList();

            //Console.WriteLine($"*************************** TABELA ZleceniaNaLinii Ilość rekordów: {TabelaPowiazan.Count()} ************************************");

            // Pobierz wszystkie zlecenia produkcyjne
            var zleceniaProdukcyjne = _context.ZleceniaProdukcyjne
                .Where(zp => zp.DataProdukcji > DateTime.Now.AddMonths(-12) && !zp.ZlecZrealizowane && zp.TypZamowienia == "Zlecenie")
                .OrderBy(x => x.DataZapisu)
                .ToList();

            // Pobierz wszystkie zlecenia produkcyjne wewnętrzne
            var zleceniaProdukcyjneWew = _context.ZleceniaProdukcyjneWew
                .Where(zw => zw.DataProdukcji > DateTime.Now.AddMonths(-12) && !zw.ZlecZrealizowane && zw.TypZamowienia == "Zlecenie")
                .OrderBy(x => x.DataZapisu)
                .ToList();

            var zleceniaProdukcyjneDto = zleceniaProdukcyjne.Select(zp => new ZleceniaProdukcyjneDto
            {
                Id = zp.Id,
                RowId = zp.RowId,
                TypZamowienia = zp.TypZamowienia,
                NumerZamowienia = zp.NumerZamowienia,
                NumerUmowy = zp.NumerUmowy,
                DataProdukcji = zp.DataProdukcji,
                DataWysylki = zp.DataWysylki,
                DataMontazu = zp.DataMontazu,
                Klient = zp.Klient,
                Adres = zp.Adres,
                Miejscowosc = zp.Miejscowosc,
                Telefon = zp.Telefon,
                Email = zp.Email,
                NazwaProduktu = zp.NazwaProduktu,
                NazwaProduktu2 = zp.NazwaProduktu2,
                KodProduktu = zp.KodProduktu,
                Ilosc = zp.Ilosc,
                Wartosc = zp.Wartosc,
                DataZapisu = zp.DataZapisu,
                KtoZapisal = zp.KtoZapisal,
                OstatniaZmiana = zp.OstatniaZmiana,
                Tags = zp.Tags,
                JednostkiNaZlecenie = zp.JednostkiNaZlecenie,
                ZlecZrealizowane = zp.ZlecZrealizowane,
                ZlecenieWewnetrzne = false,  // Ustawiamy false, ponieważ to nie jest zlecenie wewnętrzne
                ProcentWykonania = zp.ProcentWykonania,
                DataRozpProdukcji =zp.DataRozpProdukcji,
            }).ToList();

            var zleceniaProdukcyjneWewDto = zleceniaProdukcyjneWew.Select(zw => new ZleceniaProdukcyjneDto
            {
                Id = zw.Id,
                RowId = zw.RowId,
                TypZamowienia = zw.TypZamowienia,
                NumerZamowienia = zw.NumerZamowienia,
                NumerUmowy = zw.NumerUmowy,
                DataProdukcji = zw.DataProdukcji,
                DataWysylki = zw.DataWysylki,
                DataMontazu = zw.DataMontazu,
                Klient = zw.Klient,
                Adres = zw.Adres,
                Miejscowosc = zw.Miejscowosc,
                Telefon = zw.Telefon,
                Email = "biuro@george.pl",
                NazwaProduktu = zw.NazwaProduktu,
                NazwaProduktu2 = zw.NazwaProduktu2,
                KodProduktu = zw.KodProduktu,
                Ilosc = zw.Ilosc,
                Wartosc = zw.Wartosc,
                DataZapisu = zw.DataZapisu,
                KtoZapisal = zw.KtoZapisal,
                OstatniaZmiana = zw.OstatniaZmiana,
                Tags = zw.Tags,
                JednostkiNaZlecenie = zw.JednostkiNaZlecenie,
                ZlecZrealizowane = zw.ZlecZrealizowane,
                ZlecenieWewnetrzne = true,  // Ustawiamy true, ponieważ to jest zlecenie wewnętrzne
                ProcentWykonania = zw.ProcentWykonania,
                DataRozpProdukcji = zw.DataRozpProdukcji,
            }).ToList();

            // Połącz zlecenia produkcyjne z wewnętrznymi
            var wszystkieZleceniaProdukcyjneDto = zleceniaProdukcyjneDto.Concat(zleceniaProdukcyjneWewDto).ToList();

            // Pobierz wszystkie zlecenia na linii
            var zleceniaNaLinii = _context.ZleceniaNaLinii.ToList();

            // Pobierz linię produkcyjną na podstawie rowIdLinii
            var liniaProdukcyjna = _context.LinieProdukcyjne.FirstOrDefault(lp => lp.RowId == rowIdLinii);

            if (liniaProdukcyjna == null)
            {
                return new List<DaneDoPlanowaniaViewModel>();
            }

            if (juzZapisane == "NIE")
            {
                var filteredZleceniaProdukcyjne = wszystkieZleceniaProdukcyjneDto
                    .Where(zp => !zleceniaNaLinii.Any(znl => znl.RowIdZleceniaProdukcyjne == zp.RowId && znl.RowIdLinieProdukcyjne == rowIdLinii))
                    .ToList();
                //var filteredZleceniaProdukcyjne = wszystkieZleceniaProdukcyjneDto
                // .Where(zp => !zleceniaNaLinii.Any(znl => znl.RowIdLinieProdukcyjne == rowIdLinii))
                // .ToList();

                // Mapuj przefiltrowane zlecenia produkcyjne do modelu widoku
                var daneDoPlanowania = filteredZleceniaProdukcyjne.Select(async zp =>
                {
                    // Sprawdź, czy istnieje rekord w ZleceniaCzasNaLinieProd
                    var existingZlecenie = await _context.ZleceniaCzasNaLinieProd
                        .FirstOrDefaultAsync(z => z.RowIdZleceniaProdukcyjne == zp.RowId && z.RowIdLinieProdukcyjne == rowIdLinii);

                    // Oblicz czas na wykonanie zlecenia

                    float jednostkiNaZlecenie = existingZlecenie != null ? existingZlecenie.CzasNaZlecenie : zp.JednostkiNaZlecenie;
                    
                    var czasProdukcjiWDniach = (double)jednostkiNaZlecenie / liniaProdukcyjna.DziennaZdolnoscProdukcyjna;
                    var czasProdukcjiWHours = czasProdukcjiWDniach * 24;
                    string zam = "Zlecenie";

                    string rIdZlec = "---null---";
                    if (existingZlecenie == null)
                    {
                        zam = "BRAK_DANYCH";
                    }
                    else
                    {
                        rIdZlec = existingZlecenie.RowIdZleceniaProdukcyjne;
                    }

                    // Sprawdź, czas ustawienia produkcji na linii w ZleceniaNaLinii
                    var czasStartleceniaNaLinii = await _context.ZleceniaNaLinii
                        .FirstOrDefaultAsync(z => z.RowIdZleceniaProdukcyjne == rIdZlec && z.RowIdLinieProdukcyjne == rowIdLinii);

                    return new DaneDoPlanowaniaViewModel
                    {
                        PlanowanaDataRozpoczeciaProdukcji = DateTime.Now.AddDays(7 * 8), // Możesz dostosować to pole w zależności od wymagań
                        ZleceniaProdukcyjneDto = zp,
                        RzeczywistaDataRozpoczeciaProdukcjiNaLinii = czasStartleceniaNaLinii != null && czasStartleceniaNaLinii.DataRozpProdukcjiNaLinii != default(DateTime)
                        ? czasStartleceniaNaLinii.DataRozpProdukcjiNaLinii
                        : DateTime.MinValue,
                        Wyrob = zp.NazwaProduktu,
                        NumerZlecenia = zp.NumerZamowienia,
                        JednostkiNaZlecenie = jednostkiNaZlecenie,
                        DomyslnyCzasProdukcji = (int)czasProdukcjiWHours, // Przypisz czas produkcji w godzinach
                        RowIdLiniiProdukcyjnej = rowIdLinii, // Możesz dostosować to pole w zależności od wymagań
                        ZlecenieWewnetrzne = zp.ZlecenieWewnetrzne,
                        TypZamowienia = zam,
                    };
                }).Select(t => t.Result).ToList();

                return daneDoPlanowania;
            }
            else
            {
                // Filtruj zlecenia produkcyjne, aby wykluczyć te, które mają RowId równe RowIdZleceniaProdukcyjne w ZleceniaNaLinii
                var filteredZleceniaProdukcyjne = wszystkieZleceniaProdukcyjneDto
                    .Where(zp => zleceniaNaLinii.Any(znl => znl.RowIdZleceniaProdukcyjne == zp.RowId && znl.RowIdLinieProdukcyjne == rowIdLinii))
                    .ToList();
                //var filteredZleceniaProdukcyjne = wszystkieZleceniaProdukcyjneDto
                //.Where(zp => zleceniaNaLinii.Any(znl => znl.RowIdLinieProdukcyjne == rowIdLinii))
                //.ToList();


                // Mapuj przefiltrowane zlecenia produkcyjne do modelu widoku
                var daneDoPlanowania = filteredZleceniaProdukcyjne.Select(async zp =>
                {
                    // Sprawdź, czy istnieje rekord w ZleceniaCzasNaLinieProd
                    var existingZlecenie = await _context.ZleceniaCzasNaLinieProd
                        .FirstOrDefaultAsync(z => z.RowIdZleceniaProdukcyjne == zp.RowId && z.RowIdLinieProdukcyjne == rowIdLinii);

                    // Oblicz czas na wykonanie zlecenia
                    float jednostkiNaZlecenie = existingZlecenie != null ? existingZlecenie.CzasNaZlecenie : zp.JednostkiNaZlecenie;
                    var czasProdukcjiWDniach = (double)jednostkiNaZlecenie / liniaProdukcyjna.DziennaZdolnoscProdukcyjna;
                    var czasProdukcjiWHours = czasProdukcjiWDniach * 24;
                    string zam = "Zlecenie";

                    string rIdZlec = "---null---";
                    if (existingZlecenie == null)
                    {
                        zam = "BRAK_DANYCH";
                    }
                    else
                    {
                        rIdZlec = existingZlecenie.RowIdZleceniaProdukcyjne;
                    }

                    // Sprawdź, czas ustawienia produkcji na linii w ZleceniaNaLinii
                    var czasStartleceniaNaLinii = await _context.ZleceniaNaLinii
                        .FirstOrDefaultAsync(z => z.RowIdZleceniaProdukcyjne == rIdZlec && z.RowIdLinieProdukcyjne == rowIdLinii);

                    return new DaneDoPlanowaniaViewModel
                    {
                        PlanowanaDataRozpoczeciaProdukcji = zp.DataProdukcji.AddYears(1) < DateTime.Now ? DateTime.Now.AddDays(7 * 8) : zp.DataProdukcji, // Możesz dostosować to pole w zależności od wymagań -- AddYears(1) sztucznie
                        RzeczywistaDataRozpoczeciaProdukcji = zp.DataRozpProdukcji,
                        RzeczywistaDataRozpoczeciaProdukcjiNaLinii = czasStartleceniaNaLinii != null && czasStartleceniaNaLinii.DataRozpProdukcjiNaLinii != default(DateTime)
                        ? czasStartleceniaNaLinii.DataRozpProdukcjiNaLinii
                        : DateTime.MinValue,
                        ZleceniaProdukcyjneDto = zp,
                        Wyrob = zp.NazwaProduktu,
                        NumerZlecenia = zp.NumerZamowienia,
                        JednostkiNaZlecenie = jednostkiNaZlecenie,
                        DomyslnyCzasProdukcji = (int)czasProdukcjiWHours, // Przypisz czas produkcji w godzinach
                        RowIdLiniiProdukcyjnej = rowIdLinii, // Możesz dostosować to pole w zależności od wymagań
                        ZlecenieWewnetrzne = zp.ZlecenieWewnetrzne,
                        TypZamowienia = zam,
                        ProcentWykonania = zp.ProcentWykonania,
                    };
                }).Select(t => t.Result).ToList();

                return daneDoPlanowania;
            }
        }


        [HttpPost]
        public async Task<IActionResult> AddZleceniaNaLinii([FromBody] List<ZleceniaNaLinii> zleceniaNaLinii)
        {
            try
            {
                if (zleceniaNaLinii == null || !zleceniaNaLinii.Any())
                {
                    return BadRequest("Lista zleceń na linii jest pusta.");
                }

                var existingZlecenia = await _context.ZleceniaNaLinii
                    .ToListAsync(); // Pobierz wszystkie rekordy na raz

                // Porównanie z wykorzystaniem Linq po stronie klienta
                var newZlecenia = zleceniaNaLinii
                    .Where(z => !existingZlecenia.Any(ez => ez.RowIdLinieProdukcyjne == z.RowIdLinieProdukcyjne && ez.RowIdZleceniaProdukcyjne == z.RowIdZleceniaProdukcyjne))
                    .ToList();

                if (!newZlecenia.Any())
                {
                    return BadRequest("Żadne z nowych zleceń nie zostało dodane, ponieważ wszystkie już istnieją.");
                }
          
                await _context.ZleceniaNaLinii.AddRangeAsync(newZlecenia);
                await _context.SaveChangesAsync();

                //To do poprawy......
                // Pobranie identyfikatorów powiązanych zleceń
                var rowIdsZlecenia = zleceniaNaLinii.Select(z => z.RowIdZleceniaProdukcyjne).ToList();

                // Znalezienie pasujących rekordów w ZleceniaCzasNaLinieProd
                var zleceniaDoAktualizacji = await _context.ZleceniaCzasNaLinieProd
                    .Where(z => rowIdsZlecenia.Contains(z.RowIdZleceniaProdukcyjne) && z.CzasNaZlecenie == 0)
                    .ToListAsync();

                // Aktualizacja wartości CzasNaZlecenie
                foreach (var zlecenie in zleceniaDoAktualizacji)
                {
                    zlecenie.CzasNaZlecenie = 1000;
                }

                // Zapisanie zmian w bazie danych
                await _context.SaveChangesAsync();


                return Ok("Zlecenia zostały dodane.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Błąd. {ex.Message}");
            }
        }



        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateLinieProdukcyjneAsync(long id, ZleceniaNaLinii zl)
        {
            if (id != zl.Id)
            {
                return BadRequest("ID zlecenia nie pasuje do ID w żądaniu.");
            }

            _context.Entry(zl).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!LinieProdukcyjneExists(id))
                {
                    return NotFound("Nie znaleziono zlecenia na linii o podanym ID.");
                }
                else
                {
                    _logger.LogError(ex, "Błąd podczas aktualizacji ZleceniaNaLinii .");
                    return StatusCode(500, "Wystąpił błąd podczas przetwarzania żądania.");
                }
            }
        }

        private bool LinieProdukcyjneExists(long id)
        {
            return _context.ZleceniaNaLinii.Any(e => e.Id == id);
        }

        // Możesz dodać dodatkowe metody, np. delete, jeśli to konieczne
        [HttpDelete("{rowIdZlecenia}/{rowIdLinii}")]
        public async Task<ActionResult> DeleteLinieProdukcyjneAsync(string rowIdZlecenia, string rowIdLinii)
        {
            //// METODA USUNIĘCIA WSZYTKICH DANYCH!!!
            //// Pobierz wszystkie rekordy z tabeli
            //var wszystkieZleceniaNaLinii = _context.ZleceniaNaLinii.ToList();
            //// Usuń wszystkie rekordy
            //_context.ZleceniaNaLinii.RemoveRange(wszystkieZleceniaNaLinii);
            //// Zapisz zmiany do bazy danych
            //await _context.SaveChangesAsync();

            ///*************************************************************************************************************************


            // Znajdź zlecenie na podstawie rowId i rowIdLinii
            var zl = await _context.ZleceniaNaLinii
                .FirstOrDefaultAsync(z => z.RowIdZleceniaProdukcyjne == rowIdZlecenia && z.RowIdLinieProdukcyjne == rowIdLinii);

            if (zl == null)
            {
                if(rowIdLinii == "----")
                {
                   /// rowIdZlecenia = "1844f7d3-4eee-4d1e-b043-a8e22531b395"; //???  coś tu jest nie teges .....
                    var zlratuj = await _context.ZleceniaNaLinii.FirstOrDefaultAsync(z => z.RowIdZleceniaProdukcyjne == rowIdZlecenia);

                    if (zlratuj == null)
                    {
                        return NotFound("Nie znaleziono zlecenia o podanych RowId.");
                    }
                    else
                    {
                        //// METODA USUNIĘCIA WSZYTKICH DANYCH!!!
                        //// Pobierz wszystkie rekordy z tabeli
                        //var wszystkieZleceniaNaLinii = _context.ZleceniaNaLinii.ToList();
                        //// Usuń wszystkie rekordy
                        //_context.ZleceniaNaLinii.RemoveRange(wszystkieZleceniaNaLinii);
                        //// Zapisz zmiany do bazy danych
                        //await _context.SaveChangesAsync();

                        _context.ZleceniaNaLinii.Remove(zlratuj);
                        await _context.SaveChangesAsync();

                        Console.WriteLine("Usunięto zlecenie z dnia ze wszystkich linii " + zlratuj.DataZapisu);

                    }
                }
                else
                {
                    return NotFound("Nie znaleziono zlecenia o podanych RowId.");
                }
            }
            else
            {
                // Wyświetl numer zamówienia w konsoli
                Console.WriteLine("Usunięto zlecenie z dnia " + zl.DataZapisu);

                _context.ZleceniaNaLinii.Remove(zl);
                await _context.SaveChangesAsync();

            }

            return Ok("Zlecenie na linii usunięto.");
        }

    }

}
