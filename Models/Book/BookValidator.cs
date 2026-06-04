using FluentValidation;
namespace WebLibraryApi.Models.Book
{
    public class BookValidator : AbstractValidator<BookRequest>
    {
        private const int MinYear = 1450;
        private static int CurrentYear => DateTime.Now.Year;
        public BookValidator()
        {
            RuleFor(book => book.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
                
            RuleFor(book => book.Author)
                .NotEmpty().WithMessage("Author is required");

            RuleFor(book => book.PublishedYear)
                .GreaterThanOrEqualTo(MinYear).WithMessage($"Published year must be at least {MinYear}")
                .LessThanOrEqualTo(CurrentYear).WithMessage($"Published year cannot be in the future (max {CurrentYear})");
        }
    }
}
