namespace Subspace.Shared.Models
{
    public class Episode
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Synopsis { get; set; }
        public int SeriesId { get; set; }
        public Series Series { get; set; } = default!;
        public int Season { get; set; }
        public int EpisodeNumber { get; set; }
        public required string ImdbUrl {  get; set; }
        public DateTime AirDate { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public ICollection<EpisodeTag> EpisodeTags { get; set; } = new List<EpisodeTag>();
    }
}