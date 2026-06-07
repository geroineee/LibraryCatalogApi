using System.Data;
using Microsoft.Data.Sqlite;

namespace WebLibraryApi.Data
{
    public class SqliteConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqliteConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(_connectionString));
        }

        public IDbConnection Create() => new SqliteConnection(_connectionString);
    }
}
