using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using CodeOffer.Library.Api;
using CodeOffer.Library.Exceptions;
using Newtonsoft.Json.Linq;
using TimeoutException = System.TimeoutException;

namespace CodeOffer.Library.OAuth;

public class SessionToken
{
    public string Token { get; }
    public string? Account { get; set; }
    public int Expires { get; }
    public string App { get; }

    public SessionToken(string token, string? account, int expires, string app)
    {
        Token = token;
        Account = account;
        Expires = expires;
        App = app;
    }
    
    /// <summary>
    /// Wait's until the user confirms the authentication.
    /// </summary>
    /// <exception cref="Exception">Occurs when the API gives back an error.</exception>
    public async Task WaitForConfirmationAsync()
    {
        var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromHours(1);
        var api = new Controller(httpClient);
        var request = new Request(HttpMethod.Get, "https://dev-api.codeoffer.net/v1/oauth/session/state")
        {
            Content = new StringContent($"session={Token}")
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
        var account = (string?)data["account"];
        Account = account ?? throw new MissingFieldException("The response from the server has one missing important property: account");
    }

    /// <summary>
    /// Returns a link with which the user can authenticate himself.
    /// </summary>
    /// <returns></returns>
    public string GetLoginLink()
    {
#if DEBUG
        return $"http://localhost/oauth/login?session={Token}";
#else
        return $"https://codeoffer.net/oauth/login?session={Token}";
#endif
    }

    public void OpenLoginLink()
    {
        if (Account != null) throw new Exception("The user is already authenticated.");
        var url = GetLoginLink();
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