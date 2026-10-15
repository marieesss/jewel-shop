using FluentValidation;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Common.Interfaces;
using JewelryShop.Application.Features.Auth.DTOs;
using JewelryShop.Application.Features.Auth.Validation;
using JewelryShop.Application.Features.Users;
using MediatR;

namespace JewelryShop.Application.Features.Auth.Commands;

public record RegisterCommand(
    string Name,
    string Surname,
    string Email,
    string Password,
    DateOnly? Birthday) : IRequest<AuthResult>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le prénom est requis.")
            .MaximumLength(100).WithMessage("100 caractères maximum.");
        RuleFor(x => x.Surname)
            .NotEmpty().WithMessage("Le nom est requis.")
            .MaximumLength(100).WithMessage("100 caractères maximum.");
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email est requis.")
            .EmailAddress().WithMessage("L'email n'est pas valide.")
            .MaximumLength(255).WithMessage("255 caractères maximum.");
        RuleFor(x => x.Password).PasswordPolicy();
        RuleFor(x => x.Birthday)
            .Must(b => b is null || b.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La date de naissance doit être dans le passé.");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokens;

    public RegisterCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IRefreshTokenService refreshTokens)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _users.ExistsByEmailAsync(email, cancellationToken))
            throw new ConflictException("Un compte existe déjà avec cet email.");

        var user = new User
        {
            Name = request.Name.Trim(),
            Surname = request.Surname.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Birthday = request.Birthday,
            Role = UserRole.User
        };

        await _users.AddAsync(user, cancellationToken);
        var refreshToken = _refreshTokens.Issue(user);
        await _users.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return new AuthResult(new AuthResponse(token, user.Id, user.Role.ToString()), refreshToken);
    }
}
