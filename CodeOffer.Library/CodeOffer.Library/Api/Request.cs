using System.Net.Http.Headers;
using CodeOffer.Library.OAuth;

namespace CodeOffer.Library.Api;

public class Request
{
    public AuthenticationHeaderValue? AuthenticationHeader = null;
    public SessionToken? SessionToken = null;
    public HttpMethod Method { get; }
    public string Url { get; }
    public HttpContent? Content = null;

    public Request(HttpMethod method, string url)
    {
        Method = method;
        Url = url;
    }
}