using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;
using System.Security.Claims;

namespace Subspace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly SubspaceDbContext _context;

    public HomeController(SubspaceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["EpisodeCount"] = await _context.Episodes.CountAsync();
        ViewData["SeriesCount"] = await _context.Series.CountAsync();
        ViewData["TagCount"] = await _context.Tags.CountAsync();

        // Find most tagged episode
        var topTagged = await _context.Episodes
            .Include(e => e.EpisodeTags)
            .Include(e => e.Series)
            .OrderByDescending(e => e.EpisodeTags.Count)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Season,
                e.EpisodeNumber,
                e.Series.Abbreviation,
                TagCount = e.EpisodeTags.Count
            })
            .FirstOrDefaultAsync();

        ViewData["TopTaggedEpisode"] = topTagged;

        // Grab total number of episodes
        var episodeCount = await _context.Episodes.CountAsync();
        ViewData["EpisodeCount"] = episodeCount;

        // Find top tags
        var topTags = await _context.Tags
            .Select(t => new
            {
                t.Id,
                t.Name,
                UsageCount = t.EpisodeTags.Count,
                Percentage = (double)t.EpisodeTags.Count / episodeCount * 100
            })
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Name)
            .Take(10)
            .ToListAsync();

        ViewData["TopTags"] = topTags;

        // Grab untagged episodes
        var untaggedEpisodes = await _context.Episodes
            .Include(e => e.Series)
            .Where(e => !e.EpisodeTags.Any())
            .OrderBy(e => e.Id)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Season,
                e.EpisodeNumber,
                SeriesName = e.Series.Abbreviation
            })
            .ToListAsync();

        ViewData["UntaggedEpisodes"] = untaggedEpisodes;
        ViewData["UntaggedCount"] = untaggedEpisodes.Count;

        // Grab latest episode
        var latestEpisode = await _context.Episodes
            .Include(e => e.Series)
            .OrderByDescending(e => e.DateCreated)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Season,
                e.EpisodeNumber,
                e.Series.Abbreviation,
                e.DateCreated
            })
            .FirstOrDefaultAsync();

        ViewData["LatestEpisode"] = latestEpisode;

        return View();
    }
}