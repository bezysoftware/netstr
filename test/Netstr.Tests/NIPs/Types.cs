using Netstr.Extensions;
using Netstr.Messaging.Models;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Netstr.Tests.NIPs
{
    public class Clients : Dictionary<string, Client>
    {
    }

    public record Message(DateTimeOffset Received, object[] Data)
    {
    }

    public record Client(HttpClient http, WebSocket WebSocket, Keys Keys)
    {
        private const int WaitMessageAttempts = 5;
        private const int WaitMessageTimeoutMilis = 200;

        private List<Message> messages { get; } = new();
        private List<HttpResponseMessage> responses { get; } = new();

        public IEnumerable<object[]> GetReceivedMessages()
        {
            return this.messages.Select(x => x.Data);
        }

        public IEnumerable<HttpResponseMessage> GetHttpResponses()
        {
            return this.responses.AsEnumerable();
        }

        public void AddReceivedMessage(JsonElement[] message)
        {
            object[] msg = message[0].GetString() switch
            {
                MessageType.Event => [message[2].DeserializeRequired<EventId>().Id],
                MessageType.Ok => [message[2].GetBoolean(), ""],
                MessageType.Closed => [""],
                MessageType.Auth => [],
                MessageType.Count => [message[2].DeserializeRequired<CountValue>().Count],
                _ => []
            };

            this.messages.Add(new(DateTimeOffset.UtcNow, [message[0].ToString(), message[1].ToString(), ..msg]));
        }

        public void AddResponse(HttpResponseMessage response)
        {
            this.responses.Add(response);
        }

        public async Task WaitForMessageAsync(DateTimeOffset since, params string[][] values)
        {
            var i = WaitMessageAttempts;
            while (i-- >= 0)
            {
                foreach (var value in values)
                {
                    if (this.messages.Any(x => x.Received > since && x.Data.Take(value.Length).SequenceEqual(value))) return;
                }

                await Task.Delay(WaitMessageTimeoutMilis);
            }

            throw new Exception($"Message {string.Join(",", values.Select(x => string.Join("|", x)))} didn't arrive");
        }
    }

    public record Keys(string PublicKey, string PrivateKey) { }

    public record EventId([property: JsonPropertyName("id")] string Id) { }

    public record CountValue([property: JsonPropertyName("count")] int Count) { }
}