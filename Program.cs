using FluentValidation;
using Microsoft.OpenApi;
using System.ComponentModel;
using WebLibraryApi.Data;
using WebLibraryApi.Exceptions;
using WebLibraryApi.Extensions;
using WebLibraryApi.Middlewares;
using WebLibraryApi.Models;
using WebLibraryApi.Models.Book;
using WebLibraryApi.Repositories;
using WebLibraryApi.Services;

// Настройка Dapper для маппинга snake_case атрибутов в базе данных
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<ExceptionHandler>();
builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IValidator<BookRequest>, BookValidator>();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Library API",
        Description = "REST API для библиотечного каталога"
    });
});

var app = builder.Build();

app.UseExceptionHandlerMiddleware();

app.UseSwagger();
app.UseSwaggerUI();

app.Configuration.EnsureDatabaseCreated();

app.MapGet("/", () => "Welcome to home page!")
.WithSummary("Домашняя страница");

app.MapGet("/api/books", async (
    IBookService bookService,
    [Description("Фильтрация по жанру (префиксный поиск)")] string? genre,
    [Description("Фильтрация по доступности (true/false)")] bool? available,
    [Description("Номер страницы")] int page = 1,
    [Description("Размер страницы (1-100)")] int pageSize = 10) =>
{

    if (page < 1)
        return Results.BadRequest("Page must be 1 or greater");

    if (pageSize < 1 || pageSize > 100)
        return Results.BadRequest("Page size must be between 1 and 100");

    var request = new BookGetQuery()
    {
        Genre = genre,
        IsAvailable = available,
        Page = page,
        PageSize = pageSize
    };

    var books = await bookService.GetPagedBooksAsync(request);
    return Results.Ok(books);
})
.WithSummary("Получение списка книг")
.WithDescription("Получение страничного списка книг с фильтрацией по жанру и доступности")
.Produces<PagedResult<BookResponse>>(200)
.Produces(400)
.Produces(503);

app.MapGet("/api/books/{id:int}", async (
    IBookService bookService,
    [Description("ID книги")] int id) =>
{
    var book = await bookService.GetBookByIdAsync(id);
    return book is not null
    ? Results.Ok(book)
    : Results.NotFound($"Book {id} not found");
})
.WithSummary("Получение книги по ID")
.WithDescription("Возвращает книгу с указанным идентификатором")
.Produces<BookResponse>(200)
.Produces(404)
.Produces(503);

app.MapPost("/api/books", async (
    IBookService bookService,
    IValidator<BookRequest> validator,
    [Description("Данные создаваемой книги")] BookRequest request) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    try
    {
        var id = await bookService.CreateBookAsync(request);
        return Results.Created($"/api/books/{id}", new { id });
    }
    catch (ConflictException ex) { return Results.Conflict(ex.Message); }
})
.WithSummary("Создание новой книги")
.WithDescription("Создаёт новую книгу. Название должно быть уникальным.")
.Produces(201)
.Produces(400)
.Produces(409)
.Produces(503);

app.MapDelete("/api/books/{id:int}", async (
    IBookService bookService,
    [Description("ID книги")] int id) =>
{
    try
    {
        await bookService.DeleteBookAsync(id);
        return Results.NoContent();
    }
    catch (NotFoundException ex) { return Results.NotFound(ex.Message); }
})
.WithSummary("Удаление книги")
.WithDescription("Удаляет книгу с указанным идентификатором")
.Produces(204)
.Produces(404)
.Produces(503);

app.MapPut("/api/books/{id:int}", async (
    IValidator<BookRequest> validator,
    IBookService bookService,
    [Description("ID книги")] int id,
    [Description("Обновленные данные книги")] BookRequest request) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    try
    {
        var updated = await bookService.UpdateBookAsync(id, request);
        return Results.Ok(updated);
    }
    catch (NotFoundException ex) { return Results.NotFound(ex.Message); }
    catch (ConflictException ex) { return Results.Conflict(ex.Message); }
})
.WithSummary("Полное обновление книги")
.WithDescription("Заменяет все поля книги. Название должно быть уникальным.")
.Produces<BookResponse>(200)
.Produces(400)
.Produces(409)
.Produces(503);

app.MapPatch("/api/books/{id:int}/availability", async (
    IBookService bookService,
    [Description("ID книги")] int id,
    [Description("Новое значение доступности (true/false)")] BookPatchRequest request) =>
{
    try
    {
        var updated = await bookService.ChangeAvailabilityAsync(id, request.IsAvailable);
        return Results.Ok(updated);
    }
    catch (NotFoundException ex) { return Results.NotFound(ex.Message); }
})
.WithSummary("Изменение доступности книги")
.WithDescription("Обновляет только доступность книги")
.Produces<BookResponse>(200)
.Produces(404)
.Produces(503);

app.Run();