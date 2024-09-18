using FluentValidation;

namespace UserGroupSite.Shared.DTOs;

public class RegisterDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(dto => dto.Email).NotEmpty().EmailAddress();
        RuleFor(dto => dto.Password).NotEmpty().MinimumLength(6).MaximumLength(100)
            .WithMessage("The password must be at least 6 and at max 100 characters long.");
        RuleFor(dto => dto.ConfirmPassword).NotEmpty().Equal(viewModel => viewModel.Password)
            .WithMessage("The password and confirmation password do not match.");
        RuleFor(dto => dto.FirstName).NotEmpty();
        RuleFor(dto => dto.LastName).NotEmpty();
    }
}