using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedTAS(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 81, Title = "Beyond the Farthest Star", Synopsis = "The Enterprise finds an ancient abandoned starship, and a malevolent entity on it eager to take over the Starfleet ship.", SeriesId = 2, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt0827626", AirDate = new DateTime(1973, 9, 8) },
                new Episode { Id = 82, Title = "Yesteryear", Synopsis = "After finding himself erased from recent history, Spock must travel back in time to save himself as a youth.", SeriesId = 2, Season = 1, EpisodeNumber = 2, ImdbUrl = "https://www.imdb.com/title/tt0827628", AirDate = new DateTime(1973, 9, 15) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}