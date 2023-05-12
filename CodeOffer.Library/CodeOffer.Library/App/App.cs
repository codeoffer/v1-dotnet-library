using System.Net;
using System.Security.Cryptography;
using CodeOffer.Library.Api;
using CodeOffer.Library.Exceptions;
using CodeOffer.Library.OAuth;
using Newtonsoft.Json.Linq;
using TimeoutException = CodeOffer.Library.Exceptions.TimeoutException;

namespace CodeOffer.Library.App;

public class App
{
    private App(string appId, SessionToken sessionToken)
    {
        _appId = appId;
        _sessionToken = sessionToken;
    }

    private readonly string _appId;
    private readonly SessionToken _sessionToken;

    /// <summary>
    /// Creates a new App object initialized with a SessionToken and AppID by a given SessionToken.
    /// </summary>
    /// <param name="sessionToken"></param>
    /// <returns>App</returns>
    public static App BySessionToken(SessionToken sessionToken)
    {
        return new App(sessionToken.App, sessionToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns>List of Assets</returns>
    /// <exception cref="Exception">Occurs when the API gives back an error.</exception>
    /// <exception cref="MissingFieldException">Occurs when one or more fields are missing in a response.</exception>
    public async Task<AssetDirectory> GetAssetDirectoryAsync()
    {
        if (_sessionToken.Account == null) throw new UnauthorizedException("The user must be logged in to perform this action.");
        var api = new Controller(new HttpClient());
        var request = new Request(HttpMethod.Get, "https://dev-api.codeoffer.net/v1/app/assets")
        {
            SessionToken = _sessionToken,
            Content = new StringContent($"uuid={_appId}")
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
        var data = (JArray?)response.Data;
        if (data == null) throw new Exception("No data was found in the server's response.");
        var assets = new List<Asset>();
        foreach (var jToken in data)
        {
            var asset = (JObject)jToken;
            var uuid = (string?)asset["uuid"];
            var identifier = (string?)asset["identifier"];
            var name = (string?)asset["name"];
            var description = (string?)asset["description"];
            var access = (bool?)asset["access"];
            var active = (bool?)asset["active"];
            if(uuid == null) throw new MissingFieldException("The response from the server has one missing important property: uuid");
            if(identifier == null) throw new MissingFieldException("The response from the server has one missing important property: identifier");
            if(name == null) throw new MissingFieldException("The response from the server has one missing important property: name");
            if(access == null) throw new MissingFieldException("The response from the server has one missing important property: access");
            if(active == null) throw new MissingFieldException("The response from the server has one missing important property: active");
            assets.Add(new Asset(uuid, identifier, name, description, (bool)access, (bool)active));
        }

        return new AssetDirectory(assets, this);
    }
}