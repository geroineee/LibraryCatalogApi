using WebLibraryApi.Models;
using WebLibraryApi.Models.Book;

namespace WebLibraryApi.Services
{
    public interface IBookService
    {
        Task<PagedResult<BookResponse>> GetPagedBooksAsync(BookGetQuery query);
        Task<BookResponse?> GetBookByIdAsync(int id);
        Task<BookResponse> UpdateBookAsync(int id, BookRequest book);
        Task<BookResponse> ChangeAvailabilityAsync(int id, bool request);
        Task<int> CreateBookAsync(BookRequest book);
        Task DeleteBookAsync(int id);
    }
}
