using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class WhatsAppTemplateRequestValidator : AbstractValidator<WhatsAppTemplateRequest>
{
    private static readonly string[] ValidCategories = ["Welcome", "Quotation", "Reminder", "TravelAlert"];

    public WhatsAppTemplateRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Category).Must(category => ValidCategories.Contains(category))
            .WithMessage($"Category must be one of: {string.Join(", ", ValidCategories)}.");
        RuleFor(request => request.BodyText).NotEmpty().MaximumLength(1000);
    }
}

public sealed class WhatsAppSendMessageRequestValidator : AbstractValidator<WhatsAppSendMessageRequest>
{
    public WhatsAppSendMessageRequestValidator()
    {
        RuleFor(request => request.PhoneNumber).NotEmpty().MaximumLength(20);
        RuleFor(request => request.TemplateName).NotEmpty().MaximumLength(100);
    }
}
