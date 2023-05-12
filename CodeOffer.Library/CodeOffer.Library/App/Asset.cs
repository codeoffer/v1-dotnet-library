using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using CodeOffer.Library.Api;
using CodeOffer.Library.Exceptions;
using CodeOffer.Library.OAuth;
using Newtonsoft.Json.Linq;
using TimeoutException = System.TimeoutException;

namespace CodeOffer.Library.App;

public class Asset
{
    public Asset(string uuid, string identifier, string name, string? description, bool access, bool active)
    {
        Uuid = uuid;
        Identifier = identifier;
        Name = name;
        Description = description;
        Access = access;
        Active = active;
    }

    public string Uuid { get; }
    public string Identifier { get; }
    public string Name { get; }
    public string? Description { get; }
    public bool Access { get; }
    public bool Active { get; }
    private readonly SessionToken? _sessionToken = null;

    /// <summary>
    /// Returns the value of the current asset.
    /// </summary>
    /// <returns>byte[]?</returns>
    /// <exception cref="Exception">Occurs when the API gives back an error.</exception>
    /// <exception cref="InvalidDataException">Occurs when the value is not valid Base64 data.</exception>
    public async Task<byte[]?> GetValueAsync()
    {
        var api = new Controller(new HttpClient());
        var request = new Request(HttpMethod.Get, "https://dev-api.codeoffer.net/v1/app/asset")
        {
            SessionToken = _sessionToken,
            Content = new StringContent($"uuid={Uuid}")
        };
        var response = await api.SendRequestAsync(request);
        if (response.Status != HttpStatusCode.OK)
        {
            throw response.Status switch
            {
                HttpStatusCode.BadRequest => new BadRequestException(response.Message),
                HttpStatusCode.Conflict => new ConflictException(response.Message),
                HttpStatusCode.Forbidden => new ForbiddenException(response.Message),
                HttpStatusCode.InternalServerError => new InternalServerException(response.Message),
                HttpStatusCode.NotFound => new NotFoundException(response.Message),
                HttpStatusCode.RequestTimeout => new TimeoutException(response.Message),
                HttpStatusCode.Unauthorized => new UnauthorizedException(response.Message),
                _ => new Exception(response.Message)
            };
        }
        var data = (JObject?)response.Data;
        if (data == null) throw new Exception("No data was found in the server's response.");
        var value = (string?)data["value64"];
        if (value == null) return null;
        if (!Utilities.IsBase64String(value)) throw new InvalidDataException("The value contained in the asset is not valid Base64 data.");
        return Convert.FromBase64String(value);
    }

    public string GetPurchaseLink()
    {
#if DEBUG
    return $"http://localhost/asset/{Uuid}/purchase";
#else
    return $"https://codeoffer.net/asset/{Uuid}/purchase";
#endif
    }
    
    public void OpenPurchaseLink()
    {
        var url = GetPurchaseLink();
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
        else
        {
            throw new Exception("Couldn't open link, because the current operating system is unknown.");
        }
    }
}