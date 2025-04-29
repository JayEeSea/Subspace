using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;
using System.Reflection.Emit;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedST(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 782, Title = "Runaway", Synopsis = "On board the U.S.S. Discovery, Ensign Tilly encounters an unexpected visitor in need of help. However, this unlikely pair may have more in common than meets the eye.", SeriesId = 8, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt9059596", AirDate = new DateTime(2019, 1, 17) },
                new Episode { Id = 783, Title = "Calypso", Synopsis = "After waking up in an unfamiliar sickbay, Craft finds himself on board a deserted ship, and his only companion and hope for survival is an A.I. computer interface.", SeriesId = 8, Season = 1, EpisodeNumber = 2, ImdbUrl = "https://www.imdb.com/title/tt9059606", AirDate = new DateTime(2018, 11, 8) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}