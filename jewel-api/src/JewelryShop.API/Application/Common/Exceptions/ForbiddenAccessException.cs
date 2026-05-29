namespace JewelryShop.Application.Common.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "Accès refusé à cette ressource.")
        : base(message)
    {
    }
}
