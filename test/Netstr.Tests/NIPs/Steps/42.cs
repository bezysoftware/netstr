using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Netstr.Messaging.Models;
using Netstr.Options;
using TechTalk.SpecFlow;

namespace Netstr.Tests.NIPs.Steps
{
    public partial class Steps
    {
        [Given(@"a relay is running with AUTH required")]
        public void GivenARelayIsRunningWithAUTHRequired()
        {
            this.factory.AuthMode = AuthMode.Always;
        }

        [When(@"(.*) publishes an AUTH event with invalid challenge")]
        public async Task WhenAlicePublishesAnAUTHEventWithInvalidChallenge(string client)
        {
            var c = this.scenarioContext.Get<Clients>()[client];
            
            var e = new Event
            {
                Id = "",
                Signature = "",
                Content = "",
                CreatedAt = DateTimeOffset.UtcNow,
                PublicKey = c.Keys.PublicKey,
                Tags = [
                    ["relay", "ws://localhost"],
                    ["challenge", "invalid"]
                ],
                Kind = EventKind.Auth
            };

            e = Helpers.FinalizeEvent(e, c.Keys.PrivateKey);

            await c.WebSocket.SendAuthAsync(e);
        }

        [When(@"(.*) publishes an AUTH event for the challenge sent by relay")]
        public async Task WhenAlicePublishesAnAUTHEventForTheChallengeSentByRelay(string client)
        {
            var c = this.scenarioContext.Get<Clients>()[client];
            
            await c.WaitForMessageAsync(DateTimeOffset.UtcNow.AddSeconds(-5), ["AUTH"]);
            
            var auth = c.GetReceivedMessages().First(x => x[0].Equals(MessageType.Auth));
            var e = new Event {
                Id = "",
                Signature = "",
                Content = "",
                CreatedAt = DateTimeOffset.UtcNow,
                PublicKey = c.Keys.PublicKey,
                Tags = [
                    ["relay", "ws://localhost"],
                    ["challenge", auth[1].ToString() ?? ""]
                ],
                Kind = EventKind.Auth
            };

            e = Helpers.FinalizeEvent(e, c.Keys.PrivateKey);

            await c.WebSocket.SendAuthAsync(e);
        }
    }
}
