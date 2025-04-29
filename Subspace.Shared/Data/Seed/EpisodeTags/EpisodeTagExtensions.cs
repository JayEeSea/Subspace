using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Data.Seed.Episodes;
using Subspace.Shared.Data.Seed.Tags;

namespace Subspace.Shared.Data.Seed.EpisodeTags
{
    public static partial class EpisodeTagExtensions
    {
        public static void SeedEpisodeTags(this ModelBuilder builder)
        {
            builder.SeedTOSTags();
            builder.SeedTASTags();
            builder.SeedTNGTags();
            builder.SeedDS9Tags();
            builder.SeedVOYTags();
            builder.SeedENTTags();
            builder.SeedDSCTags();
            builder.SeedSTTags();
            builder.SeedPICTags();
            builder.SeedLDTags();
            builder.SeedPROTags();
            builder.SeedSNWTags();
            builder.SeedSFATags();
        }
    }
}