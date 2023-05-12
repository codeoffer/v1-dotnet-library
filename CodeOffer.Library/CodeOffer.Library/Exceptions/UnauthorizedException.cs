namespace CodeOffer.Library.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) {}
}