[![Build status](https://ci.appveyor.com/api/projects/status/sng4ogpbuhu3fc9e/branch/master?svg=true)](https://ci.appveyor.com/project/BitArmory/Turnstile/branch/master) [![Nuget](https://img.shields.io/nuget/v/BitArmory.Turnstile.svg)](https://www.nuget.org/packages/BitArmory.Turnstile/) [![Users](https://img.shields.io/nuget/dt/BitArmory.Turnstile.svg)](https://www.nuget.org/packages/BitArmory.Turnstile/) <img src="https://raw.githubusercontent.com/BitArmory/Turnstile/master/docs/turnstile.png" align='right' />

BitArmory.Turnstile for .NET and C#
===================================

Project Description
-------------------
:recycle: A minimal, no-drama, friction-less **C#** **HTTP** verification client for **Cloudflare**'s [**Turnstile** API](https://developers.cloudflare.com/turnstile/).

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
You'll need a **Cloudflare** account. You can sign up [here](https://www.cloudflare.com/)! After you sign up and setup your domain, you'll have two important pieces of information:
1. Your `site` key
2. Your `secret` key

This library supports all widget types: 
* **Managed**
* **Non-interactive**
* **Invisible**


## Turnstile
### Client-side Setup

Be sure to checkout [this video that describes how Turnstile v3 works](https://www.youtube.com/watch?v=tbvxFW4UJdU) before implementing.

Then, on every page of your website, add the following JavaScript:
```html
<html>
  <head>
    <script src='https://www.google.com/Turnstile/api.js?render=GOOGLE_SITE_KEY'></script>
  </head>
  <body>
    ...
    <script>
        gTurnstile.ready(function() {
          gTurnstile.execute('GOOGLE_SITE_KEY', {action: 'TAG'});
        });
    </script>
  </body>
</html>
```
Every page should call `gTurnstile.execute` with some unique **action** `TAG`. [Read more about actions in the official docs here](https://developers.google.com/Turnstile/docs/v3#actions).

When it is time to validate an **HTTP** `POST` you'll need transfer the captcha `token` in the browser to a hidden HTML form field as shown below:

```html
<html>
  <body>
    <form action='/do-post' method='POST'>
      <input id="captcha" type="hidden" name="captcha" value="" />
    </form>
    <script>
      function ExecuteTurnstile_OnSome_ButtonAction(){
        gTurnstile.ready(function() {
          gTurnstile.execute('GOOGLE_SITE_KEY', {action: 'SomeAction'})
            .then(function(token) {
               // Set `token` in a hidden form input.
               $("#captcha").val(token);
               
               //And finally submit the form by firing
               //off the HTTP POST over the wire at this
               //exact moment in time here.
            });
        });
      }
    </script>
  </body>
</html>
```
You'll need to execute `ExecuteTurnstile_OnSome_ButtonAction()` function the moment the user decides to submit your form. Otherwise, if you run `gTurnstile.*` code during page load, the token being copied to the hidden field can expire after a few minutes. This means, if the user takes a long time filling out a form, the token copied at page load can expire and your server will validate an expired token by the time the form is submitted resulting in a failed captcha verification.

Therefore, you should execute the `ExecuteTurnstile_OnSome_ButtonAction()` function on some `onclick=` event to get a fresh token before the form is submitted.

Also, keep in mind, `gTurnstile.execute()` returns a **JavaScript Promise**. You won't have a valid token in your `<form>` until the line `$("#captcha").val(token);` above executes. So you'll need to postpone the form submission until `$("#captcha").val(token);` is actually executed. Then, *and only then,* you can continue submitting the HTML form to have it validated on your server with a valid token. 

### Verifying the POST Server-side
When the `POST` is received on the server:
1. Get the client's IP address. If you're using **CloudFlare**, be sure to use the [`CF-Connecting-IP` header value][0].
2. Extract the `#captcha` value (client token) in the hidden **HTML** form field.
3. Use the `TurnstileService` to verify the client's **Turnstile** is valid.

```csharp
//1. Get the client IP address in your chosen web framework
string clientIp = GetClientIpAddress();
string token = null;
string secret = "your_secret_key";

//2. Extract the `#captcha` field from the hidden HTML form in your chosen web framework
if( this.Request.Form.TryGetValue("captcha", out var formField) )
{
   token = formField;
}

//3. Validate the Turnstile with Google
var captchaApi = new TurnstileService();
var result = await captchaApi.Verify3Async(token, clientIp, secret);

if( !result.IsSuccess || result.Action != "SOME_ACTION" || result.Score < 0.5 )
{
   // The POST is not valid
   return new BadRequestResult();
}
else{
   //continue processing, everything is okay!
}
```

<details><summary>GetClientIpAddress() in ASP.NET Core</summary>
<p>

**Note:** If your site is behind CloudFlare, be sure you're suing the [`CF-Connecting-IP` header value][0] instead.

```csharp
public string GetClientIpAddress(){
   return this.HttpContext.Connection.RemoteIpAddress.ToString();
}
```

</p>
</details>

<details><summary>GetClientIpAddress() in ASP.NET WebForms</summary>
<p>

**Note:** If your site is behind CloudFlare, be sure you're suing the [`CF-Connecting-IP` header value][0] instead.

```csharp
public string GetClientIpAddress(){
   return this.Request.UserHostAddress;
}
```

</p>
</details>         

You'll want to make sure the action name you choose for the request is legitimate. The `result.Score` is the probably of a human. So, you'll want to make sure you have a `result.Score > 0.5`; anything less is probably a bot.

## Turnstile v2 (I'm not a robot)
### Client-side Setup
Add the following `<div class="g-Turnstile">` and `<script>` tags to your **HTML** form:
```html
<html>
  <body>
    <form method="POST">
        ...
        <div class="g-Turnstile" data-sitekey="your_site_key"></div>
        <input type="submit" value="Submit">
    </form>

    <script src="https://www.google.com/Turnstile/api.js" async defer></script>
  </body>
</html>
```


### Verifying the POST Server-side
When the `POST` is received on the server:
1. Get the client's IP address. If you're using **CloudFlare**, be sure to use the [`CF-Connecting-IP` header value][0].
2. Extract the `g-Turnstile-response` (Client Response) **HTML** form field.
3. Use the `TurnstileService` to verify the client's **Turnstile** is valid.

The following example shows how to verify the captcha during an **HTTP** `POST` back in **ASP.NET Core: Razor Pages**.

```csharp
//1. Get the client IP address in your chosen web framework
string clientIp = GetClientIpAddress();
string captchaResponse = null;
string secret = "your_secret_key";

//2. Extract the `g-Turnstile-response` field from the HTML form in your chosen web framework
if( this.Request.Form.TryGetValue(Constants.ClientResponseKey, out var formField) )
{
   capthcaResponse = formField;
}

//3. Validate the Turnstile with Google
var captchaApi = new TurnstileService();
var isValid = await captchaApi.Verify2Async(capthcaResponse, clientIp, secret);
if( !isValid )
{
   this.ModelState.AddModelError("captcha", "The Turnstile is not valid.");
   return new BadRequestResult();
}
else{
   //continue processing, everything is okay!
}
```

<details><summary>GetClientIpAddress() in ASP.NET Core</summary>
<p>

**Note:** If your site is behind CloudFlare, be sure you're suing the [`CF-Connecting-IP` header value][0] instead.

```csharp
public string GetClientIpAddress(){
   return this.HttpContext.Connection.RemoteIpAddress.ToString();
}
```

</p>
</details>

<details><summary>GetClientIpAddress() in ASP.NET WebForms</summary>
<p>

**Note:** If your site is behind CloudFlare, be sure you're suing the [`CF-Connecting-IP` header value][0] instead.

```csharp
public string GetClientIpAddress(){
   return this.Request.UserHostAddress;
}
```

</p>
</details>    

That's it! **Happy verifying!** :tada:


Building
--------
* Download the source code.
* Run `build.cmd`.

Upon successful build, the results will be in the `\__compile` directory. If you want to build NuGet packages, run `build.cmd pack` and the NuGet packages will be in `__package`.



[0]:https://support.cloudflare.com/hc/en-us/articles/200170986-How-does-Cloudflare-handle-HTTP-Request-headers
