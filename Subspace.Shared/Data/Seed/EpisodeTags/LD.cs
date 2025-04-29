using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.EpisodeTags
{
    public static partial class EpisodeTagSeedExtensions
    {
        public static void SeedLDTags(this ModelBuilder builder)
        {
            builder.Entity<EpisodeTag>().HasData(
                new EpisodeTag { EpisodeId = 822, TagId = 41 },
                new EpisodeTag { EpisodeId = 822, TagId = 70 }
            );
        }
    }
}