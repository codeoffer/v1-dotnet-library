namespace CodeOffer.Library.Exceptions;

public class TimeoutException : Exception
{
    public TimeoutException(string message) : base(message) {}
}