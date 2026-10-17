using JewelryShop.Common.Exceptions;
using JewelryShop.Common.Interfaces;
using JewelryShop.Features.Auth.DTOs;
using JewelryShop.Infrastructure.Services;
using MediatR;

namespace JewelryShop.Features.Auth.Commands;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokens;
    private readonly TimeProvider _timeProvider;

    public RegisterCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IRefreshTokenService refreshTokens,
        TimeProvider timeProvider)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _refreshTokens = refreshTokens;
        _timeProvider = timeProvider;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
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
            Role = UserRole.User,
            CreatedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        await _users.AddAsync(user, cancellationToken);
        var refreshToken = _refreshTokens.Issue(user);
        await _users.SaveChangesAsync(cancellationToken);

        var token = _jwtService.GenerateToken(user);
        return new RegisterResult(new AuthResponse(token, user.Id, user.Role.ToString()), refreshToken);
    }
}
