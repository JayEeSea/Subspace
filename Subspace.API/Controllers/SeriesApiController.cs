
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;
using Subspace.API.Dtos;
using Subspace.API.Helpers;
using Subspace.Shared.Models;
using Subspace.API.Query;

[Route("api/[controller]")]
[ApiController]
public class SeriesController : ControllerBase
{
    private readonly SubspaceDbContext _context;

    public SeriesController(SubspaceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves a list of all Star Trek series available in the API.
    /// </summary>
    /// <returns>
    /// Returns a list of <see cref="SeriesDto"/> objects, each containing the series ID, name, abbreviation, and total episode count.
    /// </returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SeriesDto>>> GetAllSeries()
    {
        var series = await _context.Series
            .Select(s => new SeriesDto
            {
                Id = s.Id,
                Name = s.Name,
                Abbreviation = s.Abbreviation,
                EpisodeCount = s.Episodes.Count()
            })
            .ToListAsync();

        return Ok(series);
    }

    /// <summary>
    /// Retrieves a specific Star Trek series by its ID, with optional control over response detail.
    /// </summary>
    /// <param name="id">The unique numeric ID of the series to retrieve.</param>
    /// <param name="view">
    /// Optional: Controls the level of detail returned. Use <c>full</c> (default) to return complete series metadata (e.g., name, abbreviation, episode count), 
    /// or <c>compact</c> to return only <c>id</c> and <c>name</c>.
    /// </param>
    /// <returns>
    /// A single series object. Returns a <see cref="SeriesDto"/> for <c>view=full</c>, or a lightweight anonymous object for <c>view=compact</c>.
    /// Returns 404 Not Found if no series exists with the given ID.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetSeries(int id, [FromQuery] string? view = "full")
    {
        var series = await _context.Series
            .Include(s => s.Episodes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (series == null)
            return NotFound();

        return view?.ToLowerInvariant() switch
        {
            "compact" => new
            {
                series.Id,
                series.Name
            },
            _ => new SeriesDto
            {
                Id = series.Id,
                Name = series.Name,
                Abbreviation = series.Abbreviation,
                EpisodeCount = series.Episodes.Count
            }
        };
    }

    /// <summary>
    /// Retrieves a paginated list of episodes for a specific Star Trek series.
    /// </summary>
    /// <param name="id">The unique ID of the series to retrieve episodes for.</param>
    /// <param name="page">Optional: Page number (1-based). Default is 1.</param>
    /// <param name="pageSize">Optional: Number of episodes per page. Default is 25. Maximum is 100.</param>
    /// <returns>
    /// Returns a paginated list of <see cref="EpisodeDto"/> objects belonging to the specified series.
    /// Includes pagination metadata in the response body and the <c>X-Pagination</c> header.
    /// Returns 404 Not Found if the series does not exist.
    /// </returns>
    [HttpGet("{id}/episodes")]
    public async Task<ActionResult<object>> GetEpisodesForSeries(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Episodes
            .Include(e => e.Series)
            .Where(e => e.SeriesId == id)
            .OrderBy(e => e.Season).ThenBy(e => e.EpisodeNumber);

        var totalCount = await query.CountAsync();
        var episodes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtoResults = episodes.Select(EpisodeMapper.ToDto).ToList();
        var paginationMeta = PaginationHelper.GetPaginationMeta(totalCount, page, pageSize);
        Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(paginationMeta));

        return Ok(new { Meta = paginationMeta, Data = dtoResults });
    }

    /// <summary>
    /// Returns all episodes in the specified series, grouped by season and sorted in order.
    /// </summary>
    /// <param name="id">The series ID.</param>
    /// <returns>A list of seasons, each with a list of episodes.</returns>
    [HttpGet("{id}/episodes/grouped-by-season")]
    public async Task<ActionResult<IEnumerable<object>>> GetEpisodesGroupedBySeason(int id)
    {
        // Check if series exists
        var seriesExists = await _context.Series.AnyAsync(s => s.Id == id);
        if (!seriesExists)
            return NotFound($"Series with ID {id} not found.");

        // Get and group episodes
        var groupedEpisodes = await _context.Episodes
            .Where(e => e.SeriesId == id)
            .OrderBy(e => e.Season)
            .ThenBy(e => e.EpisodeNumber)
            .GroupBy(e => e.Season)
            .Select(g => new
            {
                Season = g.Key,
                Episodes = g.Select(e => new
                {
                    e.Id,
                    e.Title,
                    e.Season,
                    e.EpisodeNumber,
                    DisplayTitle = $"S{e.Season:D2}E{e.EpisodeNumber:D2} - {e.Title}",
                    e.AirDate
                }).ToList()
            })
            .ToListAsync();

        return Ok(groupedEpisodes);
    }

    /// <summary>
    /// Retrieves the first-aired episode for a specific series.
    /// </summary>
    /// <param name="id">The ID of the series to retrieve the first episode from.</param>
    /// <returns>The earliest aired episode from the specified series.</returns>
    [HttpGet("{id}/first")]
    public async Task<ActionResult<EpisodeDto>> GetFirstEpisodeForSeries(int id)
    {
        var episode = await _context.Episodes
            .Include(e => e.Series)
            .Where(e => e.SeriesId == id)
            .OrderBy(e => e.AirDate)
            .FirstOrDefaultAsync();

        return episode == null
            ? NotFound($"No episodes found for series ID {id}.")
            : EpisodeMapper.ToDto(episode);
    }

    /// <summary>
    /// Retrieves the most recently aired episode for a specific series.
    /// </summary>
    /// <param name="id">The ID of the series to retrieve the latest episode from.</param>
    /// <returns>The latest aired episode from the specified series.</returns>
    [HttpGet("{id}/latest")]
    public async Task<ActionResult<EpisodeDto>> GetLatestEpisodeForSeries(int id)
    {
        var episode = await _context.Episodes
            .Include(e => e.Series)
            .Where(e => e.SeriesId == id)
            .OrderByDescending(e => e.AirDate)
            .FirstOrDefaultAsync();

        return episode == null
            ? NotFound($"No episodes found for series ID {id}.")
            : EpisodeMapper.ToDto(episode);
    }

    /// <summary>
    /// Retrieves a single random episode from the specified series, optionally filtered by season and air date.
    /// </summary>
    /// <param name="id">The unique ID of the series to select a random episode from.</param>
    /// <param name="season">Optional: Restrict results to a specific season number.</param>
    /// <param name="firstAiredAfter">Optional: Limit to episodes aired after this date (format: yyyy-MM-dd).</param>
    /// <param name="firstAiredBefore">Optional: Limit to episodes aired before this date (format: yyyy-MM-dd).</param>
    /// <returns>
    /// Returns a single <see cref="EpisodeDto"/> object chosen at random from the filtered result set.
    /// Returns 404 Not Found if no matching episodes are available for the specified filters.
    /// </returns>
    [HttpGet("{id}/random")]
    public async Task<ActionResult<EpisodeDto>> GetRandomEpisodeForSeries(
        int id,
        [FromQuery] int? season,
        [FromQuery] DateTime? firstAiredAfter,
        [FromQuery] DateTime? firstAiredBefore)
    {
        var query = _context.Episodes
            .Include(e => e.Series)
            .Where(e => e.SeriesId == id);

        if (season.HasValue)
            query = query.Where(e => e.Season == season.Value);
        if (firstAiredAfter.HasValue)
            query = query.Where(e => e.AirDate > firstAiredAfter.Value);
        if (firstAiredBefore.HasValue)
            query = query.Where(e => e.AirDate < firstAiredBefore.Value);

        var count = await query.CountAsync();
        if (count == 0)
        {
            var msg = $"No episodes found for series ID {id}";
            if (season.HasValue) msg += $" and season {season}";
            if (firstAiredAfter.HasValue) msg += $" after {firstAiredAfter:yyyy-MM-dd}";
            if (firstAiredBefore.HasValue) msg += $" before {firstAiredBefore:yyyy-MM-dd}";
            return NotFound(msg + ".");
        }

        var index = Random.Shared.Next(count);
        var episode = await query.OrderBy(e => e.Id).Skip(index).FirstOrDefaultAsync();
        return episode == null ? NotFound() : EpisodeMapper.ToDto(episode);
    }

    /// <summary>
    /// Retrieves a list of seasons for the specified series, including the number of episodes in each season.
    /// </summary>
    /// <param name="id">The unique ID of the series to retrieve seasons for.</param>
    /// <returns>
    /// Returns a list of objects where each item contains a <c>season</c> number and the <c>count</c> of episodes in that season.
    /// Returns 404 Not Found if the series does not exist.
    /// </returns>
    [HttpGet("{id}/seasons")]
    public async Task<ActionResult<IEnumerable<object>>> GetSeasonsForSeries(int id)
    {
        var hasSeries = await _context.Series.AnyAsync(s => s.Id == id);
        if (!hasSeries)
            return NotFound($"Series with ID {id} not found.");

        var seasonGroups = await _context.Episodes
            .Where(e => e.SeriesId == id)
            .GroupBy(e => e.Season)
            .Select(g => new { Season = g.Key, Count = g.Count() })
            .OrderBy(g => g.Season)
            .ToListAsync();

        return Ok(seasonGroups);
    }

    /// <summary>
    /// Returns the most frequently used tags in the specified Star Trek series.
    /// </summary>
    /// <param name="id">The ID of the series to analyse.</param>
    /// <param name="limit">Maximum number of tags to return (optional). If omitted, returns all.</param>
    /// <returns>A list of tags used in the series, sorted by usage count.</returns>
    [HttpGet("{id}/tags")]
    public async Task<ActionResult<object>> GetTagsForSeries(int id, [FromQuery] int? limit = null)
    {
        // Check if the series exists
        var seriesExists = await _context.Series.AnyAsync(s => s.Id == id);
        if (!seriesExists)
            return NotFound($"Series with ID {id} not found.");

        // Get all tag usage grouped and sorted
        var tags = await _context.EpisodeTags
            .Where(et => et.Episode.SeriesId == id)
            .GroupBy(et => new { et.TagId, et.Tag.Name })
            .Select(g => new
            {
                Id = g.Key.TagId,
                Name = g.Key.Name,
                UsageCount = g.Count()
            })
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Name)
            .ToListAsync();

        // Apply limit in-memory if specified
        if (limit.HasValue && limit.Value > 0)
            tags = tags.Take(limit.Value).ToList();

        return Ok(new
        {
            SeriesId = id,
            Tags = tags
        });
    }
}