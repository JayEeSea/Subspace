using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;

namespace Subspace.Web.Controllers;

[Route("series")]
public class SeriesController : Controller
{
    private readonly SubspaceDbContext _context;

    public SeriesController(SubspaceDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
            return BadRequest("Search term must be at least 3 characters.");

        var loweredName = name.ToLower();

        var series = await _context.Series
            .Where(s => s.Name.ToLower().Contains(loweredName) || s.Abbreviation.ToLower().Contains(loweredName))
            .OrderBy(s => s.Name)
            .Take(10) // Limit to 10 results
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Abbreviation
            })
            .ToListAsync();

        return Json(series);
    }
}