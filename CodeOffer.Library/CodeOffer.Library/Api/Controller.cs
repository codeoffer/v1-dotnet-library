using Newtonsoft.Json.Linq;

namespace CodeOffer.Library.Api;

public class Controller
{
    private HttpClient HttpClient { get; }
    public static bool Verbose;
    public Controller(HttpClient httpClient)
    {
        HttpClient = httpClient;
    }

    /// <summary>
    /// Sends a request to the CodeOffer API with given request data.
    /// </summary>
    /// <param name="request">The request data.</param>
    /// <returns>Response</returns>
    /// <exception cref="InvalidDataException">Occurs when the API response is not valid JSON data.</exception>
    /// <exception cref="MissingFieldException">Occurs when one or more fields are missing in a response.</exception>
    public async Task<Response> SendRequestAsync(Request request)
    {
        var url = request.Url;
#if DEBUG
#else
    url = url.Replace("https://dev-api", "https://api");
#endif
        HttpContent? content = null;
        if (request.Content != null)
        {
            if (request.Method == HttpMethod.Get || request.Method == HttpMethod.Delete)
            {
                if (request.Content is StringContent)
                {
                    url += $"?{await request.Content.ReadAsStringAsync()}";
                }
                else
                {
                    throw new InvalidDataException("If the request method is GET or DELETE, the given content must be plain text.");
                }
            }
            else
            {
                content = request.Content;
            }
        }
        
        var uri = new Uri(url, UriKind.Absolute);
        var requestMessage = new HttpRequestMessage(request.Method, uri);
        requestMessage.Content = content;
        requestMessage.Headers.Authorization = request.AuthenticationHeader;
        if (request.SessionToken != null)
        {
            requestMessage.Headers.Add("OAuth-Session", request.SessionToken.Token);
        }
        if (Verbose)
        {
            Console.WriteLine("[VERBOSE]\tRequest:");
            Console.WriteLine($"[VERBOSE]\t\tURL: {requestMessage.RequestUri}");
            foreach (var header in requestMessage.Headers)
            {
                Console.WriteLine($"[VERBOSE]\t\tHeader: \"{header.Key}: {string.Join(";", header.Value)}\"");
            }

            if (requestMessage.Headers.Authorization != null)
            {
                Console.WriteLine($"[VERBOSE]\t\tHeader: \"Authorization: {requestMessage.Headers.Authorization.Scheme} {requestMessage.Headers.Authorization.Parameter}\"");
            }

            if (requestMessage.Content != null)
            {
                switch (requestMessage.Content)
                {
                    case StringContent:
                        Console.WriteLine($"[VERBOSE]\t\tContent: {await requestMessage.Content.ReadAsStringAsync()}");
                        break;
                }
                switch (requestMessage.Content)
                {
                    case ByteArrayContent:
                    {
                        var bytes = await requestMessage.Content.ReadAsByteArrayAsync();
                        Console.WriteLine($"[VERBOSE]\t\tContent: (bytes) {bytes.Length}");
                        break;
                    }
                    case MultipartContent:
                        Console.WriteLine("[VERBOSE]\t\tContent: MultipartContent");
                        break;
                }
                
                if (requestMessage.Content is MultipartFormDataContent)
                {
                    Console.WriteLine("[VERBOSE]\t\tContent: MultipartFormDataContent");
                }
            }
        }
        var response = await HttpClient.SendAsync(requestMessage);
        var responseContent = await response.Content.ReadAsStringAsync();
        if (Verbose)
        {
            Console.WriteLine("[VERBOSE]\tResponse:");
            Console.WriteLine($"[VERBOSE]\t\tStatus: {response.StatusCode}");
            Console.WriteLine("[VERBOSE]\t\tContent: ");
            Console.WriteLine($"\t\t\t{responseContent.Replace("\n", "\n\t\t\t")}");
        }
        if (!Utilities.IsValidJson(responseContent)) throw new InvalidDataException("The response returned by the server is in an invalid JSON format.");
        var json = JObject.Parse(responseContent);
        var code = (int?)json["code"];
        var message = (string?)json["message"];
        var data = json["data"];
        var experimental = json["experimental"];
        if (code == null) throw new MissingFieldException("The response from the server has one missing important property: code");
        if (message == null) throw new MissingFieldException("The response from the server has one missing important property: message");
        if (data == null) throw new MissingFieldException("The response from the server has one missing important property: data");
        if (experimental == null) throw new MissingFieldException("The response from the server has one missing important property: experimental");

        return new Response(response.StatusCode, (int)code, message, data, experimental);
    }
}