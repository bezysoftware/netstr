﻿using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Netstr.Data;
using Netstr.Messaging.Models;
using Netstr.Options;
using Netstr.Tests.NIPs;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Netstr.Tests.Subscriptions
{
    public class SubscriptionTests
    {
        private readonly WebApplicationFactory factory;

        public SubscriptionTests()
        {
            this.factory = new WebApplicationFactory();
        }

        [Fact]
        public async Task UnknownFilterTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var sub = new { unknown = "unknown" };

            await ws.SendAsync([ "REQ", "id", sub ]);
            
            var result = await ws.ReceiveOnceAsync();

            result[0].GetString().Should().Be("CLOSED");
        }

        [Fact]
        public async Task UnknownFilterTagTest()
        {
            using WebSocket ws = await this.factory.ConnectWebSocketAsync();

            var sub = @"[ ""REQ"", ""id"", { ""#abc"": [] }]";

            await ws.SendAsync(Encoding.UTF8.GetBytes(sub), WebSocketMessageType.Text, true, CancellationToken.None);

            var result = await ws.ReceiveOnceAsync();

            result[0].GetString().Should().Be("CLOSED");
        }
    }
}
