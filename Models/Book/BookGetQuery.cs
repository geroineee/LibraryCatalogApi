
namespace WebLibraryApi.Models.Book
{
    public class BookGetQuery
    {
        public string? Genre { get; set; }
        public bool? IsAvailable { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
