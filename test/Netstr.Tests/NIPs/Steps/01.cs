using FluentAssertions;
using Netstr.Messaging.Models;
using System.Text.Json;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Netstr.Tests.NIPs.Steps
{
    public partial class Steps
    {
        [When(@"(.*) sends a subscription request (.*)")]
        public async Task WhenAliceSubscribesToEvents(string client, string subscriptionId, IEnumerable<SubscriptionFilterRequest> filters)
        {
            var now = DateTimeOffset.UtcNow;
            var c = this.scenarioContext.Get<Clients>()[client];

            await c.WebSocket.SendReqAsync(subscriptionId, filters);
            await c.WaitForMessageAsync(now, ["EOSE", subscriptionId], ["CLOSED", subscriptionId]);
        }

        [When(@"(.*) publishes an event")]
        [When(@"(.*) publishes events")]
        public async Task WhenBobPublishesAnEvent(string client, Table table)
        {
            var start = DateTimeOffset.UtcNow;
            var c = this.scenarioContext.Get<Clients>()[client];
            var events = Transforms.CreateEvents(table, c);

            foreach (var e in events)
            {
                await c.WebSocket.SendEventAsync(e);
            }

            foreach (var e in events)
            {
                await c.WaitForMessageAsync(start, ["OK", e.Id]);
            }
        }

        [When(@"(.*) closes a subscription (.*)")]
        public async Task WhenAliceClosesASubscriptionAbcd(string client, string subscriptionId)
        {
            var c = this.scenarioContext.Get<Clients>()[client];

            await c.WebSocket.SendCloseAsync(subscriptionId);
            await Task.Delay(500);
        }

        [Then(@"(.*) receives a message")]
        [Then(@"(.*) receives messages")]
        public Task ThenBobReceivesAReply(string client, IEnumerable<object[]> messages)
        {
            return Helpers.VerifyWithDelayAsync(() =>
            {
                var received = this.scenarioContext.Get<Clients>()[client].GetReceivedMessages();
                received.Should().BeEquivalentTo(messages, opts => opts
                    .WithStrictOrdering()
                    .Using<string>(x => x.Expectation.Should().Match(e => e == "*" || e == x.Subject)).WhenTypeIs<string>());
            });
        }
    }
}
