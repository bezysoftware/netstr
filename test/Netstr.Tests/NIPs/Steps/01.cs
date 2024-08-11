﻿using FluentAssertions;
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

            await c.WebSocket.SendAsync([
                "REQ",
                subscriptionId,
                ..filters
            ]);

            await c.WaitForMessageAsync(now, ["EOSE", subscriptionId]);
        }

        [When(@"(.*) publishes an event")]
        [When(@"(.*) publishes events")]
        public async Task WhenBobPublishesAnEvent(string client, Table table)
        {
            var start = DateTimeOffset.UtcNow;
            var c = this.scenarioContext.Get<Clients>()[client];
            var events = table.CreateSet<Event>().Select((e, i) =>
            {
                var tags = table.Rows[i].GetString("Tags");
                return e with
                {
                    CreatedAt = DateTimeOffset.FromUnixTimeSeconds(table.Rows[i].GetInt64("CreatedAt")),
                    PublicKey = c.Keys.PublicKey,
                    Signature = e.Signature ?? Helpers.Sign(e.Id, c.Keys.PrivateKey),
                    Tags = string.IsNullOrWhiteSpace(tags)
                        ? []
                        : JsonSerializer.Deserialize<string[][]>(tags) ?? []
                };
            });

            foreach (var e in events)
            {
                await c.WebSocket.SendAsync([
                    "EVENT",
                    e
                ]);
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

            await c.WebSocket.SendAsync([
                "CLOSE",
                subscriptionId
            ]);

            await Task.Delay(500);
        }

        [Then(@"(.*) receives a message")]
        [Then(@"(.*) receives messages")]
        public Task ThenBobReceivesAReply(string client, IEnumerable<object[]> messages)
        {
            return Helpers.VerifyWithDelayAsync(() =>
            {
                var received = this.scenarioContext.Get<Clients>()[client].GetReceivedMessages();
                received.Should().BeEquivalentTo(messages, opts => opts.WithStrictOrdering());
            });
        }
    }
}