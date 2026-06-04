using Microsoft.Data.Sqlite;
using Dapper;
using System.Diagnostics.CodeAnalysis;
using WebLibraryApi.Models.Book;

namespace WebLibraryApi.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly string _connectionString;
        public BookRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(_connectionString));
        }

        public async Task<IEnumerable<Book>> GetAsync(int offset, int limit, BookFilter filter)
        {
            (string dataSql, DynamicParameters parameters) = BuildSelectQuery(offset, limit, filter);

            using var connection = new SqliteConnection(_connectionString);
            return await connection.QueryAsync<Book>(dataSql, parameters);
        }

        public async Task<int> GetTotalCountAsync(BookFilter filter)
        {
            (string sql, DynamicParameters parameters) = BuildCountQuery(filter);

            using var connection = new SqliteConnection(_connectionString);
            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            string sql = "SELECT * FROM book WHERE id = @Id";

            using var connection = new SqliteConnection(_connectionString);
            var book = await connection.QuerySingleOrDefaultAsync<Book>(sql, new {Id = id});

            return book;
        }

        public async Task<Book?> GetByTitleAsync(string title)
        {
            string sql = "SELECT * FROM book WHERE title = @Title";

            using var connection = new SqliteConnection(_connectionString);
            var book = await connection.QuerySingleOrDefaultAsync<Book>(sql, new { Title = title });

            return book;
        }
        public async Task<bool> ExistAsync(int id)
        {
            string sql = "SELECT EXISTS(SELECT 1 FROM book WHERE id = @Id)";

            using var connection = new SqliteConnection(_connectionString);
            return await connection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            string sql = "DELETE FROM book WHERE id = @Id";

            using var connection = new SqliteConnection(_connectionString);
            var result = await connection.ExecuteAsync(sql, new {Id = id});

            return result > 0;
        }

        public async Task<bool> ChangeAvailabilityAsync(int id, bool isAvailable)
        {
            string sql = "UPDATE book SET is_available = @IsAvailable WHERE id = @Id";

            using var connection = new SqliteConnection(_connectionString);
            var result = await connection.ExecuteAsync(sql, new {IsAvailable = isAvailable, Id = id});

            return result > 0;
        }

        public async Task<int> CreateAsync(Book book)
        {
            string sql = @"INSERT INTO book (title, author, published_year, genre, is_available)
                            VALUES (@Title, @Author, @PublishedYear, @Genre, @IsAvailable);
                            SELECT last_insert_rowid();";

            using var connection = new SqliteConnection(_connectionString);
            return await connection.QuerySingleOrDefaultAsync<int>(sql, book);
        }

        public async Task<bool> UpdateAsync(Book book)
        {
            string sql = @"UPDATE book
                            SET title = @Title,
                                author = @Author,
                                published_year = @PublishedYear,
                                genre = @Genre,
                                is_available = @IsAvailable
                            WHERE id = @Id";

            using var connection = new SqliteConnection(_connectionString);
            int result = await connection.ExecuteAsync(sql, book);

            return result > 0;
        }

        private static (string, DynamicParameters) BuildSelectQuery(int offset, int limit, BookFilter filter)
        {
            var parameters = new DynamicParameters();
            string whereSection = BuildWhereSection(filter, parameters);

            string sql = @$"
                SELECT * FROM book
                {whereSection}
                ORDER BY id
                LIMIT @Limit OFFSET @Offset";

            parameters.Add("@Offset", offset);
            parameters.Add("@Limit",limit);

            return (sql, parameters);
        }

        private static (string, DynamicParameters) BuildCountQuery(BookFilter filter)
        {
            var parameters = new DynamicParameters();
            string whereSection = BuildWhereSection(filter, parameters);

            string sql = $"SELECT COUNT(*) FROM book {whereSection}";

            return (sql, parameters);
        }

        private static string BuildWhereSection(BookFilter filter, DynamicParameters parameters)
        {
            if (filter.IsEmpty)
                return string.Empty;

            var conditions = new List<string>();

            if (!string.IsNullOrEmpty(filter.Genre))
            {
                conditions.Add("genre LIKE @Genre || '%'");
                parameters.Add("@Genre", filter.Genre);
            }
            if (filter.IsAvailable.HasValue)
            {
                conditions.Add("is_available = @IsAvailable");
                parameters.Add("@IsAvailable", filter.IsAvailable.Value ? 1 : 0);
            }

            var whereSection = conditions.Count != 0
                ? $"WHERE {string.Join(" AND ", conditions)}"
                : string.Empty;

            return whereSection;
        }
    }
}
