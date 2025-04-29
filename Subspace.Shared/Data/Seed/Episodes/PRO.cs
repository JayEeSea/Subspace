using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedPRO(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 872, Title = "Lost and Found: Part 1", Synopsis = "A group of lawless teens, exiled on a mining colony outside Federation space, discover a derelict Starfleet ship. Dal must gather an unlikely crew for their newfound ship if they are going to escape Tars Lamora, but the Diviner and his daughter Gwyn have other plans.", SeriesId = 11, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt10619492", AirDate = new DateTime(2021, 10, 28) },
                new Episode { Id = 873, Title = "Lost and Found: Part 2", Synopsis = "A group of lawless teens, exiled on a mining colony outside Federation space, discover a derelict Starfleet ship. Dal must gather an unlikely crew for their newfound ship if they are going to escape Tars Lamora, but the Diviner and his daughter Gwyn have other plans.", SeriesId = 11, Season = 1, EpisodeNumber = 2, ImdbUrl = "https://www.imdb.com/title/tt14080498", AirDate = new DateTime(2021, 10, 28) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}