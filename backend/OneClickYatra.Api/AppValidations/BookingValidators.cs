using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class BookingRequestValidator : AbstractValidator<BookingRequest>
{
    public BookingRequestValidator()
    {
        RuleFor(request => request.CustomerId).NotEmpty();
        RuleFor(request => request.NumberOfAdults).GreaterThanOrEqualTo(1);
        RuleFor(request => request.NumberOfChildren).GreaterThanOrEqualTo(0);
        RuleFor(request => request.TotalAmount).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Notes).MaximumLength(1000);
    }
}

public sealed class BookingStatusRequestValidator : AbstractValidator<BookingStatusRequest>
{
    private static readonly string[] ValidStatuses =
        ["Draft", "Quoted", "PendingPayment", "Confirmed", "InProgress", "Completed", "Cancelled", "RefundPending", "Refunded"];

    public BookingStatusRequestValidator()
    {
        RuleFor(request => request.Status).Must(status => ValidStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
        RuleFor(request => request.Reason).MaximumLength(500);
    }
}

public sealed class BookingCancelRequestValidator : AbstractValidator<BookingCancelRequest>
{
    public BookingCancelRequestValidator()
    {
        RuleFor(request => request.Reason).MaximumLength(500);
    }
}

public sealed class BookingRefundRequestValidator : AbstractValidator<BookingRefundRequest>
{
    public BookingRefundRequestValidator()
    {
        RuleFor(request => request.Reason).MaximumLength(500);
    }
}

public sealed class BookingPassengerRequestValidator : AbstractValidator<BookingPassengerRequest>
{
    public BookingPassengerRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Age).InclusiveBetween(0, 120).When(request => request.Age.HasValue);
        RuleFor(request => request.Gender).MaximumLength(20);
        RuleFor(request => request.IdProofType).MaximumLength(50);
        RuleFor(request => request.IdProofNumber).MaximumLength(50);
    }
}

public sealed class BookingAddOnRequestValidator : AbstractValidator<BookingAddOnRequest>
{
    public BookingAddOnRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Description).MaximumLength(500);
        RuleFor(request => request.Price).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Quantity).GreaterThanOrEqualTo(1);
    }
}
