using FluentValidation;
using GiftBoxy.Application.DTOs.Product;

namespace GiftBoxy.Application.Validators.Product
{
    public class ProductCreateDtoValidator : AbstractValidator<ProductCreateDto>
    {
        public ProductCreateDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title boş ola bilməz")
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description boş ola bilməz")
                .MaximumLength(2000);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price 0-dan böyük olmalıdır");

            RuleFor(x => x.StockCount)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.CategoryId)
                .GreaterThan(0);
        }
    }
}
