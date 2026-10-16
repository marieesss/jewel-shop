using FluentValidation;
using JewelryShop.Application.Common.Exceptions;
using JewelryShop.Application.Common.Interfaces;
using JewelryShop.Application.Features.Auth.DTOs;
using JewelryShop.Application.Features.Users;
using MediatR;

namespace JewelryShop.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("L'email est requis.")
            .EmailAddress().WithMessage("L'email n'est pas valide.");
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Le mot de passe est requis.");
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokens;

    public LoginCommandHandler(
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

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Email ou mot de passe incorrect.");

        var refreshToken = _refreshTokens.Issue(user);
        await _users.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return new AuthResult(new AuthResponse(token, user.Id, user.Role.ToString()), refreshToken);
    }
}
