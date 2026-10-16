using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Interfaces;
using JewelryShop.Features.Auth.DTOs;
using MediatR;

namespace JewelryShop.Features.Auth.Commands;

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
