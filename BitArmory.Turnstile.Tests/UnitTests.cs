using System;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using RichardSzalay.MockHttp;
using static FluentAssertions.FluentActions;

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
            .WithExactFormData("response=aaa&secret=sss")
            .Respond("application/json", ErrorJson);
            
         var captcha = new TurnstileService(client: mockHttp.ToHttpClient());

         var response = await captcha.VerifyAsync("aaa", "sss");

         response.IsSuccess.Should().BeFalse();
         response.ErrorCodes.Should().BeEquivalentTo("invalid-input-secret");

         mockHttp.VerifyNoOutstandingExpectation();
      }

      [Test]
      public async Task can_parse_good_response()
      {
         var mockHttp = new MockHttpMessageHandler();

         mockHttp.Expect(HttpMethod.Post, Constants.VerifyUrl)
            .WithExactFormData("response=aaa&secret=sss")
            .Respond("application/json", GoodJson);

         var captcha = new TurnstileService(client: mockHttp.ToHttpClient());

         var response = await captcha.VerifyAsync("aaa", "sss");

         response.IsSuccess.Should().BeTrue();
         response.ErrorCodes.Should().BeEmpty();
         response.ChallengeTs.Should().Be("2023-09-01T03:09:21.688Z");
         response.HostName.Should().Be("example.com");

         mockHttp.VerifyNoOutstandingExpectation();
      }

      [Test]
      public async Task invalid_ip_format_throws_argumentexception()
      {
         var mockHttp = new MockHttpMessageHandler();

         var captcha = new TurnstileService(client: mockHttp.ToHttpClient());

         await Awaiting(() => captcha.VerifyAsync("aaa", "sss", "rrr"))
           .Should().ThrowExactlyAsync<ArgumentException>().WithParameterName("remoteIp");

         mockHttp.GetMatchCount(mockHttp.Fallback).Should().Be(0);
      }

      [Test]
      public async Task ipv4_format_is_okay()
      {
         var mockHttp = new MockHttpMessageHandler();

         mockHttp.Expect(HttpMethod.Post, Constants.VerifyUrl)
            .WithExactFormData("response=aaa&secret=ccc&remoteip=127.0.0.1")
            .Respond("application/json", GoodJson);

         var captcha = new TurnstileService(client: mockHttp.ToHttpClient());

         await Awaiting(() => captcha.VerifyAsync("aaa", "ccc", "127.0.0.1"))
                    .Should().NotThrowAsync();

         mockHttp.VerifyNoOutstandingExpectation();
      }

      [Test]
      public async Task ipv6_format_is_okay()
      {
         var mockHttp = new MockHttpMessageHandler();

         mockHttp.Expect(HttpMethod.Post, Constants.VerifyUrl)
            .WithExactFormData("response=aaa&secret=ccc&remoteip=127:0:0:1")
            .Respond("application/json", GoodJson);

         var captcha = new TurnstileService(client: mockHttp.ToHttpClient());

         await Awaiting(() => captcha.VerifyAsync("aaa", "ccc", "127:0:0:1"))
                    .Should().NotThrowAsync();

         mockHttp.VerifyNoOutstandingExpectation();
      }
   }
}
