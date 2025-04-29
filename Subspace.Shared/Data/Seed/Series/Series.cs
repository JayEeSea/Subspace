using Microsoft.EntityFrameworkCore;
using Model = Subspace.Shared.Models.Series;

namespace Subspace.Shared.Data.Seed.Series
{
    public static partial class SeriesSeedExtensions
    {
        public static void SeriesData(this ModelBuilder builder)
        {
            builder.Entity<Model>().HasData(
                    new Model { Id = 1, Name = "Star Trek: The Original Series", Abbreviation = "TOS" },
                    new Model { Id = 2, Name = "Star Trek: The Animated Series", Abbreviation = "TAS" }
            );
        }
    }
}