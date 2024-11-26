using GEORGE.Server;
using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;

public class PracownicyService
{
    private readonly ApplicationDbContext _context;

    public PracownicyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetHasloSqlAsync(string rowIdPracownika)
    {
        var pracownik = await _context.Pracownicy
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.RowId == rowIdPracownika);

        return pracownik?.HasloSQL;
    }
}
