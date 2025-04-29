using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.EpisodeTags
{
    public static partial class EpisodeTagSeedExtensions
    {
        public static void SeedENTTags(this ModelBuilder builder)
        {
            builder.Entity<EpisodeTag>().HasData(
                new EpisodeTag { EpisodeId = 620, TagId = 20 },
                new EpisodeTag { EpisodeId = 621, TagId = 205 }
            );
        }
    }
}