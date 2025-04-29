using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedSNW(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 912, Title = "Strange New Worlds", Synopsis = "Captain Christopher Pike comes out of self-imposed exile to rescue an officer gone missing during a secret mission.", SeriesId = 12, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt12330364", AirDate = new DateTime(2022, 5, 5) },
                new Episode { Id = 913, Title = "Children of the Comet", Synopsis = "An ancient alien relic thwarts the Enterprise crew from re-routing a comet on track to strike an inhabited planet.", SeriesId = 12, Season = 1, EpisodeNumber = 2, ImdbUrl = "https://www.imdb.com/title/tt14426230", AirDate = new DateTime(2022, 5, 12) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}