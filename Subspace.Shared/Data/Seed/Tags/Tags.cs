using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;
using Model = Subspace.Shared.Models.Tag;

namespace Subspace.Shared.Data.Seed.Tags
{
    public static partial class TagExtensions
    {
        public static void TagData(this ModelBuilder builder)
        {
            builder.Entity<Model>().HasData(
                new Model { Id = 1, Name = "Time Travel" },
                new Model { Id = 2, Name = "Alternate Universe" }
            );
        }
    }
}