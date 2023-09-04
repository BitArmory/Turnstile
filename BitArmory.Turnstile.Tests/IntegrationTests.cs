using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace BitArmory.Turnstile.Tests
{
   [Explicit]
   [TestFixture]
   public class IntegrationTests
   {
      [Test]
      public async Task can_make_process_bad_request()
      {
         var service = new TurnstileService();
         service.EnableFiddlerDebugProxy("http://localhost.:8888");

         var response = await service.VerifyAsync("ffff", "bbbb");

         response.IsSuccess.Should().BeFalse();
         response.ErrorCodes[0].Should().Be("invalid-input-secret");
      }


      [Test]
      public async Task can_make_good_request()
      {
         var service = new TurnstileService();
         service.EnableFiddlerDebugProxy("http://localhost.:8888");

         var response = await service.VerifyAsync("ffff", "1x0000000000000000000000000000000AA");

         response.IsSuccess.Should().Be(true);
         response.ErrorCodes.Length.Should().Be(0);
         response.ChallengeTs.Should().NotBeNullOrEmpty();
         response.HostName.Should().Be("example.com");
      }

      [Test]
      public async Task can_make_good_request_with_idempotency()
      {
         var service = new TurnstileService();
         service.EnableFiddlerDebugProxy("http://localhost.:8888");

         var idempotencyKey = "E446D876-990F-43F6-ACAC-B3BEF8CF6803";

         var response = await service.VerifyAsync("ffff", "1x0000000000000000000000000000000AA", idempotencyKey: idempotencyKey);

         response.IsSuccess.Should().Be(true);
         response.ErrorCodes.Length.Should().Be(0);
         response.ChallengeTs.Should().NotBeNullOrEmpty();
         response.HostName.Should().Be("example.com");
      }

      [Test]
      public async Task can_make_good_request_with_remoteip()
      {
         var service = new TurnstileService();
         service.EnableFiddlerDebugProxy("http://localhost.:8888");

         var response = await service.VerifyAsync("ffff", "1x0000000000000000000000000000000AA", remoteIp: "127.0.0.1");

         response.IsSuccess.Should().Be(true);
         response.ErrorCodes.Length.Should().Be(0);
         response.ChallengeTs.Should().NotBeNullOrEmpty();
         response.HostName.Should().Be("example.com");
      }

   }
}
