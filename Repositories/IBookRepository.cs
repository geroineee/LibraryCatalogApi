using WebLibraryApi.Models.Book;

namespace WebLibraryApi.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAsync(int offset, int limit, BookFilter filter);
        Task<int> GetTotalCountAsync(BookFilter filter);
        Task<Book?> GetByIdAsync(int id);
        Task<Book?> GetByTitleAsync(string title);

        Task<bool> ExistAsync(int id);

        Task<int> CreateAsync(Book book);
        Task<bool> DeleteAsync(int id);

        Task<bool> UpdateAsync(Book book);
        Task<bool> ChangeAvailabilityAsync(int id, bool isAvailable);
    }
}
