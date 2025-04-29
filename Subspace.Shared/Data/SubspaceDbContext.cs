using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;
using System.Reflection.Emit;
using Subspace.Shared.Data.Seed.Series;
using Subspace.Shared.Data.Seed.Episodes;
using Subspace.Shared.Data.Seed.Tags;
using Subspace.Shared.Data.Seed.EpisodeTags;

namespace Subspace.Shared.Data
{
    public class SubspaceDbContext : DbContext
    {
        public SubspaceDbContext(DbContextOptions<SubspaceDbContext> options)
            : base(options)
        {
        }

        public DbSet<Episode> Episodes { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<EpisodeTag> EpisodeTags { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seeding Series
            builder.SeedSeries();

            // Episode Relationships
            builder.Entity<Episode>()
                .HasOne(e => e.Series)
                .WithMany(s => s.Episodes)
                .HasForeignKey(e => e.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            // Setting default timestamp at the database level
            builder.Entity<Episode>()
                .Property(e => e.DateCreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Common query field indexing
            builder.Entity<Episode>()
                .HasIndex(e => new { e.SeriesId, e.Season, e.EpisodeNumber });

            // Seeding Episodes
            builder.SeedEpisodes();

            // Tag relationships
            builder.Entity<EpisodeTag>()
                .HasKey(et => new { et.EpisodeId, et.TagId });

            builder.Entity<EpisodeTag>()
                .HasOne(et => et.Episode)
                .WithMany(e => e.EpisodeTags)
                .HasForeignKey(et => et.EpisodeId);

            builder.Entity<EpisodeTag>()
                .HasOne(et => et.Tag)
                .WithMany(t => t.EpisodeTags)
                .HasForeignKey(et => et.TagId);

            // Seeding Tags
            builder.SeedTags();

            // Seeding Episode Tags
            builder.SeedEpisodeTags();
        }
    }
}