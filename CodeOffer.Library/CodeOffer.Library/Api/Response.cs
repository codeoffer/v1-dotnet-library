using System.Net;
using Newtonsoft.Json.Linq;

namespace CodeOffer.Library.Api;

public class Response
{
    public HttpStatusCode Status { get; }
    public int Code { get; }
    public string Message { get; }
    public JToken Data { get; }
    public JToken Experimental { get; }

    public Response(HttpStatusCode status, int code, string message, JToken data, JToken experimental)
    {
        Status = status;
        Code = code;
        Message = message;
        Data = data;
        Experimental = experimental;
    }
}