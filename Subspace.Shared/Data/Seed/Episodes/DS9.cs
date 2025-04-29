using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.Episodes
{
    public static partial class EpisodeSeedExtensions
    {
        public static void SeedDS9(this ModelBuilder builder)
        {
            var episodes = new List<Episode>
            {
                new Episode { Id = 279, Title = "Emissary", Synopsis = "When the troubled Commander Sisko takes command of a surrendered space station, he learns that it borders a unique stable wormhole.", SeriesId = 4, Season = 1, EpisodeNumber = 1, ImdbUrl = "https://www.imdb.com/title/tt0108214", AirDate = new DateTime(1993, 1, 3) },
                new Episode { Id = 280, Title = "Past Prologue", Synopsis = "Tahna Los, a former Bajoran terrorist during the Occupation, asks Sisko for asylum on DS9. Meanwhile, the station's last Cardassian inhabitant, Garak, possibly a former spy for the Cardassian government, proves an interesting mystery to Dr. Bashir.", SeriesId = 4, Season = 1, EpisodeNumber = 2, ImdbUrl = "https://www.imdb.com/title/tt0708576", AirDate = new DateTime(1993, 1, 10) }
            };

            foreach (var ep in episodes)
            {
                ep.DateCreated = ep.AirDate;
            }

            builder.Entity<Episode>().HasData(episodes);
        }
    }
}