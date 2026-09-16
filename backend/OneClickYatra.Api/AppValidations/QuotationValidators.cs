using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class QuotationRequestValidator : AbstractValidator<QuotationRequest>
{
    public QuotationRequestValidator()
    {
        RuleFor(request => request.LeadId).NotEmpty();
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Notes).MaximumLength(1000);
    }
}

public sealed class QuotationOptionItemRequestValidator : AbstractValidator<QuotationOptionItemRequest>
{
    public QuotationOptionItemRequestValidator()
    {
        RuleFor(request => request.Description).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Category).MaximumLength(50);
    }
}

public sealed class QuotationOptionRequestValidator : AbstractValidator<QuotationOptionRequest>
{
    public QuotationOptionRequestValidator()
    {
        RuleFor(request => request.OptionName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.HotelCategory).MaximumLength(50);
        RuleFor(request => request.NumberOfPeople).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PricePerPerson).GreaterThanOrEqualTo(0);
        RuleFor(request => request.DurationDays).GreaterThanOrEqualTo(0).When(request => request.DurationDays.HasValue);
        RuleFor(request => request.DurationNights).GreaterThanOrEqualTo(0).When(request => request.DurationNights.HasValue);
        RuleForEach(request => request.Items).SetValidator(new QuotationOptionItemRequestValidator());
    }
}

public sealed class QuotationPublicApproveRequestValidator : AbstractValidator<QuotationPublicApproveRequest>
{
    public QuotationPublicApproveRequestValidator()
    {
        RuleFor(request => request.SelectedOptionId).NotEmpty();
        RuleFor(request => request.ApprovedByName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Comments).MaximumLength(500);
    }
}

public sealed class QuotationPublicRejectRequestValidator : AbstractValidator<QuotationPublicRejectRequest>
{
    public QuotationPublicRejectRequestValidator()
    {
        RuleFor(request => request.RejectedByName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Reason).MaximumLength(500);
    }
}
