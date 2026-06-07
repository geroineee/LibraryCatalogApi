using Microsoft.Data.Sqlite;
using WebLibraryApi.Models.Book;
using WebLibraryApi.Repositories;
using WebLibraryApi.Tests.Data;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace WebLibraryApi.Tests.UnitTests;
public class BookRepositoryTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;
    private BookRepository _repository = null!;
    private const string ConnectionString = "Data Source=SharedTestDb;Mode=Memory;Cache=Shared";

    public async Task InitializeAsync()
    {
        // Использование snake_case названий в бд
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        _connection = new SqliteConnection(ConnectionString);
        await _connection.OpenAsync();

        using var cmd = _connection.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE book (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                author TEXT NOT NULL,
                published_year INTEGER,
                genre TEXT,
                is_available INTEGER DEFAULT 1
            );";
        await cmd.ExecuteNonQueryAsync();

        _repository = new BookRepository(new SharedInMemoryDbFactory(ConnectionString));
    }

    public Task DisposeAsync()
    {
        _connection?.Close();
        _connection?.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateAsync_Return_New_Id()
    {
        // Assert
        var book = new Book()
        {
            Title = "Test Book",
            Author = "Test Author",
            PublishedYear = 2024,
            Genre = "Test Genre",
            IsAvailable = true
        };

        // Act
        var id = await _repository.CreateAsync(book);

        // Arrange
        Assert.True(id > 0);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBookExists()
    {

        // Assert
        var book = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            PublishedYear = 2024,
            Genre = "Test Genre",
            IsAvailable = true
        };
        var createdId = await _repository.CreateAsync(book);

        // Act
        var result = await _repository.GetByIdAsync(createdId);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(createdId, result.Id);
        Assert.Equal(book.Title, result.Title);
        Assert.Equal(book.Author, result.Author);
        Assert.Equal(book.Genre, result.Genre);
        Assert.Equal(book.PublishedYear, result.PublishedYear);
        Assert.Equal(book.IsAvailable, result.IsAvailable);
    }

    [Fact]
    public async Task DeleteAsync_WhenBookExists_ShouldReturnTrue()
    {
        // Arrange
        var book = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            PublishedYear = 2024,
            Genre = "Test Genre",
            IsAvailable = true
        };
        var createdId = await _repository.CreateAsync(book);

        // Act
        var result = await _repository.DeleteAsync(createdId);

        // Assert
        Assert.True(result);

        var deletedBook = await _repository.GetByIdAsync(createdId);
        Assert.Null(deletedBook);

        
    }

    [Fact]
    public async Task UpdateAsync_WhenBookExists()
    {
        // Assert
        var originalBook = new Book
        {
            Title = "Test Book",
            Author = "Test Author",
            PublishedYear = 2024,
            Genre = "Test Genre",
            IsAvailable = true
        };
        var id = await _repository.CreateAsync(originalBook);

        var modifiedBook = new Book
        {
            Id = id,
            Title = "New Book",
            Author = "New Author",
            PublishedYear = 2025,
            Genre = "New Genre",
            IsAvailable = true
        };

        // Act
        bool result = await _repository.UpdateAsync(modifiedBook);
        var updatedBook = await _repository.GetByIdAsync(id);

        // Assert
        Assert.True(result);
        Assert.NotNull(updatedBook);

        Assert.Equal(modifiedBook.Id, updatedBook.Id);
        Assert.Equal(modifiedBook.Title, updatedBook.Title);
        Assert.Equal(modifiedBook.Author, updatedBook.Author);
        Assert.Equal(modifiedBook.Genre, updatedBook.Genre);
        Assert.Equal(modifiedBook.PublishedYear, updatedBook.PublishedYear);
        Assert.Equal(modifiedBook.IsAvailable, updatedBook.IsAvailable);

        
    }

    [Fact]
    public async Task UpdateAsync_WhenBookDoesNotExist()
    {
        // Arrange
        var nonExistentBook = new Book
        {
            Id = 99,
            Title = "Any Title",
            Author = "Any Author",
            PublishedYear = 2024,
            Genre = "Any Genre",
            IsAvailable = true
        };

        // Act
        var result = await _repository.UpdateAsync(nonExistentBook);

        // Assert
        Assert.False(result);

        
    }
}
