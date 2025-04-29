using Microsoft.EntityFrameworkCore;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedEpisodes(this ModelBuilder builder)
        {
            builder.SeedTOS();
            builder.SeedTAS();
            builder.SeedTNG();
            builder.SeedDS9();
            builder.SeedVOY();
            builder.SeedENT();
            builder.SeedDSC();
            builder.SeedST();
            builder.SeedPIC();
            builder.SeedLD();
            builder.SeedPRO();
            builder.SeedSNW();
            builder.SeedSFA();
        }
    }
}