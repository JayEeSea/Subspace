using Subspace.API.Dtos;
using Subspace.Shared.Models;

namespace Subspace.API.Helpers
{
    public static class EpisodeMapper
    {
        public static EpisodeDto ToDto(Episode episode) => new EpisodeDto
        {
            Id = episode.Id,
            Title = episode.Title,
            Synopsis = episode.Synopsis,
            SeriesId = episode.SeriesId,
            SeriesName = episode.Series?.Name ?? "Unknown",
            Season = episode.Season,
            EpisodeNumber = episode.EpisodeNumber,
            ImdbUrl = episode.ImdbUrl,
            FormattedDate = episode.AirDate.ToString("yyyy-MM-dd"),
            Tags = episode.EpisodeTags.Select(et => et.Tag.Name).Distinct().ToList()
        };
    }
}