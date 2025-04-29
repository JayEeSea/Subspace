using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.EpisodeTags
{
    public static partial class EpisodeTagSeedExtensions
    {
        public static void SeedVOYTags(this ModelBuilder builder)
        {
            builder.Entity<EpisodeTag>().HasData(
                new EpisodeTag { EpisodeId = 452, TagId = 31 },
                new EpisodeTag { EpisodeId = 452, TagId = 177 }
            );
        }
    }
}