using GEORGE.Server;
using GEORGE.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Route("api/konfmodele")]

[ApiController]
public class KonfModeleController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public KonfModeleController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<KonfModele>>> Get()
    {
        return await _context.KonfModele.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<KonfModele>> Get(int id)
    {
        var model = await _context.KonfModele.FindAsync(id);
        return model ?? (ActionResult<KonfModele>)NotFound();
    }

    [HttpGet("FIND_ONLY_ELEMENT/{row_id_elementu}")]
    public async Task<ActionResult<List<KonfModele>>> GetAllByRowIdOnlyElement(string row_id_elementu)
    {
        return await _context.KonfModele.Where(x => x.RowId.ToString() == row_id_elementu).OrderBy(e => e.NazwaKonfiguracji).ToListAsync();
    }

    [HttpGet("FIND_ONLY_TRUE/{row_id_sys}")]
    public async Task<ActionResult<List<KonfModele>>> GetAllByRowIdOnlyTrue(string row_id_sys)
    {
        return await _context.KonfModele.Where(x => x.RowIdSystem.ToString() == row_id_sys && x.WidocznaNaLiscie).OrderBy(e => e.NazwaKonfiguracji).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<KonfModele>> Post(KonfModele model)
    {
        _context.KonfModele.Add(model);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = model.Id }, model); // 🔥 Zwraca JSON nowego modelu!
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, KonfModele model)
    {
        _context.Entry(model).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _context.KonfModele.FindAsync(id);
        _context.KonfModele.Remove(model);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
