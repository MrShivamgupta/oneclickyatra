using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class PaymentInitiateRequestValidator : AbstractValidator<PaymentInitiateRequest>
{
    public PaymentInitiateRequestValidator()
    {
        RuleFor(request => request.BookingId).NotEmpty();
    }
}

public sealed class PaymentRefundRequestValidator : AbstractValidator<PaymentRefundRequest>
{
    public PaymentRefundRequestValidator()
    {
        RuleFor(request => request.BookingId).NotEmpty();
        RuleFor(request => request.Reason).MaximumLength(500);
    }
}
