using System.Security.Claims;
using JewelryShop.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace JewelryShop.API.Tests.Fakes;

/// <summary>Capture la commande envoyée par un controller et renvoie <see cref="Response"/>.</summary>
public class FakeSender : ISender
{
    public object? Response { get; set; }

    public object? LastRequest { get; private set; }

    /// <summary>
    /// Branche le sender sur le controller (qui le résout dans <c>HttpContext.RequestServices</c>)
    /// et connecte l'utilisateur <paramref name="userId"/> avec le rôle <paramref name="role"/>.
    /// </summary>
    public void AttachTo(ControllerBase controller, long userId = 1, UserRole role = UserRole.User)
    {
        var services = new ServiceCollection().AddSingleton<ISender>(this).BuildServiceProvider();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString()), new Claim(ClaimTypes.Role, role.ToString())],
            authenticationType: "Test"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = services, User = user }
        };
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        return Task.FromResult((TResponse)Response!);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        LastRequest = request;
        return Task.CompletedTask;
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        return Task.FromResult(Response);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();
}
