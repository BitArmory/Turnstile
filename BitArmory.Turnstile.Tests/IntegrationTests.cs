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

         var response = await service.VerifyAsync("ffff", "127.0.0.1", "bbbb");

         response.IsSuccess.Should().BeFalse();
         response.ErrorCodes[0].Should().Be("invalid-input-secret");
      }


      [Test]
      public async Task can_make_good_request()
      {
         var service = new TurnstileService();
         service.EnableFiddlerDebugProxy("http://localhost.:8888");

         var response = await service.VerifyAsync("ffff", "127.0.0.1", "1x0000000000000000000000000000000AA");

         response.IsSuccess.Should().Be(true);
         response.ErrorCodes.Length.Should().Be(0);
         response.ChallengeTs.Should().NotBeNullOrEmpty();
         response.HostName.Should().Be("example.com");
      }
   }
}
