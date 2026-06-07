using System.Net;
using System.Text;
using System.Text.Json;

namespace WebLibraryApi.Tests.IntegrationTests
{
    public class BookEndpointTests : IClassFixture<IntegrationTestFactory>
    {
        private readonly HttpClient _client;

        public BookEndpointTests(IntegrationTestFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PostBook_ValidRequest()
        {
            // 1. Arrange
            var requestPayload = new
            {
                Title = "Integration Test Book",
                Author = "Test Author",
                PublishedYear = 2023,
                Genre = "Sci-Fi"
            };

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            var content = new StringContent(
                JsonSerializer.Serialize(requestPayload, jsonOptions),
                Encoding.UTF8,
                "application/json");

            // Act
            var response = await _client.PostAsync("/api/books", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            Assert.NotNull(response.Headers.Location);
            Assert.StartsWith("/api/books/", response.Headers.Location.ToString());
        }
    }
}
