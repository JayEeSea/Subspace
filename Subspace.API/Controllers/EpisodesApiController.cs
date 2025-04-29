using Microsoft.AspNetCore.Mvc;
using Subspace.Shared.Data;
using Subspace.Shared.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Subspace.API.Dtos;
using Subspace.API.Helpers;
using Subspace.API.Query;

[Route("api/[controller]")]
[ApiController]
public class EpisodesController : ControllerBase
{
    private readonly SubspaceDbContext _context;

    public EpisodesController(SubspaceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all episodes with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="seriesId">Limit results to a specific series ID.</param>
    /// <param name="season">Limit results to a specific season.</param>
    /// <param name="title">Filter by a substring match in the episode title (case-insensitive).</param>
    /// <param name="firstAiredAfter">Only include episodes aired after this date (yyyy-MM-dd).</param>
    /// <param name="firstAiredBefore">Only include episodes aired before this date (yyyy-MM-dd).</param>
    /// <param name="tagIds">
    /// Optional: A list of tag IDs to filter by. Only episodes that include <strong>any</strong> of these tags will be returned.
    /// Example: <c>tagIds=1&amp;tagIds=22</c>
    /// </param>
    /// <param name="tags">
    /// Optional: A comma-separated list of tag names to filter by (case-insensitive). Matches episodes with <strong>any</strong> of the provided tags.
    /// Example: <c>tags=Klingons,Borg</c>
    /// </param>
    /// <param name="sortBy">Field(s) to sort by. Supports comma-separated values like <c>season,episode</c>.</param>
    /// <param name="order">Sort direction: <c>asc</c> (default) or <c>desc</c>.</param>
    /// <param name="page">Results page number (1-based).</param>
    /// <param name="pageSize">Number of results per page. Max is 100.</param>
    /// <returns>Paginated list of matching episodes.</returns>
    [HttpGet]
    public async Task<ActionResult<object>> GetEpisodes(
    [FromQuery] int? seriesId,
    [FromQuery] int? season,
    [FromQuery] string? title,
    [FromQuery] string? sortBy,
    [FromQuery] string? order = "asc",
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 25,
    [FromQuery] DateTime? firstAiredAfter = null,
    [FromQuery] DateTime? firstAiredBefore = null,
    [FromQuery] List<int>? tagIds = null,
    [FromQuery] string? tags = null)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _context.Episodes
            .Include(e => e.Series)
            .Include(e => e.EpisodeTags)
                .ThenInclude(et => et.Tag)
            .AsQueryable();

        query = EpisodeQueryBuilder.ApplyFilters(query, seriesId, season, title, null, null, firstAiredAfter, firstAiredBefore, tagIds, tags);
        query = EpisodeQueryBuilder.ApplySorting(query, sortBy, order);

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
    /// Retrieves a single episode by its ID, with optional control over response detail.
    /// </summary>
    /// <param name="id">The unique numeric ID of the episode to retrieve.</param>
    /// <param name="view">
    /// Optional: Controls the level of detail returned. Use <c>full</c> (default) to get complete episode metadata, 
    /// or <c>compact</c> to return only <c>id</c>, <c>title</c>, and a <c>displayTitle</c> field.
    /// </param>
    /// <returns>
    /// A single episode object. Returns a full <see cref="EpisodeDto"/> for <c>view=full</c>, or a lightweight anonymous object for <c>view=compact</c>.
    /// Returns 404 Not Found if no episode is found with the given ID.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetEpisode(
    int id,
    [FromQuery] string? view = "full") // "compact" or "full"
    {
        var episode = await _context.Episodes
            .Include(e => e.Series)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (episode == null)
            return NotFound();

        return view?.ToLowerInvariant() switch
        {
            "compact" => new
            {
                episode.Id,
                episode.Title,
                DisplayTitle = $"S{episode.Season:D2}E{episode.EpisodeNumber:D2}: {episode.Title}"
            },
            _ => EpisodeMapper.ToDto(episode)
        };
    }

    /// <summary>
    /// Retrieves a single episode using series short code, season, and episode number.
    /// </summary>
    /// <param name="seriesCode">Short code for the series (e.g. TNG, VOY).</param>
    /// <param name="season">Season number.</param>
    /// <param name="episode">Episode number within the season.</param>
    /// <returns>The matching episode or 404 if not found.</returns>
    [HttpGet("alias/{seriesAbbr}/{season:int}/{episode:int}")]
    public async Task<ActionResult<object>> GetEpisodeByAlias(
    string seriesAbbr,
    int season,
    int episode)
    {
        var matchingEpisode = await _context.Episodes
            .Include(e => e.Series)
            .Include(e => e.EpisodeTags).ThenInclude(et => et.Tag)
            .Where(e =>
                e.Series.Abbreviation.ToLower() == seriesAbbr.ToLower() &&
                e.Season == season &&
                e.EpisodeNumber == episode)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.Season,
                e.EpisodeNumber,
                e.AirDate,
                Series = new
                {
                    e.Series.Id,
                    e.Series.Name,
                    e.Series.Abbreviation
                },
                Tags = e.EpisodeTags.Select(et => new { et.Tag.Id, et.Tag.Name })
            })
            .FirstOrDefaultAsync();

        if (matchingEpisode == null)
            return NotFound($"No episode found for {seriesAbbr} S{season:D2}E{episode:D2}");

        return Ok(matchingEpisode);
    }

    /// <summary>
    /// Retrieves the earliest-aired episode, optionally filtered by series, season, or tags.
    /// </summary>
    /// <param name="seriesId">Optional: Restrict to a specific series ID.</param>
    /// <param name="season">Optional: Restrict to a specific season number.</param>
    /// <param name="tagIds">
    /// Optional: A list of tag IDs to filter by. Only episodes that include <strong>any</strong> of these tags will be returned.
    /// Example: <c>tagIds=1&amp;tagIds=22</c>
    /// </param>
    /// <param name="tags">
    /// Optional: A comma-separated list of tag names to filter by (case-insensitive). Matches episodes with <strong>any</strong> of the provided tags.
    /// Example: <c>tags=Q,Klingons</c>
    /// </param>
    /// <returns>The first aired episode matching the filters.</returns>
    [HttpGet("first")]
    public async Task<ActionResult<EpisodeDto>> GetFirstEpisode(
    [FromQuery] int? seriesId,
    [FromQuery] int? season,
    [FromQuery] List<int>? tagIds = null,
    [FromQuery] string? tags = null)
    {
        var query = _context.Episodes
            .Include(e => e.Series)
            .Include(e => e.EpisodeTags)
                .ThenInclude(et => et.Tag)
            .AsQueryable();

        query = EpisodeQueryBuilder.ApplyFilters(query, seriesId, season, null, null, null, null, null, tagIds, tags);

        var episode = await query.OrderBy(e => e.AirDate).FirstOrDefaultAsync();

        return episode == null
            ? NotFound("No matching episodes found.")
            : EpisodeMapper.ToDto(episode);
    }

    /// <summary>
    /// Retrieves the most recently aired episode, optionally filtered by series, season, or tags.
    /// </summary>
    /// <param name="seriesId">Optional: Restrict to a specific series ID.</param>
    /// <param name="season">Optional: Restrict to a specific season number.</param>
    /// <param name="tagIds">
    /// Optional: One or more tag IDs. Episodes with any of the specified tag IDs will be matched.
    /// Example: <c>tagIds=1&amp;tagIds=22</c>
    /// </param>
    /// <param name="tags">
    /// Optional: A comma-separated list of tag names (case-insensitive). Episodes matching any of the provided tag names will be included.
    /// Example: <c>tags=Time Travel,Borg</c>
    /// </param>
    /// <returns>The latest aired episode matching the filter criteria.</returns>
    [HttpGet("latest")]
    public async Task<ActionResult<EpisodeDto>> GetLatestEpisode(
    [FromQuery] int? seriesId,
    [FromQuery] int? season,
    [FromQuery] List<int>? tagIds = null,
    [FromQuery] string? tags = null)
    {
        var query = _context.Episodes
            .Include(e => e.Series)
            .Include(e => e.EpisodeTags)
                .ThenInclude(et => et.Tag)
            .AsQueryable();

        query = EpisodeQueryBuilder.ApplyFilters(query, seriesId, season, null, null, null, null, null, tagIds, tags);

        var episode = await query.OrderByDescending(e => e.AirDate).FirstOrDefaultAsync();

        return episode == null
            ? NotFound("No matching episodes found.")
            : EpisodeMapper.ToDto(episode);
    }

    /// <summary>
    /// Retrieves a single random episode, optionally filtered by series, season, title, keywords, air date, or tags.
    /// </summary>
    /// <param name="seriesId">Limit to a specific series by its ID.</param>
    /// <param name="season">Limit to a specific season number.</param>
    /// <param name="title">Only include episodes whose title contains this substring (case-insensitive).</param>
    /// <param name="keyword">A comma-separated list of OR keywords to match in the synopsis.</param>
    /// <param name="keywordsAll">A comma-separated list of AND keywords to match in the synopsis.</param>
    /// <param name="firstAiredAfter">Only include episodes aired after this date (yyyy-MM-dd).</param>
    /// <param name="firstAiredBefore">Only include episodes aired before this date (yyyy-MM-dd).</param>
    /// <param name="tagIds">
    /// Optional: One or more tag IDs. Episodes with any of the specified tag IDs will be matched.
    /// Example: tagIds=1&amp;tagIds=22
    /// </param>
    /// <param name="tags">
    /// Optional: A comma-separated list of tag names (case-insensitive). Episodes matching any of the provided tag names will be included.
    /// Example: tags=Q,Horror
    /// </param>
    /// <returns>
    /// A single <see cref="EpisodeDto"/> that matches the specified filters, chosen randomly.
    /// Returns 404 if no episode matches.
    /// </returns>
    [HttpGet("random")]
    public async Task<ActionResult<EpisodeDto>> GetRandomEpisode(
    [FromQuery] int? seriesId,
    [FromQuery] int? season,
    [FromQuery] string? title,
    [FromQuery] string? keyword,
    [FromQuery] string? keywordsAll,
    [FromQuery] DateTime? firstAiredAfter,
    [FromQuery] DateTime? firstAiredBefore,
    [FromQuery] List<int>? tagIds = null,
    [FromQuery] string? tags = null)
    {
        var query = _context.Episodes
            .Include(e => e.Series)
            .Include(e => e.EpisodeTags)
                .ThenInclude(et => et.Tag)
            .AsQueryable();

        query = EpisodeQueryBuilder.ApplyFilters(query, seriesId, season, title, keyword, keywordsAll, firstAiredAfter, firstAiredBefore, tagIds, tags);

        var count = await query.CountAsync();
        if (count == 0)
            return NotFound("No episodes match the filters.");

        var index = Random.Shared.Next(count);
        var episode = await query.OrderBy(e => e.Id).Skip(index).FirstOrDefaultAsync();

        return episode == null ? NotFound() : EpisodeMapper.ToDto(episode);
    }

    /// <summary>
    /// Returns a batch of random episodes that match optional filters.
    /// </summary>
    /// <param name="count">Number of random episodes to return (default: 5, max: 100).</param>
    /// <param name="seriesId">Optional: Filter by series ID.</param>
    /// <param name="season">Optional: Filter by season number.</param>
    /// <param name="title">Optional: Filter by title substring (case-insensitive).</param>
    /// <param name="keyword">Optional: Match episodes where the synopsis contains any of these comma-separated words.</param>
    /// <param name="keywordsAll">Optional: Match episodes where the synopsis contains all of these comma-separated words.</param>
    /// <param name="firstAiredAfter">Optional: Only include episodes aired after this date.</param>
    /// <param name="firstAiredBefore">Optional: Only include episodes aired before this date.</param>
    /// <param name="tagIds">Optional: Filter by one or more tag IDs.</param>
    /// <param name="tags">Optional: Filter by one or more tag names.</param>
    /// <returns>A list of randomly selected episodes that match the filters.</returns>
    [HttpGet("random/multiple")]
    public async Task<ActionResult<IEnumerable<EpisodeDto>>> GetRandomEpisodes(
    [FromQuery] int count = 5,
    [FromQuery] int? seriesId = null,
    [FromQuery] int? season = null,
    [FromQuery] string? title = null,
    [FromQuery] string? keyword = null,
    [FromQuery] string? keywordsAll = null,
    [FromQuery] DateTime? firstAiredAfter = null,
    [FromQuery] DateTime? firstAiredBefore = null,
    [FromQuery] List<int>? tagIds = null,
    [FromQuery] string? tags = null)
    {
        count = Math.Clamp(count, 1, 100);

        // Step 1: Build the filtered query
        var query = _context.Episodes
            .Include(e => e.Series)
            .Include(e => e.EpisodeTags)
                .ThenInclude(et => et.Tag)
            .AsQueryable();

        query = EpisodeQueryBuilder.ApplyFilters(query, seriesId, season, title, keyword, keywordsAll, firstAiredAfter, firstAiredBefore, tagIds, tags);

        // Step 2: Get matching episode IDs
        var allMatchingIds = await query.Select(e => e.Id).ToListAsync();

        if (allMatchingIds.Count == 0)
            return NotFound("No episodes match the filters.");

        // Step 3: Shuffle and take N
        var selectedIds = allMatchingIds
            .OrderBy(_ => Guid.NewGuid()) // true in-memory shuffle
            .Take(count)
            .ToList();

        // Step 4: Load full episode data
        var selectedEpisodes = await _context.Episodes
            .Include(e => e.Series)
            .Include(e => e.EpisodeTags)
                .ThenInclude(et => et.Tag)
            .Where(e => selectedIds.Contains(e.Id))
            .ToListAsync();

        // Step 5: Optional - re-sort to match selectedIds order
        var sortedEpisodes = selectedIds
            .Select(id => selectedEpisodes.First(e => e.Id == id))
            .ToList();

        var dtoResults = sortedEpisodes.Select(EpisodeMapper.ToDto).ToList();

        return Ok(dtoResults);
    }

    /// <summary>
    /// Returns the most recently added episodes, ordered by their creation date.
    /// </summary>
    /// <param name="count">Number of episodes to return (default 10, max 50).</param>
    /// <param name="seriesId">Optional: Filter to a specific series.</param>
    /// <param name="firstAiredAfter">Optional: Only include episodes aired after this date.</param>
    /// <param name="firstAiredBefore">Optional: Only include episodes aired before this date.</param>
    /// <param name="tagIds">Optional: List of tag IDs to match.</param>
    /// <param name="tags">Optional: List of tag names to match.</param>
    /// <returns>List of recently added episodes.</returns>
    [HttpGet("recently-added")]
    public async Task<ActionResult<IEnumerable<EpisodeDto>>> GetRecentlyAddedEpisodes(
        [FromQuery] int count = 10,
        [FromQuery] int? seriesId = null,
        [FromQuery] DateTime? firstAiredAfter = null,
        [FromQuery] DateTime? firstAiredBefore = null,
        [FromQuery] List<int>? tagIds = null,
        [FromQuery] string? tags = null)
    {
        count = Math.Clamp(count, 1, 50);

        var query = _context.Episodes
            .Include(e => e.Series)
            .Include(e => e.EpisodeTags)
                .ThenInclude(et => et.Tag)
            .AsQueryable();

        query = EpisodeQueryBuilder.ApplyFilters(query, seriesId, null, null, null, null, firstAiredAfter, firstAiredBefore, tagIds, tags);

        var episodes = await query
            .OrderByDescending(e => e.DateCreated)
            .Take(count)
            .ToListAsync();

        var dtoResults = episodes.Select(EpisodeMapper.ToDto).ToList();
        return Ok(dtoResults);
    }

    /// <summary>
    /// Returns episodes that share tags with the specified episode, ranked by the number of shared tags.
    /// </summary>
    /// <param name="episodeId">The ID of the reference episode.</param>
    /// <param name="limit">The maximum number of related episodes to return. Default is 10.</param>
    /// <returns>A list of related episodes with shared tag counts.</returns>
    [HttpGet("related")]
    public async Task<ActionResult<object>> GetRelatedEpisodes(
        [FromQuery] int episodeId,
        [FromQuery] int limit = 10)
    {
        if (limit < 1 || limit > 100)
            return BadRequest("Limit must be between 1 and 100.");

        // Step 1: Get the tag IDs for the target episode
        var targetTagIds = await _context.EpisodeTags
            .Where(et => et.EpisodeId == episodeId)
            .Select(et => et.TagId)
            .ToListAsync();

        if (!targetTagIds.Any())
            return NotFound($"Episode with ID {episodeId} has no tags or does not exist.");

        // Step 2: Find episodes that share any of these tags (excluding the original)
        var relatedEpisodes = await _context.EpisodeTags
            .Where(et => targetTagIds.Contains(et.TagId) && et.EpisodeId != episodeId)
            .GroupBy(et => et.EpisodeId)
            .Select(g => new
            {
                EpisodeId = g.Key,
                SharedTagCount = g.Count()
            })
            .OrderByDescending(g => g.SharedTagCount)
            .ThenBy(g => g.EpisodeId)
            .Take(limit)
            .ToListAsync();

        // Step 3: Load episode data
        var episodeIds = relatedEpisodes.Select(r => r.EpisodeId).ToList();
        var episodes = await _context.Episodes
            .Include(e => e.Series)
            .Where(e => episodeIds.Contains(e.Id))
            .ToListAsync();

        // Step 4: Join results and return
        var results = relatedEpisodes
            .Join(episodes,
                  r => r.EpisodeId,
                  e => e.Id,
                  (r, e) => new
                  {
                      e.Id,
                      e.Title,
                      DisplayTitle = $"{e.Series.Name} S{e.Season:D2}E{e.EpisodeNumber:D2} - {e.Title}",
                      SharedTagCount = r.SharedTagCount
                  })
            .OrderByDescending(e => e.SharedTagCount)
            .ThenBy(e => e.Id)
            .ToList();

        return Ok(new
        {
            EpisodeId = episodeId,
            Related = results
        });
    }

    /// <summary>
    /// Returns episodes sorted by air date across all series, with optional filtering.
    /// </summary>
    /// <param name="start">Optional: Start date for air date filtering (inclusive).</param>
    /// <param name="end">Optional: End date for air date filtering (inclusive).</param>
    /// <param name="seriesId">Optional: Filter by series ID.</param>
    /// <param name="tagId">Optional: Filter episodes that have this tag.</param>
    /// <param name="limit">Optional: Maximum number of episodes to return (default: 100).</param>
    /// <returns>A list of episodes sorted by AirDate.</returns>
    [HttpGet("timeline")]
    public async Task<ActionResult<IEnumerable<object>>> GetEpisodeTimeline(
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? end = null,
        [FromQuery] int? seriesId = null,
        [FromQuery] int? tagId = null,
        [FromQuery] int limit = 100)
    {
        limit = Math.Clamp(limit, 1, 500);

        var query = _context.Episodes
            .Include(e => e.Series)
            .Include(e => e.EpisodeTags).ThenInclude(et => et.Tag)
            .AsQueryable();

        if (start.HasValue)
            query = query.Where(e => e.AirDate >= start.Value);

        if (end.HasValue)
            query = query.Where(e => e.AirDate <= end.Value);

        if (seriesId.HasValue)
            query = query.Where(e => e.SeriesId == seriesId.Value);

        if (tagId.HasValue)
            query = query.Where(e => e.EpisodeTags.Any(et => et.TagId == tagId.Value));

        var episodes = await query
            .OrderBy(e => e.AirDate)
            .ThenBy(e => e.Series.Name)
            .Take(limit)
            .Select(e => new
            {
                e.Id,
                e.Title,
                e.AirDate,
                e.Season,
                e.EpisodeNumber,
                Series = new { e.Series.Id, e.Series.Name, e.Series.Abbreviation },
                Tags = e.EpisodeTags.Select(et => new { et.Tag.Id, et.Tag.Name })
            })
            .ToListAsync();

        return Ok(episodes);
    }
}