namespace Subspace.API.Dtos
{
    /// <summary>
    /// Represents a single Star Trek episode with metadata.
    /// </summary>
    public class EpisodeDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Synopsis { get; set; } = default!;
        public int SeriesId { get; set; }
        public string SeriesName { get; set; } = default!;
        public int Season { get; set; }
        public int EpisodeNumber { get; set; }
        public string ImdbUrl { get; set; } = default!;
        public string FormattedDate { get; set; } = default!;
        public string DisplayTitle => $"{SeriesName} S{Season:D2}E{EpisodeNumber:D2} - {Title}";

        public List<string> Tags { get; set; } = new();
    }

}
