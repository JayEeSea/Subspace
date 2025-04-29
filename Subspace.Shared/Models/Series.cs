namespace Subspace.Shared.Models
{
    public class Series
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Abbreviation { get; set; }

        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
    }
}
