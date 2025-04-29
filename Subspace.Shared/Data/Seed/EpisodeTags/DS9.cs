using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.EpisodeTags
{
    public static partial class EpisodeTagSeedExtensions
    {
        public static void SeedDS9Tags(this ModelBuilder builder)
        {
            builder.Entity<EpisodeTag>().HasData(
                new EpisodeTag { EpisodeId = 279, TagId = 39 },
                new EpisodeTag { EpisodeId = 279, TagId = 142 }
            );
        }
    }
}