namespace Subspace.Shared.Models
{
    public class Tag
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        public ICollection<EpisodeTag> EpisodeTags { get; set; } = new List<EpisodeTag>();
    }
}