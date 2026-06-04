using System.Runtime.CompilerServices;
using WebLibraryApi.Models.Book;

namespace WebLibraryApi.Extensions
{
    public static class BookMapping
    {
        public static Book ToEntity(this BookRequest request, int id = 0)
        {
            return new Book
            {
                Id = id,
                Title = request.Title,
                Author = request.Author,
                PublishedYear = request.PublishedYear,
                Genre = request.Genre,
                IsAvailable = request.IsAvailable
            };
        }

        public static BookResponse ToResponse(this Book book)
        {
            return new BookResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                PublishedYear = book.PublishedYear,
                Genre = book.Genre,
                IsAvailable = book.IsAvailable
            };
        }
    }
}
