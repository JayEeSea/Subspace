using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;
using System.Xml.Linq;

namespace Subspace.Web.Controllers;

[Route("tags")]
public class TagsController : Controller
{
    private readonly SubspaceDbContext _context;

    public TagsController(SubspaceDbContext context)
    {
        _context = context;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
            return BadRequest("Search term must be at least 3 characters.");

        var loweredName = name.ToLower();

        var tags = await _context.Tags
            .Where(t => t.Name.Contains(name))
            .OrderBy(t => t.Name)
            .Take(10) // Limit to 10 results
            .Select(t => new
            {
                t.Id,
                t.Name
            })
            .ToListAsync();

        return Json(tags);
    }

    [HttpGet("random")]
    public async Task<IActionResult> GetRandomTags()
    {
        var tags = await _context.Tags
            .FromSqlRaw("SELECT * FROM Tags ORDER BY RAND() LIMIT 2")
            .Select(t => t.Name)
            .ToListAsync();

        return Json(tags);
    }
}