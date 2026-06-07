# Library API

REST API для библиотечного каталога на .NET 10 + Dapper + SQLite.

## Запуск без Docker

1. Клонировать репозиторий
2. В корневом каталоге выполнить:

```
dotnet run
```

API: http://localhost:5000
Swagger: http://localhost:5000/swagger

Файл базы данных SQLite будет создан автоматически по пути `./database/books.db`.

## Запуск с Docker

### Docker-compose

```
docker-compose up -d --build
```

### Docker

```
docker build -t library-api .
docker run -d -p 5000:8080 -v ./database:/app/database --name library-api-container library-api
```

API: http://localhost:5000
Swagger: http://localhost:5000/swagger

## Эндпоинты API

| Метод | URL | Описание |
|-------|-----|----------|
| GET | /api/books | Список книг (фильтрация, пагинация) |
| GET | /api/books/{id} | Получить книгу по ID |
| POST | /api/books | Создать книгу |
| PUT | /api/books/{id} | Полностью обновить книгу |
| DELETE | /api/books/{id} | Удалить книгу |
| PATCH | /api/books/{id}/availability | Изменить доступность |

## Бизнес-логика и ограничения

- Название книги (title) — обязательное поле, максимум 200 символов
- Автор (author) — обязательное поле
- Год издания (publishedYear) — от 1450 до текущего года включительно
- Название книги должно быть уникальным
- Пагинация: page — номер страницы (от 1), pageSize — размер страницы (от 1 до 100)
- Фильтрация: по жанру (префиксный поиск, регистронезависимый) и по доступности (true/false)

## Коды ответов

| Код | Описание |
|-----|----------|
| 200 | Успешный GET или PUT |
| 201 | Книга создана (POST) |
| 204 | Успешное удаление (DELETE) |
| 400 | Ошибка валидации или неверные параметры пагинации |
| 404 | Книга не найдена |
| 409 | Конфликт (книга с таким названием уже существует) |
| 503 | Ошибка подключения к базе данных |

## Примеры запросов

GET /api/books — получить список книг
```
curl "http://localhost:5000/api/books?genre=Fantasy&available=true&page=1&pageSize=10"
```
Ответ:
```
{
  "items": [...],
  "totalCount": 25,
  "page": 1,
  "pageSize": 10,
  "totalPages": 3
}
```

GET /api/books/1 — получить книгу по ID
```
curl "http://localhost:5000/api/books/1"
```

Ответ:
```
{
  "id": 1,
  "title": "The Hobbit",
  "author": "J.R.R. Tolkien",
  "publishedYear": 1937,
  "genre": "Fantasy",
  "isAvailable": true
}
```

POST /api/books — создать книгу
```
curl -X POST "http://localhost:5000/api/books" -H "Content-Type: application/json" -d '{"title":"The Hobbit","author":"J.R.R. Tolkien","publishedYear":1937,"genre":"Fantasy","isAvailable":false}'
```

Ответ:
```
{
  "id": 1
}
```

PUT /api/books/1 — полностью обновить книгу
```
curl -X PUT "http://localhost:5000/api/books/1" -H "Content-Type: application/json" -d '{"title":"The Hobbit","author":"J.R.R. Tolkien","publishedYear":1937,"genre":"Fantasy","isAvailable":false}'
```

Ответ:
```
{
  "id": 1,
  "title": "The Hobbit",
  "author": "J.R.R. Tolkien",
  "publishedYear": 1937,
  "genre": "Fantasy",
  "isAvailable": false
}
```

DELETE /api/books/1 — удалить книгу
```
curl -X DELETE "http://localhost:5000/api/books/1"
```

Ответ: 204 No Content

PATCH /api/books/1/availability — изменить доступность
```
curl -X PATCH "http://localhost:5000/api/books/1/availability" -H "Content-Type: application/json" -d '{"isAvailable":false}'
```
Ответ:
```
{
  "id": 1,
  "title": "The Hobbit",
  "author": "J.R.R. Tolkien",
  "publishedYear": 1937,
  "genre": "Fantasy",
  "isAvailable": false
}
```