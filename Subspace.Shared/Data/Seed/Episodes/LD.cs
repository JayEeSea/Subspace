using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedLD(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 822, Title = "Second Contact", Synopsis = "Ensigns Mariner and Boimler run into difficulty on Galar. Meanwhile, an alien virus infects the crew of the Cerritos.", SeriesId = 10, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt9207516", AirDate = new DateTime(2020, 8, 6) },
                new Episode { Id = 823, Title = "Envoys", Synopsis = "After a high-profile mission goes awry, Boimler is further plagued with self-doubt when Mariner proves herself to be a more naturally talented sci-fi badass than he. Rutherford quits his job in engineering and explores other departments on the U.S.S. Cerritos.", SeriesId = 10, Season = 1, EpisodeNumber = 2, ImdbUrl = "https://www.imdb.com/title/tt9207518", AirDate = new DateTime(2020, 8, 13) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}