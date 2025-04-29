using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;
using Subspace.Shared.Models;

namespace Subspace.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagsController : ControllerBase
{
    private readonly SubspaceDbContext _context;

    public TagsController(SubspaceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Returns a list of all tags used to categorise episodes.
    /// </summary>
    /// <returns>A list of <see cref="Tag"/> objects.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetTags()
    {
        var tags = await _context.Tags
            .OrderBy(t => t.Name)
            .Select(t => new
            {
                t.Id,
                t.Name
            })
            .ToListAsync();

        return Ok(tags);
    }

    /// <summary>
    /// Retrieves a specific tag and its associated episodes, with pagination.
    /// </summary>
    /// <param name="id">The unique ID of the tag.</param>
    /// <param name="page">Page number (1-based). Default is 1.</param>
    /// <param name="pageSize">Number of episodes per page. Default is 25.</param>
    /// <returns>
    /// A tag object containing its ID, name, and a paginated list of associated episodes (with ID and title).
    /// Returns 404 if the tag is not found.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetTagWithEpisodes(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        if (page < 1 || pageSize < 1)
            return BadRequest("Page and pageSize must be greater than 0.");

        var tag = await _context.Tags
            .Include(t => t.EpisodeTags)
                .ThenInclude(et => et.Episode)
                    .ThenInclude(e => e.Series)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tag == null)
            return NotFound($"Tag with ID {id} not found.");

        var allEpisodes = tag.EpisodeTags
            .Select(et => new
            {
                et.Episode.Id,
                et.Episode.Title,
                DisplayTitle = $"{et.Episode.Series.Name} S{et.Episode.Season:D2}E{et.Episode.EpisodeNumber:D2} - {et.Episode.Title}"
            })
            .OrderBy(e => e.Id)
            .ToList();

        var totalCount = allEpisodes.Count;
        var pagedEpisodes = allEpisodes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(new
        {
            tag.Id,
            tag.Name,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Episodes = pagedEpisodes
        });
    }

    /// <summary>
    /// Returns tags that frequently co-occur with the specified tag across all episodes.
    /// </summary>
    /// <param name="id">The ID of the tag to find related tags for.</param>
    /// <param name="limit">The maximum number of related tags to return. Default is 10.</param>
    /// <returns>A list of related tags ordered by co-occurrence frequency.</returns>
    [HttpGet("{id}/related")]
    public async Task<ActionResult<object>> GetRelatedTags(int id, [FromQuery] int limit = 10)
    {
        if (limit < 1 || limit > 100)
            return BadRequest("Limit must be between 1 and 100.");

        // Step 1: Find all episode IDs that have the given tag
        var episodeIds = await _context.EpisodeTags
            .Where(et => et.TagId == id)
            .Select(et => et.EpisodeId)
            .Distinct()
            .ToListAsync();

        if (!episodeIds.Any())
            return NotFound($"No episodes found with tag ID {id}.");

        // Step 2: Find other tags used in those episodes, excluding the original tag
        var relatedTags = await _context.EpisodeTags
            .Where(et => episodeIds.Contains(et.EpisodeId) && et.TagId != id)
            .GroupBy(et => new { et.TagId, et.Tag.Name })
            .Select(g => new
            {
                Id = g.Key.TagId,
                Name = g.Key.Name,
                CoOccurrenceCount = g.Count()
            })
            .OrderByDescending(g => g.CoOccurrenceCount)
            .ThenBy(g => g.Name)
            .Take(limit)
            .ToListAsync();

        return Ok(new
        {
            TagId = id,
            RelatedTags = relatedTags
        });
    }

    /// <summary>
    /// Provides tag suggestions based on a prefix match. Optimised for UI autocomplete.
    /// </summary>
    /// <param name="q">The prefix to match against tag names. Required.</param>
    /// <param name="limit">Maximum number of suggestions to return. Default is 10.</param>
    /// <returns>A list of matching tag names and IDs.</returns>
    [HttpGet("autocomplete")]
    public async Task<ActionResult<IEnumerable<object>>> AutocompleteTags(
        [FromQuery(Name = "q")] string? prefix,
        [FromQuery] int limit = 10)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return BadRequest("Query parameter 'q' is required.");

        if (limit < 1 || limit > 50)
            return BadRequest("Limit must be between 1 and 50.");

        prefix = prefix.Trim().ToLowerInvariant();

        var suggestions = await _context.Tags
            .Where(t => t.Name.ToLower().StartsWith(prefix))
            .OrderBy(t => t.Name)
            .Take(limit)
            .Select(t => new
            {
                t.Id,
                t.Name
            })
            .ToListAsync();

        return Ok(suggestions);
    }

    /// <summary>
    /// Returns the most common co-occurring tag pairs across all episodes.
    /// </summary>
    /// <param name="minCount">Minimum number of co-occurrences to include (default: 2).</param>
    /// <param name="limit">Maximum number of pairs to return (default: 50).</param>
    /// <returns>Tag pairs with their co-occurrence count.</returns>
    [HttpGet("common-pairs")]
    public async Task<ActionResult<IEnumerable<object>>> GetCommonTagPairs(
    [FromQuery] int minCount = 2,
    [FromQuery] int limit = 50)
    {
        // Step 1: Get all tags grouped by episode — in memory
        var episodeTagGroups = await _context.EpisodeTags
            .Include(et => et.Tag)
            .GroupBy(et => et.EpisodeId)
            .ToListAsync();

        // Step 2: Build co-tag pairs from each episode
        var pairCounts = new Dictionary<(int, int), int>();

        foreach (var group in episodeTagGroups)
        {
            var tagIds = group.Select(et => et.TagId).Distinct().ToList();

            for (int i = 0; i < tagIds.Count; i++)
            {
                for (int j = i + 1; j < tagIds.Count; j++)
                {
                    var pair = (Math.Min(tagIds[i], tagIds[j]), Math.Max(tagIds[i], tagIds[j]));

                    if (pairCounts.ContainsKey(pair))
                        pairCounts[pair]++;
                    else
                        pairCounts[pair] = 1;
                }
            }
        }

        // Step 3: Get top co-occurring pairs
        var topPairs = pairCounts
            .Where(p => p.Value >= minCount)
            .OrderByDescending(p => p.Value)
            .Take(limit)
            .ToList();

        // Step 4: Load tag names
        var allTagIds = topPairs.SelectMany(p => new[] { p.Key.Item1, p.Key.Item2 }).Distinct().ToList();
        var tagMap = await _context.Tags
            .Where(t => allTagIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Name);

        // Step 5: Build final response
        var result = topPairs.Select(p => new
        {
            tag1 = new { id = p.Key.Item1, name = tagMap[p.Key.Item1] },
            tag2 = new { id = p.Key.Item2, name = tagMap[p.Key.Item2] },
            coOccurrenceCount = p.Value
        });

        return Ok(result);
    }

    /// <summary>
    /// Returns the most frequently used tags across all episodes, with optional filters and pagination.
    /// </summary>
    /// <param name="page">Page number (1-based). Defaults to 1.</param>
    /// <param name="pageSize">Number of items per page. Defaults to 20.</param>
    /// <param name="seriesId">Optional: Restrict to episodes from a specific series.</param>
    /// <param name="firstAiredAfter">Optional: Only include episodes aired after this date (yyyy-MM-dd).</param>
    /// <param name="firstAiredBefore">Optional: Only include episodes aired before this date (yyyy-MM-dd).</param>
    /// <returns>A paginated list of tags with their usage count.</returns>
    [HttpGet("popular")]
    public async Task<ActionResult<object>> GetPopularTags(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? seriesId = null,
        [FromQuery] DateTime? firstAiredAfter = null,
        [FromQuery] DateTime? firstAiredBefore = null)
    {
        if (page < 1 || pageSize < 1)
            return BadRequest("Page and pageSize must be greater than 0.");

        var query = _context.EpisodeTags
            .Include(et => et.Episode)
            .Include(et => et.Tag)
            .AsQueryable();

        if (seriesId.HasValue)
            query = query.Where(et => et.Episode.SeriesId == seriesId.Value);

        if (firstAiredAfter.HasValue)
            query = query.Where(et => et.Episode.AirDate > firstAiredAfter.Value);

        if (firstAiredBefore.HasValue)
            query = query.Where(et => et.Episode.AirDate < firstAiredBefore.Value);

        var groupedQuery = query
            .GroupBy(et => new { et.TagId, et.Tag.Name })
            .Select(g => new
            {
                Id = g.Key.TagId,
                Name = g.Key.Name,
                UsageCount = g.Count()
            });

        var totalCount = await groupedQuery.CountAsync();

        var results = await groupedQuery
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Results = results
        });
    }

    /// <summary>
    /// Returns the most used tags among episodes added in the last X days (default 30).
    /// </summary>
    /// <param name="days">How many days back to consider. Default is 30.</param>
    /// <param name="seriesId">Optional series ID filter.</param>
    /// <param name="limit">Maximum number of tags to return. Default is 10.</param>
    /// <returns>A list of trending tags with their usage count.</returns>
    [HttpGet("popular/recent")]
    public async Task<ActionResult<IEnumerable<object>>> GetPopularRecentTags(
        [FromQuery] int days = 365,
        [FromQuery] int? seriesId = null,
        [FromQuery] int limit = 10)
    {
        var sinceDate = DateTime.UtcNow.AddDays(-days);

        var recentQuery = _context.EpisodeTags
            .Include(et => et.Tag)
            .Include(et => et.Episode)
            .Where(et => et.Episode.DateCreated >= sinceDate);

        if (seriesId.HasValue)
            recentQuery = recentQuery.Where(et => et.Episode.SeriesId == seriesId.Value);

        var popularTags = await recentQuery
            .GroupBy(et => new { et.TagId, et.Tag.Name })
            .Select(g => new
            {
                Id = g.Key.TagId,
                Name = g.Key.Name,
                UsageCount = g.Count()
            })
            .OrderByDescending(t => t.UsageCount)
            .ThenBy(t => t.Name)
            .Take(limit)
            .ToListAsync();

        return Ok(popularTags);
    }

    /// <summary>
    /// Returns tags that have increased in usage recently compared to a prior baseline period.
    /// </summary>
    /// <param name="recentDays">Length of the recent window (e.g., last 365 days).</param>
    /// <param name="baselineDays">Length of the baseline window (e.g., previous 365 days).</param>
    /// <param name="seriesId">Optional filter by series.</param>
    /// <param name="limit">Max number of trending tags to return.</param>
    /// <returns>List of tags with recent vs baseline usage and growth info.</returns>
    [HttpGet("popular/trending")]
    public async Task<ActionResult<IEnumerable<object>>> GetTrendingTags(
        [FromQuery] int recentDays = 365,
        [FromQuery] int baselineDays = 365,
        [FromQuery] int? seriesId = null,
        [FromQuery] int limit = 10)
    {
        var now = DateTime.UtcNow;
        var recentStart = now.AddDays(-recentDays);
        var baselineStart = recentStart.AddDays(-baselineDays);

        // Base query
        var tagUsageQuery = _context.EpisodeTags
            .Include(et => et.Episode)
            .Include(et => et.Tag)
            .AsQueryable();

        if (seriesId.HasValue)
            tagUsageQuery = tagUsageQuery.Where(et => et.Episode.SeriesId == seriesId.Value);

        // Group counts
        var recentCounts = await tagUsageQuery
            .Where(et => et.Episode.DateCreated >= recentStart)
            .GroupBy(et => new { et.TagId, et.Tag.Name })
            .Select(g => new
            {
                TagId = g.Key.TagId,
                Name = g.Key.Name,
                RecentCount = g.Count()
            })
            .ToListAsync();

        var baselineCounts = await tagUsageQuery
            .Where(et => et.Episode.DateCreated >= baselineStart && et.Episode.DateCreated < recentStart)
            .GroupBy(et => et.TagId)
            .Select(g => new
            {
                TagId = g.Key,
                BaselineCount = g.Count()
            })
            .ToDictionaryAsync(x => x.TagId, x => x.BaselineCount);

        // Join recent and baseline and calculate trend
        var trending = recentCounts
            .Select(rc =>
            {
                baselineCounts.TryGetValue(rc.TagId, out int baseline);
                double growth = baseline == 0 ? rc.RecentCount : (rc.RecentCount - baseline) / (double)baseline;
                return new
                {
                    id = rc.TagId,
                    name = rc.Name,
                    recentCount = rc.RecentCount,
                    baselineCount = baseline,
                    growth = Math.Round(growth * 100, 2) // % change
                };
            })
            .Where(t => t.recentCount > 0 && t.growth > 0)
            .OrderByDescending(t => t.growth)
            .ThenByDescending(t => t.recentCount)
            .Take(limit)
            .ToList();

        return Ok(trending);
    }

    /// <summary>
    /// Searches for tags by name using case-insensitive matching. Supports exact, startsWith, and contains modes, with pagination.
    /// </summary>
    /// <param name="q">The search term to match in the tag name. Case-insensitive.</param>
    /// <param name="page">Page number (1-based). Default is 1.</param>
    /// <param name="pageSize">Number of results per page. Default is 25.</param>
    /// <param name="mode">
    /// Optional: Match mode. 
    /// <list type="bullet">
    ///   <item><term><c>contains</c></term><description>Default. Returns tags where the name contains the query.</description></item>
    ///   <item><term><c>startsWith</c></term><description>Returns tags where the name starts with the query.</description></item>
    ///   <item><term><c>exact</c></term><description>Returns tags that exactly match the query.</description></item>
    /// </list>
    /// </param>
    /// <returns>A paginated list of matching tag IDs and names.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<object>> SearchTags(
        [FromQuery(Name = "q")] string? query,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string mode = "contains") // "contains", "startsWith", or "exact"
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query parameter 'q' is required.");

        if (page < 1 || pageSize < 1)
            return BadRequest("Page and pageSize must be greater than 0.");

        query = query.Trim().ToLowerInvariant();

        var tagQuery = _context.Tags.AsQueryable();

        tagQuery = mode.ToLowerInvariant() switch
        {
            "exact" => tagQuery.Where(t => t.Name.ToLower() == query),
            "startswith" => tagQuery.Where(t => t.Name.ToLower().StartsWith(query)),
            _ => tagQuery.Where(t => t.Name.ToLower().Contains(query))
        };

        var totalCount = await tagQuery.CountAsync();

        var results = await tagQuery
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync();

        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            Results = results
        });
    }
}