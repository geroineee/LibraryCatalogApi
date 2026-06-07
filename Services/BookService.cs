using WebLibraryApi.Models;
using WebLibraryApi.Models.Book;
using WebLibraryApi.Repositories;

using WebLibraryApi.Exceptions;
using WebLibraryApi.Extensions;

namespace WebLibraryApi.Services
{
    public class BookService(
        IBookRepository bookRepository,
        ILogger<BookService> logger) : IBookService
    {
        private readonly IBookRepository _repository = bookRepository;
        private readonly ILogger<BookService> _logger = logger;

        public async Task<PagedResult<BookResponse>> GetPagedBooksAsync(BookGetQuery query)
        {
            var filter = new BookFilter() { Genre = query.Genre, IsAvailable = query.IsAvailable };
            int totalCount = await _repository.GetTotalCountAsync(filter);

            int currentPage = query.Page;
            int totalPages = totalCount > 0
                ? (int)Math.Ceiling(totalCount / (double)query.PageSize)
                : 1;

            if (currentPage > totalPages)
            {
                _logger.LogWarning("Requested page {RequestedPage} exceeds total pages {TotalPages}. Returning last page.",
                    query.Page, totalPages);
                currentPage = totalPages;
            }

            int offset = (currentPage - 1) * query.PageSize;

            var books = await _repository.GetAsync(offset, query.PageSize, filter);
            var result = new PagedResult<BookResponse>()
            {
                Items = books.Select(b => b.ToResponse()),
                Page = currentPage,
                TotalCount = totalCount,
                PageSize = query.PageSize
            };

            return result;
        }

        public async Task<BookResponse?> GetBookByIdAsync(int id)
        {
            var book = await _repository.GetByIdAsync(id);
            return book is not null
                ? book.ToResponse()
                : null;
        }

        public async Task DeleteBookAsync(int id)
        {
            bool exists = await _repository.ExistAsync(id);

            if (!exists)
            {
                _logger.LogWarning("Book {BookId} not found for deletion.", id);
                throw new NotFoundException($"Book {id} does not exist.");
            }

            await _repository.DeleteAsync(id);
            _logger.LogInformation("Book {BookId} deleted", id);
        }

        public async Task<BookResponse> ChangeAvailabilityAsync(int id, bool availability)
        {
            var book = await _repository.GetByIdAsync(id);

            if (book is null)
            {
                _logger.LogWarning("Book {BookId} not found for changing availability.", id);
                throw new NotFoundException($"Book {id} not found");
            }

            await _repository.ChangeAvailabilityAsync(id, availability);
            book.IsAvailable = availability;

            _logger.LogInformation("Book {BookId} availability changed from to {New}",
                id, availability);

            return book.ToResponse();
        }

        public async Task<int> CreateBookAsync(BookRequest request)
        {
            var sameTitleBook = await _repository.GetByTitleAsync(request.Title);

            if (sameTitleBook is not null)
            {
                _logger.LogWarning("Cannot create book with title '{BookTitle}': already exists.", request.Title);
                throw new ConflictException($"Book with title '{request.Title}' already exists");
            }
            int id = await _repository.CreateAsync(request.ToEntity());
            _logger.LogInformation("Book {BookId} created", id);

            return id;
        }

        public async Task<BookResponse> UpdateBookAsync(int id, BookRequest request)
        {
            var book = request.ToEntity(id);
            var existing = await _repository.GetByIdAsync(book.Id);

            if (existing is null)
            {
                _logger.LogWarning("Book {BookId} not found for update.", book.Id);
                throw new NotFoundException($"Book {book.Id} not found");
            }

            if (!string.Equals(book.Title, existing.Title))
            {
                var sameTitleBook = await _repository.GetByTitleAsync(book.Title);
                if (sameTitleBook is not null && sameTitleBook.Id != book.Id)
                {
                    _logger.LogWarning("Cannot update book {BookId} with title '{BookTitle}': already exists.",
                        book.Id, book.Title);
                    throw new ConflictException($"Book with title '{book.Title}' already exists");
                }
            }

            await _repository.UpdateAsync(book);
            _logger.LogInformation("Book {BookId} updated", book.Id);

            return book.ToResponse();
        }
    }
}
