using FluentValidation;
using OneClickYatra.Api.Globals;
using OneClickYatra.Api.Models.Requests;

namespace OneClickYatra.Api.AppValidations;

public sealed class CreateStaffUserRequestValidator : AbstractValidator<CreateStaffUserRequest>
{
    private static readonly string[] ValidRoles =
        [RoleConstants.TravelAgent, RoleConstants.OperationsStaff, RoleConstants.Finance, RoleConstants.SuperAdmin];

    public CreateStaffUserRequestValidator()
    {
        RuleFor(request => request.FullName).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(request => request.Password)
            .NotEmpty()
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
        RuleFor(request => request.Role).Must(role => ValidRoles.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}.");
    }
}

public sealed class UpdateUserRoleRequestValidator : AbstractValidator<UpdateUserRoleRequest>
{
    private static readonly string[] ValidRoles =
        [RoleConstants.TravelAgent, RoleConstants.OperationsStaff, RoleConstants.Finance, RoleConstants.SuperAdmin];

    public UpdateUserRoleRequestValidator()
    {
        RuleFor(request => request.Role).Must(role => ValidRoles.Contains(role))
            .WithMessage($"Role must be one of: {string.Join(", ", ValidRoles)}.");
    }
}
