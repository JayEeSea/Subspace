using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;
using Subspace.Shared.Models;

namespace Subspace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TagsController : Controller
{
    private readonly SubspaceDbContext _context;

    public TagsController(SubspaceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, int page = 1, int pageSize = 30)
    {
        var query = _context.Tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => EF.Functions.Like(t.Name, $"%{search}%"));
            ViewData["SearchTerm"] = search;
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var tags = await query
            .OrderBy(t => t.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = totalPages;

        return View(tags);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Tag tag)
    {
        if (ModelState.IsValid)
        {
            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tag created successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(tag);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return NotFound();
        return View(tag);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Tag tag)
    {
        if (ModelState.IsValid)
        {
            _context.Update(tag);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Tag updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(tag);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag == null) return NotFound();
        return View(tag);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var tag = await _context.Tags.FindAsync(id);
        if (tag != null)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
        TempData["SuccessMessage"] = "Tag deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}