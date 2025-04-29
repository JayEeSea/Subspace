namespace Subspace.API.Dtos
{
    /// <summary>
    /// Represents a single Star Trek series with metadata.
    /// </summary>
    public class SeriesDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }           // e.g. Star Trek: Voyager
        public required string Abbreviation { get; set; }   // VOY
        public int EpisodeCount { get; set; }
    }
}