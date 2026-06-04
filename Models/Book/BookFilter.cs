namespace WebLibraryApi.Models.Book
{
    public class BookFilter
    {
        public string? Genre { get; init; }
        public bool? IsAvailable { get; init; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Genre) && !IsAvailable.HasValue;
    }
}
