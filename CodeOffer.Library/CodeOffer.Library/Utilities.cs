using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CodeOffer.Library;

public abstract class Utilities
{
    public static bool IsValidJson(string strInput)
    {
        if (string.IsNullOrWhiteSpace(strInput)) { return false;}
        strInput = strInput.Trim();
        if ((!strInput.StartsWith("{") || !strInput.EndsWith("}")) &&
            (!strInput.StartsWith("[") || !strInput.EndsWith("]"))) return false;
        try
        {
            JToken.Parse(strInput);
            return true;
        }
        catch (JsonReaderException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    public static bool IsBase64String(string base64)
    {
        var buffer = new Span<byte>(new byte[base64.Length]);
        return Convert.TryFromBase64String(base64, buffer , out _);
    }
}