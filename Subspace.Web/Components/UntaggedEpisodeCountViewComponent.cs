using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data;

namespace Subspace.Web.Components
{
    public class UntaggedEpisodeCountViewComponent : ViewComponent
    {
        private readonly SubspaceDbContext _context;

        public UntaggedEpisodeCountViewComponent(SubspaceDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Adjust this logic depending on how tags are stored
            var count = await _context.Episodes
                .Where(e => !e.EpisodeTags.Any())
                .CountAsync();

            return View("Default", count);
        }
    }
}