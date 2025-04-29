using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;
using Subspace.Shared.Models;

namespace Subspace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class EpisodesController : Controller
{
    private readonly SubspaceDbContext _context;

    public EpisodesController(SubspaceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(
    int? seriesId,
    int? season,
    string? search,
    bool? untagged,
    int page = 1,
    int pageSize = 30)
    {
        var query = _context.Episodes
            .Include(e => e.Series)
            .Include(e => e.EpisodeTags)
                .ThenInclude(et => et.Tag)
            .AsQueryable();

        // 🔥 Prioritise 'untagged' over search
        if (untagged == true)
        {
            query = query.Where(e => !e.EpisodeTags.Any());
            ViewData["SearchTerm"] = "Untagged";  // Fake search for the UI only
        }
        else if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e =>
                EF.Functions.Like(e.Title, $"%{search}%") ||
                e.EpisodeTags.Any(et => EF.Functions.Like(et.Tag.Name, $"%{search}%"))
            );
            ViewData["SearchTerm"] = search;
        }

        // 🔥 Filter by series
        if (seriesId.HasValue)
        {
            query = query.Where(e => e.SeriesId == seriesId.Value);
            ViewData["SelectedSeriesId"] = seriesId.Value;
        }

        // 🔥 Filter by season
        if (season.HasValue)
        {
            query = query.Where(e => e.Season == season.Value);
            ViewData["SelectedSeason"] = season.Value;
        }

        // 🔥 Ordering
        if (seriesId.HasValue)
        {
            query = query.OrderBy(e => e.Season).ThenBy(e => e.EpisodeNumber);
        }
        else
        {
            query = query.OrderBy(e => e.AirDate);
        }

        // 🔥 Pagination
        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var episodes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // 🔥 ViewData for filters
        ViewData["SeriesList"] = await _context.Series
            .OrderBy(s => s.Name)
            .ToListAsync();

        if (seriesId.HasValue)
        {
            var availableSeasons = await _context.Episodes
                .Where(e => e.SeriesId == seriesId.Value)
                .Select(e => e.Season)
                .Distinct()
                .OrderBy(s => s)
                .ToListAsync();

            ViewData["AvailableSeasons"] = availableSeasons;
        }

        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;

        return View(episodes);
    }

    public async Task<IActionResult> Create()
    {
        ViewData["SeriesList"] = await _context.Series.ToListAsync();
        ViewData["TagsList"] = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Episode episode, int[] SelectedTagIds)
    {
        if (ModelState.IsValid)
        {
            foreach (var tagId in SelectedTagIds)
            {
                episode.EpisodeTags.Add(new EpisodeTag { TagId = tagId });
            }

            _context.Episodes.Add(episode);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Episode created successfully.";
            return RedirectToAction(nameof(Index));
        }
        ViewData["SeriesList"] = await _context.Series.ToListAsync();
        ViewData["TagsList"] = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
        return View(episode);
    }

    public async Task<IActionResult> Edit(int id, string? returnUrl)
    {
        var episode = await _context.Episodes
            .Include(e => e.EpisodeTags)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (episode == null)
            return NotFound();

        ViewData["SeriesList"] = await _context.Series.ToListAsync();
        ViewData["TagsList"] = await _context.Tags.OrderBy(t => t.Name).ToListAsync();
        ViewData["ReturnUrl"] = returnUrl;

        return View(episode);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string Title, int Season, int EpisodeNumber, int SeriesId, DateTime AirDate, string? Synopsis, string? ImdbUrl, int[]? SelectedTagIds,string? returnUrl)
    {
        SelectedTagIds ??= Array.Empty<int>();

        var existingEpisode = await _context.Episodes
            .Include(e => e.EpisodeTags)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (existingEpisode == null) return NotFound();

        // Manually update scalar fields
        existingEpisode.Title = Title;
        existingEpisode.Season = Season;
        existingEpisode.EpisodeNumber = EpisodeNumber;
        existingEpisode.SeriesId = SeriesId;
        existingEpisode.AirDate = AirDate;
        existingEpisode.Synopsis = Synopsis;
        existingEpisode.ImdbUrl = ImdbUrl;

        // Remove old tags
        _context.EpisodeTags.RemoveRange(existingEpisode.EpisodeTags);

        // Add new tags
        foreach (var tagId in SelectedTagIds)
        {
            _context.EpisodeTags.Add(new EpisodeTag
            {
                EpisodeId = id,
                TagId = tagId
            });
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Episode edited successfully.";

        if (!string.IsNullOrWhiteSpace(returnUrl))
            return Redirect("/Admin/Episodes" + returnUrl);

        return RedirectToAction(nameof(Index));
    }


    public async Task<IActionResult> Delete(int id)
    {
        var episode = await _context.Episodes
            .Include(e => e.Series)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (episode == null) return NotFound();
        return View(episode);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var episode = await _context.Episodes.FindAsync(id);
        if (episode != null)
        {
            _context.Episodes.Remove(episode);
            await _context.SaveChangesAsync();
        }
        TempData["SuccessMessage"] = "Episode deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}