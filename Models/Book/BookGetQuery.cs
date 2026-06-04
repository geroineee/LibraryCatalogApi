namespace WebLibraryApi.Models.Book
{
    public record BookGetQuery(
        string? Genre,
        bool? IsAvailable,
        int Page = 1,
        int PageSize = 10);
}
