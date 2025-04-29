using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.EpisodeTags
{
    public static partial class EpisodeTagSeedExtensions
    {
        public static void SeedSTTags(this ModelBuilder builder)
        {
            builder.Entity<EpisodeTag>().HasData(
                new EpisodeTag { EpisodeId = 782, TagId = 228 },
                new EpisodeTag { EpisodeId = 783, TagId = 112 }
            );
        }
    }
}