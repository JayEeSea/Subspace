using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedTNG(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 103, Title = "Encounter at Farpoint", Synopsis = "On the maiden mission of the U.S.S. Enterprise (NCC-1701-D), an omnipotent being known as Q challenges the crew to discover the secret of a mysterious base in an advanced and civilized fashion.", SeriesId = 3, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt0094030", AirDate = new DateTime(1987, 9, 28) },
                new Episode { Id = 104, Title = "The Naked Now", Synopsis = "The crew of the Enterprise is infected with a virus that causes them to behave as though they were intoxicated.", SeriesId = 3, Season = 1, EpisodeNumber = 2, ImdbUrl = "https://www.imdb.com/title/tt0708810", AirDate = new DateTime(1987, 10, 3) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}