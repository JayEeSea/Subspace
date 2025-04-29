using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedVOY(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 452, Title = "Caretaker", Synopsis = "While pursuing the trail of Maquis rebels, a newly commissioned Starfleet ship gets pulled to the far side of the galaxy.", SeriesId = 5, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt0114530", AirDate = new DateTime(1995, 1, 16) },
                new Episode { Id = 453, Title = "Parallax", Synopsis = "Tensions rise between the merged starfleet and maquis crews when they find another ship caught within a quantum singularity, only find there's more to the ship than it seems.", SeriesId = 5, Season = 1, EpisodeNumber = 2, ImdbUrl = "https://www.imdb.com/title/tt0708943", AirDate = new DateTime(1995, 1, 23) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}