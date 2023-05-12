namespace CodeOffer.Library.Exceptions;

public class AssetNotFoundException : Exception
{
    public AssetNotFoundException(string message) : base(message) {}
}