using FluentValidation;
using Microsoft.Data.Sqlite;
using WebLibraryApi.Extensions;
using WebLibraryApi.Exceptions;
using WebLibraryApi.Models.Book;
using WebLibraryApi.Repositories;
using WebLibraryApi.Services;

// Настройка Dapper для маппинга snake_case атрибутов в базе данных
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();

builder.Services.AddScoped<IValidator<BookRequest>, BookValidator>();

var app = builder.Build();

app.Configuration.EnsureDatabaseCreated();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (NotFoundException ex)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (ConflictException ex)
    {
        context.Response.StatusCode = 409;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (SqliteException ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Database error");
        
        context.Response.StatusCode = 503;
        await context.Response.WriteAsJsonAsync(new { error = "Database service unavailable" });
    }
});

app.MapGet("/", () => "Hello!");
app.MapGet("/api/books", async (
    IBookService bookService,
    string? genre,
    bool? available,
    int page = 1,
    int pageSize = 10) =>
{
    if (page < 1)
        return Results.BadRequest(new { error = "Page must be 1 or greater"} );
    if ( pageSize < 1 || pageSize > 100)
        return Results.BadRequest(new { error = "Page size must be between 1 and 100" });

    var request = new BookGetQuery(genre, available, page, pageSize);
    var books = await bookService.GetPagedBooksAsync(request);
    return Results.Ok(books);
});

app.MapGet("/api/books/{id:int}", async (
    IBookService bookService,
    int id) =>
{
    var book = await bookService.GetBookByIdAsync(id);
    return book is not null 
        ? Results.Ok(book)
        : Results.NotFound(new {error = "Book not found" });
});

app.MapPost("/api/books", async (
    IBookService bookService,
    IValidator<BookRequest> validator,
    BookRequest request) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors.First());

    var id = await bookService.CreateBookAsync(request);

    return Results.Created($"/api/books/{id}", new { id });
});

app.MapDelete("/api/books/{id:int}", async (IBookService bookService, int id) =>
{
    await bookService.DeleteBookAsync(id);
    return Results.NoContent();
});

app.MapPut("/api/books/{id:int}", async (
    IValidator<BookRequest> validator,
    IBookService bookService,
    int id,
    BookRequest request) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors.First());

    var updated = await bookService.UpdateBookAsync(id, request);

    return Results.Ok(updated);
});

app.MapPatch("/api/books/{id:int}/availability", async (
    IBookService bookService,
    int id,
    BookPatchRequest request) =>
{
    var updated = await bookService.ChangeAvailabilityAsync(id, request.IsAvailable);

    return Results.Ok(updated);
});

app.Run();