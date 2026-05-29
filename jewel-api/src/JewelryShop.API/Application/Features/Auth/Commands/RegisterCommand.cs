using FluentValidation;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Common.Interfaces;
using JewelryShop.Application.Features.Auth.DTOs;
using JewelryShop.Application.Features.Users;
using MediatR;

namespace JewelryShop.Application.Features.Auth.Commands;

public record RegisterCommand(
    string Name,
    string Surname,
    string Email,
    string Password,
    DateOnly? Birthday) : IRequest<AuthResponse>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Surname).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(x => x.Birthday)
            .Must(b => b is null || b.Value < DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("La date de naissance doit être dans le passé.");
    }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
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
        await _users.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return new AuthResponse(token, user.Id, user.Role.ToString());
    }
}
