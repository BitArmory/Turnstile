namespace BitArmory.Turnstile
{
   /// <summary>
   /// Default constants.
   /// </summary>
   public static class Constants
   {
      /// <summary>
      /// Default URL for verifying Turnstile.
      /// </summary>
      public const string VerifyUrl = "https://challenges.cloudflare.com/turnstile/v0/siteverify";

      /// <summary>
      /// Default URL for Turnstile JavaScript
      /// </summary>
      public const string JavaScriptUrl = "https://challenges.cloudflare.com/turnstile/v0/api.js";

      /// <summary>  
      /// Default HTTP header key for Turnstile response.
      /// </summary>
      public const string ClientResponseKey = "cf-turnstile-response";
   }
}
