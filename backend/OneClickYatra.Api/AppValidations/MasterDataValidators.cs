using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class CountryRequestValidator : AbstractValidator<CountryRequest>
{
    public CountryRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        RuleFor(request => request.IsoCode).NotEmpty().Length(2, 3).Matches("^[A-Za-z]+$")
            .WithMessage("IsoCode must be 2-3 letters (e.g. IN, USA).");
    }
}

public sealed class CityRequestValidator : AbstractValidator<CityRequest>
{
    public CityRequestValidator()
    {
        RuleFor(request => request.CountryId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
    }
}

public sealed class CategoryRequestValidator : AbstractValidator<CategoryRequest>
{
    public CategoryRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Slug).NotEmpty().MaximumLength(160)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug must be lowercase, hyphen-separated (e.g. honeymoon-packages).");
        RuleFor(request => request.Description).MaximumLength(500);
    }
}

public sealed class SeasonRequestValidator : AbstractValidator<SeasonRequest>
{
    public SeasonRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.StartMonth).InclusiveBetween(1, 12);
        RuleFor(request => request.EndMonth).InclusiveBetween(1, 12);
    }
}

public sealed class DestinationRequestValidator : AbstractValidator<DestinationRequest>
{
    public DestinationRequestValidator()
    {
        RuleFor(request => request.CountryId).NotEmpty();
        RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Slug).NotEmpty().MaximumLength(160)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug must be lowercase, hyphen-separated (e.g. bali-indonesia).");
        RuleFor(request => request.ShortDescription).MaximumLength(300);
        RuleFor(request => request.HeroImageUrl).MaximumLength(500);
    }
}

public sealed class DestinationSearchRequestValidator : AbstractValidator<DestinationSearchRequest>
{
    public DestinationSearchRequestValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
    }
}
