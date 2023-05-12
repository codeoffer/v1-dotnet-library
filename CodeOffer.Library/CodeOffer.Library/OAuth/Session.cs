using System.Net;
using CodeOffer.Library.Api;
using CodeOffer.Library.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimeoutException = System.TimeoutException;

namespace CodeOffer.Library.OAuth;

public class Session
{
    public Session(string appId)
    {
        AppId = appId;
    }
    private string AppId { get; }

    /// <summary>
    /// Creates a new SessionToken.
    /// </summary>
    /// <returns>SessionToken</returns>
    /// <exception cref="Exception">Occurs when the API gives back an error.</exception>
    /// <exception cref="MissingFieldException">Occurs when one or more fields are missing in a response.</exception>
    public async Task<SessionToken> CreateSessionTokenAsync()
    {
        var api = new Controller(new HttpClient());
        var request = new Request(HttpMethod.Put, "https://dev-api.codeoffer.net/v1/oauth/session")
        {
            Content = new StringContent(JsonConvert.SerializeObject(new
            {
                app = AppId
            }))
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
        var token = (string?)data["token"];
        var account = (string?)data["account"];
        var expires = (int?)data["expires"];
        var app = (string?)data["app"];
        if(token == null) throw new MissingFieldException("The response from the server has one missing important property: token");
        if(expires == null) throw new MissingFieldException("The response from the server has one missing important property: expires");
        if(app == null) throw new MissingFieldException("The response from the server has one missing important property: app");

        return new SessionToken(token, account, (int)expires, app);
    }
    
    /// <summary>
    /// Gets a SessionToken object.
    /// </summary>
    /// <param name="sessionToken">The session token to get.</param>
    /// <returns></returns>
    /// <exception cref="Exception">Occurs when the API gives back an error.</exception>
    /// <exception cref="MissingFieldException">Occurs when one or more fields are missing in a response.</exception>
    public static async Task<SessionToken> GetSessionTokenAsync(string sessionToken)
    {
        var api = new Controller(new HttpClient());
        var request = new Request(HttpMethod.Get, "https://dev-api.codeoffer.net/v1/oauth/session")
        {
            Content = new StringContent($"session={sessionToken}")
        };
        var response = await api.SendRequestAsync(request);
        if (response.Status != HttpStatusCode.OK) throw new Exception(response.Message);
        var data = (JObject?)response.Data;
        if (data == null) throw new Exception("No data was found in the server's response.");
        var token = (string?)data["token"];
        var account = (string?)data["account"];
        var expires = (int?)data["expires"];
        var app = (string?)data["app"];
        if(token == null) throw new MissingFieldException("The response from the server has one missing important property: token");
        if(expires == null) throw new MissingFieldException("The response from the server has one missing important property: expires");
        if(app == null) throw new MissingFieldException("The response from the server has one missing important property: app");

        return new SessionToken(token, account, (int)expires, app);
    }
}