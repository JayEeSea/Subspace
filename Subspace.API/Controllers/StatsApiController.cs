using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;

namespace Subspace.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatsController : ControllerBase
{
    private readonly SubspaceDbContext _context;

    public StatsController(SubspaceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns a summary of dataset statistics across series, episodes, and tags.
    /// </summary>
    /// <returns>An object with total counts and key highlights.</returns>
    [HttpGet("overview")]
    public async Task<ActionResult<object>> GetOverviewStats()
    {
        var totalSeries = await _context.Series.CountAsync();
        var totalEpisodes = await _context.Episodes.CountAsync();
        var totalTags = await _context.Tags.CountAsync();

        var mostUsedTag = await _context.EpisodeTags
            .GroupBy(et => new { et.TagId, et.Tag.Name })
            .Select(g => new
            {
                Id = g.Key.TagId,
                Name = g.Key.Name,
                UsageCount = g.Count()
            })
            .OrderByDescending(t => t.UsageCount)
            .FirstOrDefaultAsync();

        var mostTaggedEpisode = await _context.EpisodeTags
            .GroupBy(et => new { et.EpisodeId, et.Episode.Title, et.Episode.Series.Name, et.Episode.Season, et.Episode.EpisodeNumber })
            .Select(g => new
            {
                Id = g.Key.EpisodeId,
                Title = g.Key.Title,
                DisplayTitle = $"{g.Key.Name} S{g.Key.Season:D2}E{g.Key.EpisodeNumber:D2} - {g.Key.Title}",
                TagCount = g.Count()
            })
            .OrderByDescending(e => e.TagCount)
            .FirstOrDefaultAsync();

        var firstEpisode = await _context.Episodes
            .OrderBy(e => e.AirDate)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.AirDate
            })
            .FirstOrDefaultAsync();

        var latestEpisode = await _context.Episodes
            .OrderByDescending(e => e.AirDate)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.AirDate
            })
            .FirstOrDefaultAsync();

        return Ok(new
        {
            TotalSeries = totalSeries,
            TotalEpisodes = totalEpisodes,
            TotalTags = totalTags,
            MostUsedTag = mostUsedTag,
            MostTaggedEpisode = mostTaggedEpisode,
            FirstEpisode = firstEpisode,
            LatestEpisode = latestEpisode
        });
    }
}