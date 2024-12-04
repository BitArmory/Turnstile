using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BitArmory.Turnstile.Utils;


namespace BitArmory.Turnstile
{
   /// <summary>
   /// Service for validating Turnstile.
   /// </summary>
   public class TurnstileService : IDisposable
   {
      /// <summary>
      /// The underlying HTTP client used to make the request.
      /// </summary>
      public HttpClient HttpClient { get; set; }

      /// <summary>
      /// The default HTTP client to use for new instances of the <see cref="TurnstileService"/>.
      /// </summary>
      public static HttpClient DefaultHttpClient { get; set; } = new HttpClient();

      private readonly string verifyUrl;

      /// <summary>
      /// Create a new service instance. This should be a singleton if you're using an IoC container.
      /// </summary>
      public TurnstileService() : this(Constants.VerifyUrl, DefaultHttpClient)
      {
      }

      /// <summary>
      /// Create a new service instance. This should be a singleton if you're using an IoC container.
      /// </summary>
      /// <param name="verifyUrl">Manually override the Google verify URL endpoint. If null, <seealso cref="Constants.VerifyUrl" /> is used.</param>
      /// <param name="client">The HttpClient to use for this instance. If null, the global static <seealso cref="TurnstileService.DefaultHttpClient"/> is used.</param>
      public TurnstileService(string verifyUrl = null, HttpClient client = null)
      {
         this.verifyUrl = verifyUrl ?? Constants.VerifyUrl;
         this.HttpClient = client ?? DefaultHttpClient;
      }

      /// <summary>
      /// Validate Turnstile <paramref name="clientToken"/> using your secret.
      /// </summary>
      /// <param name="clientToken">Required. The user response token provided by the Turnstile client-side integration on your site. The <seealso cref="Constants.ClientResponseFormKey"/> value pulled from the hidden HTML Form field.</param>
      /// <param name="siteSecret">Required. The server-side secret: v2 secret, invisible secret, or android secret. The shared key between your site and Turnstile.</param>
      /// <param name="remoteIp">Optional. The remote IP of the client</param>
      /// <param name="idempotencyKey">Optional. The optional UUID to be associated with the response. 
      /// Normally, a response may only be validated once. That is, if the same response is presented twice, the second and each subsequent
      /// request will generate an error stating that the response has already been consumed. If an application requires 
      /// to retry failed requests, it must utilize the idempotency functionality. You can do so by providing a UUID as
      /// the idempotencyKey parameter when initially validating the response and the same UUID 
      /// with any subsequent request for that response.</param>
      /// <param name="cancellationToken">Optional. Async cancellation token.</param>
      public virtual async Task<TurnstileResponse> VerifyAsync(string clientToken, string siteSecret, string remoteIp = null, string idempotencyKey = null, CancellationToken cancellationToken = default)
      {
         if( string.IsNullOrWhiteSpace(siteSecret) ) throw new ArgumentException("The secret must not be null or empty", nameof(siteSecret));
         if( string.IsNullOrWhiteSpace(clientToken) ) throw new ArgumentException("The client response must not be null or empty", nameof(clientToken));
         if( remoteIp is not null && !remoteIp.Contains(".") && !remoteIp.Contains(":") )
         {
            throw new ArgumentException("The remote ip parameter must be formatted in IPv4 '.' or IPv6 ':' syntax.", nameof(remoteIp));
         }


         var form = PrepareRequestBody(clientToken, siteSecret, remoteIp, idempotencyKey);

         var response = await this.HttpClient.PostAsync(verifyUrl, form, cancellationToken)
            .ConfigureAwait(false);

         if( !response.IsSuccessStatusCode ) return new TurnstileResponse {IsSuccess = false};

         var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

         var model = Json.Parse(json);

         var result = new TurnstileResponse();

         foreach( var kv in model )
         {
            switch( kv.Key )
            {
               case "success":
                  result.IsSuccess = kv.Value;
                  break;
               case "challenge_ts":
                  result.ChallengeTs = kv.Value;
                  break;
               case "hostname":
                  result.HostName = kv.Value;
                  break;
               case "cdata":
                  result.CData = kv.Value;
                  break;
               case "action":
                   result.Action = kv.Value;
                   break;
               case "error-codes" when kv.Value is JsonArray errors:
               {
                  result.ErrorCodes = errors.Children
                     .Select(n => (string)n)
                     .ToArray();

                  break;
               }
               default:
                  result.ExtraJson.Add(kv.Key, kv.Value);
                  break;
            }
         }

         return result;
      }

      /// <summary>
      /// Method called when the request body needs to be prepared.
      /// </summary>
      /// <param name="clientResponse">The client response</param>
      /// <param name="secret"></param>
      /// <param name="remoteIp"></param>
      /// <returns></returns>
      protected FormUrlEncodedContent PrepareRequestBody(string clientResponse, string secret, string remoteIp, string idempotencyKey)
      {
         var form = new List<KeyValuePair<string, string>>()
            {
               new KeyValuePair<string, string>("response", clientResponse),
               new KeyValuePair<string, string>("secret", secret)
            };

         if( !string.IsNullOrWhiteSpace(remoteIp) )
         {
            form.Add(new KeyValuePair<string, string>("remoteip", remoteIp));
         }
         if( !string.IsNullOrWhiteSpace(idempotencyKey) )
         {
            form.Add(new KeyValuePair<string, string>("idempotency_key", idempotencyKey));
         }

         return new FormUrlEncodedContent(form);
      }

      /// <summary>
      /// Disposes the local <seealso cref="HttpClient"/>.
      /// Note: If the global static <seealso cref="TurnstileService.DefaultHttpClient"/> was used in this instance, this .Dispose() method will not dispose the global static HttpClient.
      /// </summary>
      public void Dispose()
      {
         if( this.HttpClient != DefaultHttpClient )
         {
            this.HttpClient.Dispose();
         }
      }

#if !STANDARD13
      /// <summary>
      /// Enable HTTP debugging via Fiddler. Ensure Tools > Fiddler Options... > Connections is enabled and has a port configured.
      /// Then, call this method with the following URL format: http://localhost.:PORT where PORT is the port number Fiddler proxy
      /// is listening on. (Be sure to include the period after the localhost).
      /// Note, calling this method will replace the object in <seealso cref="HttpClient"/> property.
      /// with a correctly configured HttpClient for proxy usage.
      /// </summary>
      /// <param name="proxyUrl">The full proxy URL Fiddler proxy is listening on. IE: http://localhost.:8888 - The period after localhost is important to include.</param>
      public void EnableFiddlerDebugProxy(string proxyUrl)
      {
         var webProxy = new WebProxy(proxyUrl, BypassOnLocal: false);
         var handler = new HttpClientHandler {Proxy = webProxy};
         this.HttpClient = new HttpClient(handler, true);
      }
#endif
   }
}
