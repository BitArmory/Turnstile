[![Build status](https://ci.appveyor.com/api/projects/status/sng4ogpbuhu3fc9e/branch/master?svg=true)](https://ci.appveyor.com/project/BitArmory/Turnstile/branch/master) [![Nuget](https://img.shields.io/nuget/v/BitArmory.Turnstile.svg)](https://www.nuget.org/packages/BitArmory.Turnstile/) [![Users](https://img.shields.io/nuget/dt/BitArmory.Turnstile.svg)](https://www.nuget.org/packages/BitArmory.Turnstile/) <img src="https://raw.githubusercontent.com/BitArmory/Turnstile/master/docs/turnstile.png" align='right' />

BitArmory.Turnstile for .NET and C#
===================================

Project Description
-------------------
A minimal, no-drama, friction-less **C#** **HTTP** verification client for **Cloudflare**'s [**Turnstile** API](https://developers.cloudflare.com/turnstile/).

The problem with current **Turnstile** libraries in **.NET** is that all of them take a hard dependency on the underlying web framework like **ASP.NET WebForms**, **ASP.NET MVC 5**, **ASP.NET Core**, or **ASP.NET Razor Pages**. 

Furthermore, current **Turnstile** libraries for **.NET** are hard coded against the `HttpContext.Request` to retrieve the remote IP address of the visitor. Unfortunately, this method doesn't work if your website is behind a service like **Cloudflare** where the [`CF-Connecting-IP` header value](https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers) is the ***real*** IP address of the visitor on your site.

**BitArmory.Turnstile** is a minimal library that works across all **.NET** web frameworks without taking a hard dependency on any web framework. If you want to leverage platform specific features, like **MVC** ***Action Filters***, you'll need to implement your own `ActionFilter` that leverages the functionality in this library.

#### Supported Platforms
* **.NET Standard 1.3** or later
* **.NET Framework 4.5** or later

#### Supported Turnstile Widgets
* **Managed**
* **Non-interactive**
* **Invisible**


#### Crypto Tip Jar
<a href="https://commerce.coinbase.com/checkout/52bb1ce7-58c2-4df3-8ac7-58b5289c99d0"><img src="https://raw.githubusercontent.com/BitArmory/Turnstile/master/docs/tipjar.png" /></a>
* :dog2: **Dogecoin**: `DGVC2drEMt41sEzEHSsiE3VTrgsQxGn5qe`


### Download & Install
**NuGet Package [BitArmory.Turnstile](https://www.nuget.org/packages/BitArmory.Turnstile/)**

```powershell
Install-Package BitArmory.Turnstile
```


General Usage
-------------
### Getting Started
You'll need a **Cloudflare** account. Get started by following the directions [here](https://developers.cloudflare.com/turnstile/get-started/)! After you sign up and you're signed into the dashboard; you'll need two important pieces of information:
1. Your `site` key
2. Your `secret` key

This library supports all widget types: 
* **Managed**
* **Non-interactive**
* **Invisible**


## Turnstile
### Client-side Setup

Be sure to [read the documentation](https://developers.cloudflare.com/turnstile/) before implementing.

Then, to protect an **HTML** form submission on your website, add the following:
```html
<html>
  <head>
    <script src="https://challenges.cloudflare.com/turnstile/v0/api.js" async defer></script>
  </head>
  <body>
    <form method='POST' action='/my-form-post-endpoint'>
        ...
        <div class="cf-turnstile" data-sitekey="your_site_key"></div>
        <input type="submit" value="Submit">
    </form>
  </body>
</html>
```
When the user visits the **HTML** form, a hidden form field `cf-turnstile-response` will be added to the **HTML** form above. The `cf-turnstile-response` represents a token of the captcha challenge result and will need to be verified server-side when the **HTML** form is posted to your server.

### Verifying the POST Server-side
When the **HTML** form `POST` is received on the server:
1. Get the client's IP address. If you're using **Cloudflare**, be sure to use the [`CF-Connecting-IP` header value][0].
2. Extract the hidden form field `cf-turnstile-response` from the browser's form POST submission.
3. Use the `TurnstileService` to verify that the client's **Turnstile** `cf-turnstile-response` challenge is valid.

The following example shows how to verify the captcha during an **HTTP** form `POST` back in **ASP.NET Core: Razor Pages**.

```csharp
//1. Get the client IP address in your chosen web framework
string secret = "your_secret_key";
string clientIp = GetClientIpAddress();
string browserChallengeToken = null;

//2. Extract the `cf-turnstile-response` hidden field from the HTML form in your chosen web framework
//Tip: You can also use Constants.ClientResponseFormKey instead of the magic string below
if( this.Request.Form.TryGetValue("cf-turnstile-response", out var hiddenFormField) )
{
   browserChallengeToken = hiddenFormField;
}

//3. Validate the Turnstile challenge with Cloudflare
var turnstileApi = new TurnstileService();
var verifyResult = await turnstileApi.VerifyAsync(browserChallengeToken, secret, clientIp);
if( verifyResult.IsSuccess is false )
{
   this.ModelState.AddModelError("captcha", "The Cloudflare challenge is not valid.");
   return new BadRequestResult();
}
else{
   //continue processing, everything is okay!
}
```

**Notes:**
* The `TurnstileService.VerifyAsync` supports an optional idempotency parameter; you can read more about that [here](https://developers.cloudflare.com/turnstile/get-started/server-side-validation/#accepted-parameters).
* The `clientIp` is technically optional but providing the client's IP address prevents abuses by ensuring that the current HTTP request is the one that received the token.

<details><summary><b>GetClientIpAddress() in ASP.NET Core</b></summary>
<p>

**Note:** If your site is behind Cloudflare, be sure you're using the [`CF-Connecting-IP` header value][0] instead.

```csharp
public string GetClientIpAddress(){
   return this.HttpContext.Connection.RemoteIpAddress.ToString();
}
```
</p>
</details>

<details><summary><b>GetClientIpAddress() in ASP.NET WebForms</b></summary>
<p>

**Note:** If your site is behind Cloudflare, be sure you're using the [`CF-Connecting-IP` header value][0] instead.

```csharp
public string GetClientIpAddress(){
   return this.Request.UserHostAddress;
}
```
</p>
</details> 

That's it! **Happy verifying!** :tada:

Testing and Development Environment
--------
**Cloudflare Turnstile** has some convenient testing keys and secrets that respond/behave differently for various UI interactions and server-side verification responses to help test your website. Be sure to read the following testing documentation [here](https://developers.cloudflare.com/turnstile/reference/testing/).

The testing keys and secrets can be used from the `TestingSiteKeys` and `TestingSecretKeys` constant classes:

https://github.com/BitArmory/Turnstile/blob/1d59d2696526a7fafaacb55dcdac918bb1c895e5/BitArmory.Turnstile/Constants.cs#L30-L73

Building
--------
* Download the source code.
* Run `build.cmd`.

Upon successful build, the results will be in the `\__compile` directory. If you want to build NuGet packages, run `build.cmd pack` and the NuGet packages will be in `__package`.



[0]:https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers
