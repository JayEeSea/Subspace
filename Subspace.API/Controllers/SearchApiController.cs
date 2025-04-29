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
public class SearchController : ControllerBase
{
    private readonly SubspaceDbContext _context;

    public SearchController(SubspaceDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Searches for episodes with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="seriesId">Filter by series ID.</param>
    /// <param name="season">Filter by season number.</param>
    /// <param name="title">Partial match on episode title (case-insensitive).</param>
    /// <param name="keyword">Filter for episodes containing <strong>any</strong> of the keywords in title or synopsis.</param>
    /// <param name="keywordsAll">Filter for episodes containing <strong>all</strong> keywords in title or synopsis.</param>
    /// <param name="firstAiredAfter">Only include episodes aired after this date.</param>
    /// <param name="firstAiredBefore">Only include episodes aired before this date.</param>
    /// <param name="tagIds">Filter by any tag ID (e.g., <c>tagIds=3&amp;tagIds=7</c>).</param>
    /// <param name="tags">Comma-separated tag names (e.g., <c>tags=Klingon,Borg</c>).</param>
    /// <param name="sortBy">Field(s) to sort by (e.g., <c>season,episode</c>).</param>
    /// <param name="order">Sort order: <c>asc</c> (default) or <c>desc</c>.</param>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of results per page (default 25, max 100).</param>
    /// <returns>Paginated list of matching episodes with metadata.</returns>
    /// <response code="200">Returns a filtered, sorted, paginated list of episodes.</response>
    /// <response code="400">If the query parameters are invalid.</response>
    [HttpGet("episodes")]
    public async Task<ActionResult<object>> SearchEpisodes(
    [FromQuery] int? seriesId,
    [FromQuery] int? season,
    [FromQuery] string? title,
    [FromQuery] string? keyword,
    [FromQuery] string? keywordsAll,
    [FromQuery] DateTime? firstAiredAfter = null,
    [FromQuery] DateTime? firstAiredBefore = null,
    [FromQuery] string? sortBy = "id",
    [FromQuery] string? order = "asc",
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 25,
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

        query = EpisodeQueryBuilder.ApplyFilters(query, seriesId, season, title, keyword, keywordsAll, firstAiredAfter, firstAiredBefore, tagIds, tags);
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
    /// Returns unified search suggestions for episodes, tags, and series based on the provided query.
    /// </summary>
    /// <param name="query">
    /// Required search term. The input is case-insensitive and matched against:
    /// <list type="bullet">
    ///   <item><term>Episodes</term><description>Matches episode titles and synopsis.</description></item>
    ///   <item><term>Tags</term><description>Matches tag names.</description></item>
    ///   <item><term>Series</term><description>Matches series names and abbreviations.</description></item>
    /// </list>
    /// </param>
    /// <param name="limit">Optional. The maximum number of results to return per category. Defaults to 5. Max: 10.</param>
    /// <returns>
    /// A combined result set containing top-matching episodes, tags, and series.
    /// </returns>
    /// <response code="200">Returns matching results for episodes, tags, and series</response>
    /// <response code="400">If the query is missing or invalid</response>
    [HttpGet("suggest")]
    public async Task<ActionResult<object>> GetSearchSuggestions(
    [FromQuery(Name = "q")] string? query,
    [FromQuery] int limit = 5)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query parameter 'q' is required.");

        query = query.Trim();
        limit = Math.Clamp(limit, 1, 10);

        string likePattern = $"%{query}%";

        // Tags
        var matchingTags = await _context.Tags
            .Where(t => EF.Functions.Like(t.Name, likePattern))
            .OrderBy(t => t.Name)
            .Take(limit)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync();

        // Series
        var matchingSeries = await _context.Series
            .Where(s => EF.Functions.Like(s.Name, likePattern) || EF.Functions.Like(s.Abbreviation, likePattern))
            .OrderBy(s => s.Name)
            .Take(limit)
            .Select(s => new { s.Id, s.Name, s.Abbreviation })
            .ToListAsync();

        // Episodes
        var matchingEpisodes = await _context.Episodes
            .Where(e =>
                EF.Functions.Like(e.Title, likePattern) ||
                EF.Functions.Like(e.Synopsis ?? "", likePattern))
            .Include(e => e.Series)
            .OrderBy(e => e.Title)
            .Take(limit)
            .Select(e => new
            {
                e.Id,
                e.Title,
                Series = new { e.Series.Id, e.Series.Name, e.Series.Abbreviation },
                DisplayTitle = $"S{e.Season:D2}E{e.EpisodeNumber:D2} - {e.Title}"
            })
            .ToListAsync();

        return Ok(new
        {
            Tags = matchingTags,
            Series = matchingSeries,
            Episodes = matchingEpisodes
        });
    }
}