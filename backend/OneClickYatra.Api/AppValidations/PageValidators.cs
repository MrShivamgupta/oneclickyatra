using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class PageRequestValidator : AbstractValidator<PageRequest>
{
    public PageRequestValidator()
    {
        RuleFor(request => request.Slug).NotEmpty().MaximumLength(160)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug must be lowercase, hyphen-separated (e.g. about-us).");
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
    }
}
