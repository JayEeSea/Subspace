using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedDSC(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 717, Title = "The Vulcan Hello", Synopsis = "While patrolling Federation space, the U.S.S. Shenzhou encounters an object of unknown origin, putting First Officer Michael Burnham to her greatest test yet.", SeriesId = 7, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt5509372", AirDate = new DateTime(2017, 9, 24) },
                new Episode { Id = 718, Title = "Battle at the Binary Stars", Synopsis = "Escaping from the brig while the ship is under attack, Burnham joins the captain in an audacious plan to end a battle rapidly escalating into war.", SeriesId = 7, Season = 1, EpisodeNumber = 2, ImdbUrl = "https://www.imdb.com/title/tt5835712", AirDate = new DateTime(2017, 9, 24) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}