using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedENT(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 620, Title = "Broken Bow", Synopsis = "Enterprise, Earth's first Warp 5 vessel, embarks on a dangerous first mission: bringing back a chased Klingon to his home world of Qo'noS.", SeriesId = 6, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt0286610", AirDate = new DateTime(2001, 9, 26) },
                new Episode { Id = 621, Title = "Fight or Flight", Synopsis = "Captain Archer wants to convert curiosity into deeds and decides to enter a ship floating in space. Hoshi has trouble adjusting to life on Enterprise.", SeriesId = 6, Season = 1, EpisodeNumber = 3, ImdbUrl = "https://www.imdb.com/title/tt0572209", AirDate = new DateTime(2001, 10, 3) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}