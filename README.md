
# CodeOffer .NET Library

This is a .NET library that provides an API wrapper for the public CodeOffer API. The package simplifies authentication and management of in-app assets for developers who use the CodeOffer API in their applications.

## Installation

First install the Library from the NuGet Package manager.

## Usage

### Authentication & Sessions

Initialize a new session, but first you need to import the `oauth` class from.

```csharp
using CodeOffer.Library.OAuth;
```

```csharp
var session = new Session("APP_ID");
```

After that you can create a new session token:

```csharp
var token = await session.CreateSessionTokenAsync();
```

Now get the login link and ask the user to log in:
```csharp
Console.WriteLine("Please login.");  
token.OpenLoginLink();
```

Right after that call the `await token.WaitForConfirmationAsync();` task, this task will wait until the user completed the login-process.

Then you can return the current user by using the `GetUserAsync` method and passing the token as a parameter:

```csharp
var user = await User.GetUserAsync(token);
```

And now you can get the username, email, profile picture and access to the current logged in app (if the user purchased / downloaded the app with his account)

```csharp
Console.WriteLine($"Hello: {user.Username}, access to this App? {(user.Access ? "Yes" : "No")}");
```

### Assets

You can return all the apps your app contains.

First import the `App` class.

```csharp
using CodeOffer.Library.App;
```

After that you need to get the app by the valid session token.
```csharp
var app = App.BySessionToken(token);
```

Then you can return all the assets the app contains.
```csharp
var assets = await app.GetAssetDirectoryAsync();
```

#### Complete Example

```csharp
var session = new Session("10aa641e562bdd82d2f8449d");  
var token = await session.CreateSessionTokenAsync();  
Console.WriteLine("Please login.");  
token.OpenLoginLink();  
await token.WaitForConfirmationAsync();  
var user = await User.GetUserAsync(token);  
Console.WriteLine($"Hello: {user.Username}, access to this App? {(user.Access ? "Yes" : "No")}");  
var app = App.BySessionToken(token);  
var assets = await app.GetAssetDirectoryAsync();  
foreach (var asset in assets)  
{  
	Console.WriteLine($"Name: {asset.Name}, access: {(asset.Access ? "Yes" : "No")}");  
}
```

## License

This package is licensed under the Apache-2.0 License.
