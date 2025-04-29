using Microsoft.EntityFrameworkCore;

namespace Subspace.Shared.Data.Seed.Tags
{
    public static partial class TagExtensions
    {
        public static void SeedTags(this ModelBuilder builder)
        {
            builder.TagData();
        }
    }
}