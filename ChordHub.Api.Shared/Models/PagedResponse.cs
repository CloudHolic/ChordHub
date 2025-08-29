namespace ChordHub.Api.Shared.Models;

public class PagedResponse<T> : ApiResponse<T>
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }
}
