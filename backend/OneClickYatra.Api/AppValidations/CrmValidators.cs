using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class CustomerRequestValidator : AbstractValidator<CustomerRequest>
{
    public CustomerRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Email).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.Email));
        RuleFor(request => request.Phone).NotEmpty().Matches(@"^\+?[0-9]{7,15}$");
    }
}

public sealed class LeadRequestValidator : AbstractValidator<LeadRequest>
{
    public LeadRequestValidator()
    {
        RuleFor(request => request.CustomerName).NotEmpty().MaximumLength(150);
        RuleFor(request => request.Mobile).NotEmpty().Matches(@"^\+?[0-9]{7,15}$");
        RuleFor(request => request.Email).EmailAddress().When(request => !string.IsNullOrWhiteSpace(request.Email));
        RuleFor(request => request.Budget).GreaterThanOrEqualTo(0).When(request => request.Budget.HasValue);
    }
}

public sealed class LeadStatusRequestValidator : AbstractValidator<LeadStatusRequest>
{
    private static readonly string[] ValidStatuses = ["New", "Contacted", "QuotationSent", "Negotiation", "Confirmed", "Lost"];

    public LeadStatusRequestValidator()
    {
        RuleFor(request => request.Status).Must(status => ValidStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
    }
}

public sealed class LeadAssignRequestValidator : AbstractValidator<LeadAssignRequest>
{
    public LeadAssignRequestValidator()
    {
        RuleFor(request => request.AssignedToUserId).NotEmpty();
    }
}

public sealed class LeadScoreRequestValidator : AbstractValidator<LeadScoreRequest>
{
    public LeadScoreRequestValidator()
    {
        RuleFor(request => request.LeadScore).InclusiveBetween(0, 100);
    }
}

public sealed class FollowUpRequestValidator : AbstractValidator<FollowUpRequest>
{
    private static readonly string[] ValidTypes = ["Call", "WhatsApp", "Email", "Visit"];

    public FollowUpRequestValidator()
    {
        RuleFor(request => request.ScheduledAt).NotEmpty();
        RuleFor(request => request.Type).Must(type => ValidTypes.Contains(type))
            .WithMessage($"Type must be one of: {string.Join(", ", ValidTypes)}.");
        RuleFor(request => request.Notes).MaximumLength(500);
    }
}

public sealed class FollowUpStatusRequestValidator : AbstractValidator<FollowUpStatusRequest>
{
    private static readonly string[] ValidStatuses = ["Pending", "Completed", "Cancelled"];

    public FollowUpStatusRequestValidator()
    {
        RuleFor(request => request.Status).Must(status => ValidStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}.");
    }
}
