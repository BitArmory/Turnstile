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
      /// Default form key for the Turnstile response.
      /// </summary>
      public const string ClientResponseFormKey = "cf-turnstile-response";
   }


   /// <summary>
   /// The following site keys are available for testing. These site keys are for use
   /// with client-side HTML rendering of the `class="cf-turnstile" data-sitekey=____` widget.
   /// Source: https://developers.cloudflare.com/turnstile/reference/testing/
   /// </summary>
   public static class TestingSiteKeys
   {
      /// <summary>
      /// A site key for a visible captcha widget that always passes validation
      /// </summary>
      public const string AlwaysPassesVisible = "1x00000000000000000000AA";
      /// <summary>
      /// A site key for a visible captcha widget that always blocks validation
      /// </summary>
      public const string AlwaysBlocksVisible = "2x00000000000000000000AB";
      /// <summary>
      /// A site key for an invisible captcha widget that always passes validation
      /// </summary>
      public const string AlwaysPassesInvisible = "1x00000000000000000000BB";
      /// <summary>
      /// A site key for an invisible captcha widget that always blocks validation
      /// </summary>
      public const string AlwaysBlocksInvisible = "2x00000000000000000000BB";
      /// <summary>
      /// A site key for a visible captcha widget that forces an interactive challenge
      /// </summary>
      public const string ForcesInteractiveChallengeVisible = "3x00000000000000000000FF";
   }

   /// <summary>
   /// The following secret keys are available for testing. These site secret keys are for use
   /// with the server-side 'siteSecret' parameter for <see cref="TurnstileService.VerifyAsync(string, string, string, System.Threading.CancellationToken)" />
   /// Source: https://developers.cloudflare.com/turnstile/reference/testing/
   /// </summary>
   public static class TestingSecretKeys
   {
      /// <summary>
      /// A site SECRET (on server-side verify) that always passes.
      /// </summary>
      public const string VerifyAlwaysPasses = "1x0000000000000000000000000000000AA";
      /// <summary>
      /// A site SECRET (on server-side verify) that always fails.
      /// </summary>
      public const string VerifyAlwaysFails = "2x0000000000000000000000000000000AA";
      /// <summary>
      /// A site SECRET (on server-side verify) that always fails with 'token already spent' error.
      /// </summary>
      public const string VerifyTokenAlreadySpentError = "3x0000000000000000000000000000000AA";
   }
}
