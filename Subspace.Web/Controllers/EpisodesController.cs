using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;
using Subspace.Shared.Models;

namespace Subspace.Web.Controllers;

[Route("episodes")]
public class EpisodesController : Controller
{
    private readonly SubspaceDbContext _context;

    public EpisodesController(SubspaceDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
            return BadRequest("Search term must be at least 3 characters.");

        var loweredName = name.ToLower();

        var episodes = await _context.Episodes
            .Include(e => e.Series)
            .Where(e => e.Title.ToLower().Contains(loweredName))
            .OrderBy(e => e.Title)
            .Take(10)
            .Select(e => new
            {
                e.Id,
                e.Title,
                Series = e.Series.Name
            })
            .ToListAsync();

        return Json(episodes);
    }

    [HttpGet("range")]
    public async Task<IActionResult> GetEpisodeRange()
    {
        var oldest = await _context.Episodes
            .OrderBy(e => e.DateCreated)
            .Select(e => e.Title)
            .FirstOrDefaultAsync();

        var newest = await _context.Episodes
            .OrderByDescending(e => e.DateCreated)
            .Select(e => e.Title)
            .FirstOrDefaultAsync();

        return Json(new { oldest, newest });
    }
}