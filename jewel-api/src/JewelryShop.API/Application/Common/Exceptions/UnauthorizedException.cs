namespace JewelryShop.Application.Common.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message = "Identifiants invalides.")
        : base(message)
    {
    }
}
