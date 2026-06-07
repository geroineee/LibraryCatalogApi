using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

using WebLibraryApi.Data;
using WebLibraryApi.Tests.Data;

namespace WebLibraryApi.Tests.IntegrationTests;

public class IntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _connectionString = "Data Source=IntegrationTestDb;Mode=Memory;Cache=Shared";
    private SqliteConnection _connection = null!;

    public async Task InitializeAsync()
    {
        // Использование snake_case названий в бд
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        _connection = new SqliteConnection(_connectionString);
        await _connection.OpenAsync();

        var query = @"CREATE TABLE IF NOT EXISTS book (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            title TEXT NOT NULL,
            author TEXT NOT NULL,
            published_year INTEGER,
            genre TEXT,
            is_available INTEGER
        );";

        await _connection.ExecuteAsync(query);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder
            .UseEnvironment("Testing")
            .ConfigureServices(services =>
        {
            services.RemoveAll<IDbConnectionFactory>();
            services.AddTransient<IDbConnectionFactory>(_ => new SharedInMemoryDbFactory(_connectionString));
        });
    }

    public new Task DisposeAsync()
    {
        _connection?.Close();
        _connection?.Dispose();
        return Task.CompletedTask;
    }
}