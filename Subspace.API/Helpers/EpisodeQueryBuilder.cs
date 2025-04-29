using Microsoft.EntityFrameworkCore;
using Subspace.Shared.Models;
using Subspace.API.Helpers;
using System.Linq.Expressions;

namespace Subspace.API.Query
{
    public static class EpisodeQueryBuilder
    {
        public static IQueryable<Episode> ApplyFilters(
            IQueryable<Episode> query,
            int? seriesId,
            int? season,
            string? title,
            string? keyword,
            string? keywordsAll,
            DateTime? firstAiredAfter,
            DateTime? firstAiredBefore,
            IEnumerable<int>? tagIds = null,
            string? tags = null)
        {
            if (seriesId.HasValue)
                query = query.Where(e => e.SeriesId == seriesId.Value);

            if (season.HasValue)
                query = query.Where(e => e.Season == season.Value);

            if (!string.IsNullOrWhiteSpace(title))
                query = query.Where(e => e.Title.Contains(title));

            if (firstAiredAfter.HasValue)
                query = query.Where(e => e.AirDate > firstAiredAfter.Value);

            if (firstAiredBefore.HasValue)
                query = query.Where(e => e.AirDate < firstAiredBefore.Value);

            if (tagIds?.Any() == true)
                query = query.Where(e => e.EpisodeTags.Any(et => tagIds.Contains(et.TagId)));

            if (!string.IsNullOrWhiteSpace(tags))
            {
                var tagNames = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                   .Select(t => t.ToLowerInvariant());
                query = query.Where(e => e.EpisodeTags.Any(et => tagNames.Contains(et.Tag.Name.ToLower())));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var keywords = keyword.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                Expression<Func<Episode, bool>> orFilter = e => false;
                foreach (var k in keywords)
                {
                    var temp = k;
                    orFilter = ExpressionExtensions.Or(orFilter, e => e.Synopsis.Contains(temp));
                }
                query = query.Where(orFilter);
            }

            if (!string.IsNullOrWhiteSpace(keywordsAll))
            {
                var keywords = keywordsAll.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                Expression<Func<Episode, bool>> andFilter = e => true;
                foreach (var k in keywords)
                {
                    var temp = k;
                    andFilter = ExpressionExtensions.And(andFilter, e => e.Synopsis.Contains(temp));
                }
                query = query.Where(andFilter);
            }

            return query;
        }

        public static IQueryable<Episode> ApplySorting(
            IQueryable<Episode> query,
            string sortBy,
            string order)
        {
            sortBy = string.IsNullOrWhiteSpace(sortBy) ? "id" : sortBy.ToLowerInvariant();
            order = string.IsNullOrWhiteSpace(order) ? "asc" : order.ToLowerInvariant();

            IOrderedQueryable<Episode>? sortedQuery = null;

            foreach (var field in sortBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                sortedQuery = field switch
                {
                    "title" => query.ApplySort(sortedQuery, e => e.Title, order),
                    "airdate" => query.ApplySort(sortedQuery, e => e.AirDate, order),
                    "season" => query.ApplySort(sortedQuery, e => e.Season, order),
                    "episode" => query.ApplySort(sortedQuery, e => e.EpisodeNumber, order),
                    "id" or _ => query.ApplySort(sortedQuery, e => e.Id, order)
                };

                query = sortedQuery!;
            }

            return query;
        }
    }
}