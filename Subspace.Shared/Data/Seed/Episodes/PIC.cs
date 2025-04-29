using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedPIC(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 792, Title = "Remembrance", Synopsis = "Fourteen years after retiring from Starfleet, Jean-Luc Picard, still haunted by the death of Data, is living a quiet life on his family vineyard when a woman comes to him for help.", SeriesId = 9, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt9381924", AirDate = new DateTime(2020, 1, 24) },
                new Episode { Id = 793, Title = "Maps and Legends", Synopsis = "Without the support of Starfleet, Picard turns to Dr. Agnes Jurati and his estranged colleague Raffi Musiker for help in finding the truth about Dahj, unaware that hidden enemies are also interested in what he'll find.", SeriesId = 9, Season = 1, EpisodeNumber = 2, ImdbUrl = "https://www.imdb.com/title/tt9420276", AirDate = new DateTime(2020, 1, 30) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}