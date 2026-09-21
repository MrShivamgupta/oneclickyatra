using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class FeedbackRequestValidator : AbstractValidator<FeedbackRequest>
{
    public FeedbackRequestValidator()
    {
        RuleFor(request => request.Rating).InclusiveBetween(1, 5);
        RuleFor(request => request.Comment).MaximumLength(1000);
    }
}
