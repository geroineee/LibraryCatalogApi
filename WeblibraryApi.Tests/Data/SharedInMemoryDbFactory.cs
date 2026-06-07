using Microsoft.Data.Sqlite;
using System.Data;
using WebLibraryApi.Data;

namespace WebLibraryApi.Tests.Data
{
    public class SharedInMemoryDbFactory(string connectionString) : IDbConnectionFactory
    {
        private readonly string _connectionString = connectionString;
        public IDbConnection Create() => new SqliteConnection(_connectionString);
    }
}
