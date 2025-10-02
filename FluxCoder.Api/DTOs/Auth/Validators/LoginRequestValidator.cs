using FluentValidation;

namespace FluxCoder.Api.DTOs.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("Имя пользователя обязателен к заполнению")
            .MinimumLength(3).WithMessage("Имя пользователя должен содержать не менее 3 символов.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен к заполнению")
            .MinimumLength(6).WithMessage("Пароль должен содержать не менее 6 символов.");
    }
}