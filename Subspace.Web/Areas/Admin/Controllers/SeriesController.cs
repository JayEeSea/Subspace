using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;
using Subspace.Shared.Models;
using Subspace.Web.Data;

namespace Subspace.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class SeriesController : Controller
{
    private readonly SubspaceDbContext _context;

    public SeriesController(SubspaceDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var series = await _context.Series.OrderBy(s => s.Id).ToListAsync();
        return View(series);
    }

    public IActionResult Create() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Series series)
    {
        if (ModelState.IsValid)
        {
            _context.Add(series);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Series created successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(series);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var series = await _context.Series.FindAsync(id);
        if (series == null) return NotFound();
        return View(series);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Series series)
    {
        if (ModelState.IsValid)
        {
            _context.Update(series);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Series edited successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(series);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var series = await _context.Series.FindAsync(id);
        if (series == null) return NotFound();
        return View(series);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var series = await _context.Series.FindAsync(id);
        if (series != null)
        {
            _context.Series.Remove(series);
            await _context.SaveChangesAsync();
        }
        TempData["SuccessMessage"] = "Series deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}