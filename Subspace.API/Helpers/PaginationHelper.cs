namespace Subspace.API.Helpers
{
    public static class PaginationHelper
    {
        public static object GetPaginationMeta(int totalCount, int page, int pageSize)
        {
            return new
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
    }
}