using FluentValidation;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class PackageRequestValidator : AbstractValidator<PackageRequest>
{
    public PackageRequestValidator()
    {
        RuleFor(request => request.DestinationId).NotEmpty();
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Slug).NotEmpty().MaximumLength(220)
            .Matches("^[a-z0-9]+(-[a-z0-9]+)*$").WithMessage("Slug must be lowercase, hyphen-separated (e.g. golden-triangle-5n6d).");
        RuleFor(request => request.DurationDays).GreaterThan(0);
        RuleFor(request => request.DurationNights).GreaterThanOrEqualTo(0);
        RuleFor(request => request.ShortDescription).MaximumLength(300);
    }
}

public sealed class PackageStatusRequestValidator : AbstractValidator<PackageStatusRequest>
{
    public PackageStatusRequestValidator()
    {
        RuleFor(request => request.Status).NotEmpty().Must(status => status is "Draft" or "Published")
            .WithMessage("Status must be 'Draft' or 'Published'.");
    }
}

public sealed class PackageItineraryDayRequestValidator : AbstractValidator<PackageItineraryDayRequest>
{
    public PackageItineraryDayRequestValidator()
    {
        RuleFor(request => request.DayNumber).GreaterThan(0);
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
    }
}

public sealed class PackageInclusionRequestValidator : AbstractValidator<PackageInclusionRequest>
{
    public PackageInclusionRequestValidator()
    {
        RuleFor(request => request.Description).NotEmpty().MaximumLength(400);
    }
}

public sealed class PackagePricingTierRequestValidator : AbstractValidator<PackagePricingTierRequest>
{
    public PackagePricingTierRequestValidator()
    {
        RuleFor(request => request.TierName).NotEmpty().MaximumLength(100);
        RuleFor(request => request.PricePerPerson).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Currency).NotEmpty().Length(3);
    }
}

public sealed class PackageInventoryRequestValidator : AbstractValidator<PackageInventoryRequest>
{
    public PackageInventoryRequestValidator()
    {
        RuleFor(request => request.TotalSeats).GreaterThanOrEqualTo(0);
        RuleFor(request => request.BookedSeats).GreaterThanOrEqualTo(0);
        RuleFor(request => request.BookedSeats).LessThanOrEqualTo(request => request.TotalSeats)
            .WithMessage("Booked seats cannot exceed total seats.");
        RuleFor(request => request.Status).Must(status => status is "Open" or "Closed" or "SoldOut")
            .WithMessage("Status must be 'Open', 'Closed' or 'SoldOut'.");
    }
}

public sealed class PackageMediaRequestValidator : AbstractValidator<PackageMediaRequest>
{
    public PackageMediaRequestValidator()
    {
        RuleFor(request => request.MediaUrl).NotEmpty().MaximumLength(500);
        RuleFor(request => request.MediaType).Must(type => type is "Image" or "Video")
            .WithMessage("MediaType must be 'Image' or 'Video'.");
    }
}
