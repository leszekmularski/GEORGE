using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GEORGE.Server.Controllers
{
    [ApiController]
    [Route("api/pakiety-szybowe")]
    public class PakietySzyboweController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PakietySzyboweController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/pakiety-szybowe?systemId={guid}
        [HttpGet]
        public async Task<ActionResult<List<PakietSzybowy>>> GetAll([FromQuery] Guid? systemId = null)
        {
            var query = _context.PakietySzybowe
                .Include(p => p.Warstwy.OrderBy(w => w.Kolejnosc))
                .AsQueryable();

            if (systemId.HasValue && systemId.Value != Guid.Empty)
                query = query.Where(p => p.SystemOkienId == systemId.Value || p.SystemOkienId == null);

            var list = await query
                .OrderBy(p => p.Nazwa)
                .AsNoTracking()
                .ToListAsync();

            return Ok(list);
        }

        // GET api/pakiety-szybowe/{id}
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PakietSzybowy>> GetById(Guid id)
        {
            var entity = await _context.PakietySzybowe
                .Include(p => p.Warstwy.OrderBy(w => w.Kolejnosc))
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return entity == null ? NotFound() : Ok(entity);
        }

        // POST api/pakiety-szybowe
        [HttpPost]
        public async Task<ActionResult<PakietSzybowy>> Create([FromBody] PakietSzybowy dto)
        {
            if (dto == null) return BadRequest();

            var entity = new PakietSzybowy
            {
                Id = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
                Nazwa = dto.Nazwa ?? "",
                Opis = dto.Opis ?? "",
                Aktywny = dto.Aktywny,
                Ug = dto.Ug,
                Lt = dto.Lt,
                g = dto.g,
                Rw = dto.Rw,
                CenaZaM2 = dto.CenaZaM2,
                SystemOkienId = dto.SystemOkienId,          // ✅ FIX 1
                DataUtworzenia = DateTime.UtcNow,
                DataModyfikacji = DateTime.UtcNow,
                Warstwy = new()
            };

            int order = 0;
            foreach (var w in dto.Warstwy ?? new())
            {
                entity.Warstwy.Add(new WarstwaSzyby
                {
                    Id = w.Id == Guid.Empty ? Guid.NewGuid() : w.Id,
                    PakietSzybowyId = entity.Id,
                    Kolejnosc = order++,
                    Typ = w.Typ,
                    Grubosc = w.Grubosc,
                    RodzajSzyby = w.RodzajSzyby,
                    Powloka = w.Powloka,
                    Kolor = w.Kolor,
                    RodzajRamki = w.RodzajRamki,
                    Gaz = w.Gaz,
                    Producent = w.Producent,
                    Uwagi = w.Uwagi,
                });
            }

            _context.PakietySzybowe.Add(entity);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
        }

        // PUT api/pakiety-szybowe/{id}
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<PakietSzybowy>> Update(Guid id, [FromBody] PakietSzybowy dto)
        {
            if (dto == null) return BadRequest();

            var entity = await _context.PakietySzybowe
                .Include(p => p.Warstwy)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity == null) return NotFound();

            entity.Nazwa = dto.Nazwa ?? "";
            entity.Opis = dto.Opis ?? "";
            entity.Aktywny = dto.Aktywny;
            entity.Ug = dto.Ug;
            entity.Lt = dto.Lt;
            entity.g = dto.g;
            entity.Rw = dto.Rw;
            entity.CenaZaM2 = dto.CenaZaM2;
            entity.SystemOkienId = dto.SystemOkienId;      // ✅ FIX 3
            entity.DataModyfikacji = DateTime.UtcNow;

            // Podmień warstwy
            _context.WarstwySzyby.RemoveRange(entity.Warstwy);
            entity.Warstwy.Clear();

            int order = 0;
            foreach (var w in dto.Warstwy ?? new())
            {
                entity.Warstwy.Add(new WarstwaSzyby
                {
                    Id = Guid.NewGuid(),
                    PakietSzybowyId = id,
                    Kolejnosc = order++,
                    Typ = w.Typ,
                    Grubosc = w.Grubosc,
                    RodzajSzyby = w.RodzajSzyby,
                    Powloka = w.Powloka,
                    Kolor = w.Kolor,
                    RodzajRamki = w.RodzajRamki,
                    Gaz = w.Gaz,
                    Producent = w.Producent,
                    Uwagi = w.Uwagi,
                });
            }

            await _context.SaveChangesAsync();
            return Ok(entity);
        }

        // DELETE api/pakiety-szybowe/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _context.PakietySzybowe
                .Include(p => p.Warstwy)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (entity == null) return NotFound();

            _context.PakietySzybowe.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST api/pakiety-szybowe/RESET — przywraca 3 domyślne pakiety
        [HttpPost("RESET")]
        public async Task<IActionResult> Reset()
        {
            _context.PakietySzybowe.RemoveRange(_context.PakietySzybowe);
            await _context.SaveChangesAsync();

            var seed = new List<PakietSzybowy>
            {
                new()
                {
                    Nazwa = "4/16/4 - standard",
                    Opis = "Podstawowy pakiet jednokomorowy z powietrzem",
                    Ug = 2.8, Lt = 81, g = 0.76, Rw = 30, CenaZaM2 = 85,
                    // SystemOkienId = null (globalny) — domyślnie null
                    Warstwy = new()
                    {
                        new() { Typ = TypWarstwy.Szyba, Grubosc = 4, RodzajSzyby = RodzajSzyby.Float, Kolejnosc = 0 },
                        new() { Typ = TypWarstwy.Ramka, Grubosc = 16, RodzajRamki = RodzajRamki.Aluminiowa, Gaz = GazWRamce.Powietrze, Kolejnosc = 1 },
                        new() { Typ = TypWarstwy.Szyba, Grubosc = 4, RodzajSzyby = RodzajSzyby.Float, Kolejnosc = 2 }
                    }
                },
                new()
                {
                    Nazwa = "4/16/4 niskoemisyjna",
                    Opis = "Jednokomorowy z powłoką XN i argonem",
                    Ug = 1.1, Lt = 78, g = 0.62, Rw = 31, CenaZaM2 = 140,
                    Warstwy = new()
                    {
                        new() { Typ = TypWarstwy.Szyba, Grubosc = 4, RodzajSzyby = RodzajSzyby.Float, Kolejnosc = 0 },
                        new() { Typ = TypWarstwy.Ramka, Grubosc = 16, RodzajRamki = RodzajRamki.WarmEdge, Gaz = GazWRamce.Argon, Kolejnosc = 1 },
                        new() { Typ = TypWarstwy.Szyba, Grubosc = 4, RodzajSzyby = RodzajSzyby.Niskoemisyjne, Powloka = "XN", Kolejnosc = 2 }
                    }
                },
                new()
                {
                    Nazwa = "4/14/4/14/4 trzyszybowe",
                    Opis = "Standardowe trzyszybowe, dwa argonowe wypełnienia",
                    Ug = 0.7, Lt = 72, g = 0.52, Rw = 32, CenaZaM2 = 220,
                    Warstwy = new()
                    {
                        new() { Typ = TypWarstwy.Szyba, Grubosc = 4, RodzajSzyby = RodzajSzyby.Float, Kolejnosc = 0 },
                        new() { Typ = TypWarstwy.Ramka, Grubosc = 14, RodzajRamki = RodzajRamki.WarmEdge, Gaz = GazWRamce.Argon, Kolejnosc = 1 },
                        new() { Typ = TypWarstwy.Szyba, Grubosc = 4, RodzajSzyby = RodzajSzyby.Niskoemisyjne, Kolejnosc = 2 },
                        new() { Typ = TypWarstwy.Ramka, Grubosc = 14, RodzajRamki = RodzajRamki.WarmEdge, Gaz = GazWRamce.Argon, Kolejnosc = 3 },
                        new() { Typ = TypWarstwy.Szyba, Grubosc = 4, RodzajSzyby = RodzajSzyby.Niskoemisyjne, Kolejnosc = 4 }
                    }
                }
            };

            foreach (var p in seed)
            {
                foreach (var w in p.Warstwy) w.PakietSzybowyId = p.Id;
                _context.PakietySzybowe.Add(p);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Przywrócono pakiety domyślne.", count = seed.Count });
        }
    }
}