using Dapper;
using Microsoft.Data.Sqlite;

namespace WebLibraryApi.Extensions
{
    public static class DatabaseExtensions
    {
        public static void EnsureDatabaseCreated(this IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(connectionString));
            var dataSource = connectionString.Replace("Data Source=", "");
            var directory = Path.GetDirectoryName(dataSource);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            connection.Execute(@"
            CREATE TABLE IF NOT EXISTS book (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                author TEXT NOT NULL,
                published_year INTEGER NOT NULL,
                genre TEXT,
                is_available INTEGER NOT NULL
            )");

            var seedRelativePath = Environment.GetEnvironmentVariable("DB_SEED_FILE");

            if (seedRelativePath is null) 
                return;

            var sqlFilePath = Path.Combine(Directory.GetCurrentDirectory(), seedRelativePath);

            int bookCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM book");

            if (bookCount == 0 && File.Exists(sqlFilePath))
            {
                var sql = File.ReadAllText(sqlFilePath);
                connection.Execute(sql);
            }
        }
    }
}
