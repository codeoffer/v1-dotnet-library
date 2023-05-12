using System.Net;
using CodeOffer.Library.Api;
using CodeOffer.Library.Exceptions;
using CodeOffer.Library.OAuth;
using Newtonsoft.Json.Linq;
using TimeoutException = System.TimeoutException;
using System.Drawing;
using System.Runtime.InteropServices;


namespace CodeOffer.Library;

public class User
{
    private User(string uuid, string username, string? profilePicture, string email, bool access)
    {
        Uuid = uuid;
        Username = username;
        ProfilePicture = profilePicture;
        Email = email;
        Access = access;
    }

    public string Uuid { get; }
    public string Username { get; }
    public string? ProfilePicture { get; }
    public string Email { get; }
    public bool Access { get; }

    /// <summary>
    /// Gets the user assigned to the SessionToken.
    /// </summary>
    /// <param name="sessionToken"></param>
    /// <returns>User</returns>
    /// <exception cref="Exception">Occurs when the API gives back an error.</exception>
    /// <exception cref="MissingFieldException">Occurs when one or more fields are missing in a response.</exception>
    public static async Task<User> GetUserAsync(SessionToken sessionToken)
    {
        var api = new Controller(new HttpClient());
        var request = new Request(HttpMethod.Get, "https://dev-api.codeoffer.net/v1/oauth/session/user")
        {
            SessionToken = sessionToken
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
        var uuid = (string?)data["uuid"];
        var username = (string?)data["username"];
        var profilePicture = (string?)data["profile_picture"];
        var email = (string?)data["email"];
        var access = (bool?)data["access"];
        if(uuid == null) throw new MissingFieldException("The response from the server has one missing important property: uuid");
        if(username == null) throw new MissingFieldException("The response from the server has one missing important property: username");
        if(email == null) throw new MissingFieldException("The response from the server has one missing important property: email");
        if(access == null) throw new MissingFieldException("The response from the server has one missing important property: access");

        return new User(uuid, username, profilePicture, email, (bool)access);
    }

    /// <summary>
    /// Returns the profile picture of the current user as an Image object.
    /// </summary>
    /// <returns>Image</returns>
    /// <exception cref="NullReferenceException">Occurs when the user doesn't have a profile picture.</exception>
    /// <exception cref="Exception"></exception>
    public async Task<Image> GetProfilePictureAsync()
    {
        if (ProfilePicture == null) throw new NullReferenceException("This user has no profile picture.");
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(ProfilePicture);
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to get user's profile picture: {response.StatusCode}");
        var bytes = await response.Content.ReadAsByteArrayAsync();
        using var ms = new MemoryStream(bytes);
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) throw new Exception("This function is only available on a Windows operating system, use GetProfilePictureBytesAsync instead.");
        var image = Image.FromStream(ms);
        return image;
    }

    /// <summary>
    /// Returns the profile picture of the current user in bytes.
    /// </summary>
    /// <returns>byte[]</returns>
    /// <exception cref="NullReferenceException">Occurs when the user doesn't have a profile picture.</exception>
    /// <exception cref="Exception"></exception>
    public async Task<byte[]> GetProfilePictureBytesAsync()
    {
        if (ProfilePicture == null) throw new NullReferenceException("This user has no profile picture.");
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(ProfilePicture);
        if (response.StatusCode != HttpStatusCode.OK)
            throw new Exception($"Failed to get user's profile picture: {response.StatusCode}");
        return await response.Content.ReadAsByteArrayAsync();
    }
}