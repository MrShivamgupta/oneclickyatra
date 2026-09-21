using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class VendorRequestValidator : AbstractValidator<VendorRequest>
{
    private static readonly string[] ValidVendorTypes = ["Hotel", "Airline", "Transport", "DMC", "ActivityProvider"];

    public VendorRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(200);
        RuleFor(request => request.VendorType).Must(type => ValidVendorTypes.Contains(type))
            .WithMessage($"VendorType must be one of: {string.Join(", ", ValidVendorTypes)}.");
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Phone).NotEmpty().MaximumLength(30);
        RuleFor(request => request.Address).MaximumLength(500);
        RuleFor(request => request.City).MaximumLength(100);
        RuleFor(request => request.Country).MaximumLength(100);
    }
}

public sealed class VendorContactRequestValidator : AbstractValidator<VendorContactRequest>
{
    public VendorContactRequestValidator()
    {
        RuleFor(request => request.ContactName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Designation).MaximumLength(100);
        RuleFor(request => request.Phone).NotEmpty().MaximumLength(30);
        RuleFor(request => request.Email).EmailAddress().MaximumLength(256).When(request => !string.IsNullOrWhiteSpace(request.Email));
    }
}

public sealed class VendorRateRequestValidator : AbstractValidator<VendorRateRequest>
{
    public VendorRateRequestValidator()
    {
        RuleFor(request => request.ServiceDescription).NotEmpty().MaximumLength(300);
        RuleFor(request => request.RateAmount).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Currency).NotEmpty().Length(3);
        RuleFor(request => request.ValidTo).GreaterThanOrEqualTo(request => request.ValidFrom!.Value)
            .When(request => request.ValidFrom.HasValue && request.ValidTo.HasValue);
    }
}

public sealed class VendorPaymentRequestValidator : AbstractValidator<VendorPaymentRequest>
{
    public VendorPaymentRequestValidator()
    {
        RuleFor(request => request.Amount).GreaterThan(0);
        RuleFor(request => request.Notes).MaximumLength(500);
    }
}

public sealed class VendorPaymentStatusRequestValidator : AbstractValidator<VendorPaymentStatusRequest>
{
    private static readonly string[] ValidStatuses = ["Pending", "Paid", "Overdue"];

    public VendorPaymentStatusRequestValidator()
    {
        RuleFor(request => request.Status).Must(status => ValidStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
    }
}

public sealed class VendorPerformanceRequestValidator : AbstractValidator<VendorPerformanceRequest>
{
    public VendorPerformanceRequestValidator()
    {
        RuleFor(request => request.Rating).InclusiveBetween(1, 5);
        RuleFor(request => request.Notes).MaximumLength(500);
    }
}

public sealed class VendorLinkUserRequestValidator : AbstractValidator<VendorLinkUserRequest>
{
    public VendorLinkUserRequestValidator()
    {
        RuleFor(request => request.UserId).NotEmpty();
    }
}
