using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class EnquiryRequestValidator : AbstractValidator<EnquiryRequest>
{
    public EnquiryRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Phone)
            .NotEmpty()
            .Matches(@"^\+?[0-9]{7,15}$").WithMessage("Phone must be a valid number (7-15 digits, optional leading +).");
        RuleFor(request => request.Message).NotEmpty().MaximumLength(1000);
    }
}

public sealed class EnquiryStatusRequestValidator : AbstractValidator<EnquiryStatusRequest>
{
    public EnquiryStatusRequestValidator()
    {
        RuleFor(request => request.Status).NotEmpty().Must(status => status is "New" or "Contacted" or "Converted" or "Closed")
            .WithMessage("Status must be 'New', 'Contacted', 'Converted' or 'Closed'.");
    }
}

public sealed class EnquirySearchRequestValidator : AbstractValidator<EnquirySearchRequest>
{
    public EnquirySearchRequestValidator()
    {
        RuleFor(request => request.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PageSize).InclusiveBetween(1, 100);
        RuleFor(request => request.Status).Must(status => status is null or "New" or "Contacted" or "Converted" or "Closed")
            .WithMessage("Status must be 'New', 'Contacted', 'Converted' or 'Closed'.");
    }
}
