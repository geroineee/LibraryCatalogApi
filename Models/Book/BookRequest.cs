namespace WebLibraryApi.Models.Book
{
    public class BookRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int PublishedYear { get; set; }
        public string? Genre { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}
