using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedTOS(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 1, Title = "The Cage", Synopsis = "Capt. Pike is held prisoner and tested by aliens who have the power to project incredibly lifelike illusions.", SeriesId = 1, Season = 1, EpisodeNumber = 0, ImdbUrl = "https://www.imdb.com/title/tt0059753", AirDate = new DateTime(1988, 11, 27) },
                new Episode { Id = 2, Title = "The Man Trap", Synopsis = "Dr. McCoy discovers his old flame is not what she seems after crew members begin dying from a sudden lack of salt in their bodies.", SeriesId = 1, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt0708469", AirDate = new DateTime(1966, 9, 8) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}