﻿using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;

namespace Subspace.Shared.Data.Seed.EpisodeTags
{
    public static partial class EpisodeTagSeedExtensions
    {
        public static void SeedSFATags(this ModelBuilder builder)
        {
            builder.Entity<EpisodeTag>().HasData(

            );
        }
    }
}