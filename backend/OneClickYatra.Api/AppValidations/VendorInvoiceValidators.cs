using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class VendorInvoiceStatusRequestValidator : AbstractValidator<VendorInvoiceStatusRequest>
{
    private static readonly string[] ValidStatuses = ["Pending", "Reviewed"];

    public VendorInvoiceStatusRequestValidator()
    {
        RuleFor(request => request.Status).Must(status => ValidStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
    }
}
