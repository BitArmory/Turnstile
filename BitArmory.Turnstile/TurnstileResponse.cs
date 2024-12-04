using System.Collections.Generic;

namespace BitArmory.Turnstile
{
   /// <summary>
   /// Base class for all JSON responses.
   /// </summary>
   public class JsonResponse
   {
      /// <summary>
      /// Extra data for/from the JSON serializer/deserializer to included with the object model.
      /// </summary>
      public IDictionary<string, object> ExtraJson { get; } = new Dictionary<string, object>();
   }
   
   /// <summary>
   /// Response from Turnstile verify URL.
   /// </summary>
   public class TurnstileResponse : JsonResponse
   {
      /// <summary>
      /// Whether this request was a valid Turnstile token for your site.
      /// </summary>
      public bool IsSuccess { get; set; }

      /// <summary>
      /// Timestamp of the challenge load (ISO format yyyy-MM-dd'T'HH:mm:ssZZ)
      /// </summary>
      public string ChallengeTs { get; set; }

      /// <summary>
      /// missing-input-secret: The secret parameter is missing.
      /// invalid-input-secret: The secret parameter is invalid or malformed.
      /// missing-input-response: The response parameter is missing.
      /// invalid-input-response: The response parameter is invalid or malformed.
      /// bad-request: The request is invalid or malformed.
      /// timeout-or-duplicate: The response is no longer valid: either is too old or has been used previously.
      /// </summary>
      public string[] ErrorCodes { get; set; }

      /// <summary>
      /// The hostname of the site where the Turnstile was solved
      /// </summary>
      public string HostName { get; set; }
      
       /// <summary>
       /// The customer widget identifier passed to the widget on the client side. This is used to differentiate
       /// widgets using the same sitekey in analytics. Its integrity is protected from modifications by an attacker.
       /// </summary>
      public string Action { get; set; }

      /// <summary>
      /// The customer data passed to the widget on the client side. This can be used by the customer
      /// to convey state. Its integrity is protected from modifications by an attacker.
      /// </summary>
      public string CData { get; set; }
   }

}
