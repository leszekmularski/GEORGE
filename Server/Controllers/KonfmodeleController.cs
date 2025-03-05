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

    [HttpPost]
    public async Task<IActionResult> Post(KonfModele model)
    {
        _context.KonfModele.Add(model);
        await _context.SaveChangesAsync();
        return Ok();
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
