using Microsoft.EntityFrameworkCore;

namespace Subspace.Shared.Data.Seed.Series
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedSeries(this ModelBuilder builder)
        {
            builder.SeriesData();
        }
    }
}