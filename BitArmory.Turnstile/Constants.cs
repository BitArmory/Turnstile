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


   /// <summary>
   /// The following sitekeys and secret keys are available for testing. 
   /// It is recommended that you use these keys in your development environment
   /// to ensure the challenges running in Turnstile do not conflict with your developer tools.
   /// Source: https://developers.cloudflare.com/turnstile/reference/testing/
   /// </summary>
   public static class TestingKeysAndSecrets
   {
      /// <summary>
      /// A site key for a visible captcha widget that always passes validation
      /// </summary>
      public const string SiteKeyAlwaysPassesVisible = "1x00000000000000000000AA";
      /// <summary>
      /// A site key for a visible captcha widget that always blocks validation
      /// </summary>
      public const string SiteKeyAlwaysBlocksVisible = "2x00000000000000000000AB";
      /// <summary>
      /// A site key for an invisible captcha widget that always passes validation
      /// </summary>
      public const string SiteKeyAlwaysPassesInvisible = "1x00000000000000000000BB";
      /// <summary>
      /// A site key for an invisible captcha widget that always blocks validation
      /// </summary>
      public const string SiteKeyAlwaysBlocksInvisible = "2x00000000000000000000BB";
      /// <summary>
      /// A site key for a visible captcha widget that forces an interactive challenge
      /// </summary>
      public const string SiteKeyForcesInteractiveChallengeVisible = "3x00000000000000000000FF";


      /// <summary>
      /// A site SECRET (on the server-side verify) that always passes.
      /// </summary>
      public const string SecretKeyAlwaysPasses = "1x0000000000000000000000000000000AA";
      /// <summary>
      /// A site SECRET (on the server-side verify) that always fails.
      /// </summary>
      public const string SecretKeyAlwaysFails = "2x0000000000000000000000000000000AA";
      /// <summary>
      /// A site SECRET (on the server-side verify) that always fails with 'token already spent' error.
      /// </summary>
      public const string SecretKeyTokenAlreadySpent = "3x0000000000000000000000000000000AA";
   }
}
