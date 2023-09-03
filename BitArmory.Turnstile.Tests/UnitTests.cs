using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace BitArmory.Turnstile.Tests
{
   [TestFixture]
   public class UnitTests
   {
      public const string ErrorJson = @"{
   ""success"":false,
   ""error-codes"":[""invalid-input-secret""],
   ""messages"":[]
}";

      public const string GoodJson = @"{
    ""success"":true,
    ""error-codes"":[],
    ""challenge_ts"":""2023-09-01T03:09:21.688Z"",
    ""hostname"":""example.com""
}";

      [Test]
      public async Task can_parse_errors()
      {
         var mockHttp = new MockHttpMessageHandler();
         
         mockHttp.Expect(HttpMethod.Post, Constants.VerifyUrl)
            .Respond("application/json", ErrorJson)
            .WithExactFormData("response=aaa&remoteip=bbb&secret=ccc");

         var captcha = new TurnstileService(client: mockHttp.ToHttpClient());

         var response = await captcha.VerifyAsync("aaa", "bbb", "ccc");

         response.IsSuccess.Should().BeFalse();
         response.ErrorCodes.Should().BeEquivalentTo("invalid-input-secret");
      }

      [Test]
      public async Task can_parse_good_response()
      {
         var mockHttp = new MockHttpMessageHandler();

         mockHttp.Expect(HttpMethod.Post, Constants.VerifyUrl)
            .Respond("application/json", GoodJson)
            .WithExactFormData("response=aaa&remoteip=bbb&secret=ccc");

         var captcha = new TurnstileService(client: mockHttp.ToHttpClient());

         var response = await captcha.VerifyAsync("aaa", "bbb", "ccc");

         response.IsSuccess.Should().BeTrue();
         response.ErrorCodes.Should().BeEmpty();
         response.ChallengeTs.Should().Be("2023-09-01T03:09:21.688Z");
         response.HostName.Should().Be("example.com");

      }
   }
}
