using Netstr.Messaging;
using Netstr.Messaging.Models;
using Netstr.Options;
using System.Reflection;
using System.Text.Json;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Netstr.Tests.NIPs
{
    [Binding]
    public class Transforms
    {
        [StepArgumentTransformation]
        public IEnumerable<SubscriptionFilterRequest> CreateSubscriptionFilters(Table table)
        {
            return table.CreateSet<SubscriptionFilterRequest>().Select((x, i) =>
            {
                var since = table.Rows[i].GetInt64("Since");
                var until = table.Rows[i].GetInt64("Until");
                return x with
                {
                    AdditionalData = table.Rows[i]
                        .Where(x => (x.Key.StartsWith("#") || x.Key.StartsWith("&")) && !string.IsNullOrEmpty(x.Value))
                        .ToDictionary(x => x.Key, x => JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(x.Value.Split(",")))),
                    Since = since > 0 ? DateTimeOffset.FromUnixTimeSeconds(since) : null,
                    Until = since > 0 ? DateTimeOffset.FromUnixTimeSeconds(until) : null,
                };
            });
        }

        [StepArgumentTransformation]
        public IEnumerable<object[]> CreateEventIds(Table table)
        {
            return table.Rows.Select<TableRow, object[]>(row =>
            {
                return row[0] switch
                {
                    MessageType.Event => [MessageType.Event, row[1], row.GetString("EventId")],
                    MessageType.EndOfStoredEvents => [MessageType.EndOfStoredEvents, row[1]],
                    MessageType.Ok => [MessageType.Ok, row[1], row.GetBoolean("Success"), row.GetString("Message") ?? ""],
                    MessageType.Closed => [MessageType.Closed, row[1], row.GetString("Message") ?? ""],
                    MessageType.Auth => [MessageType.Auth, row[1] ?? ""],
                    MessageType.Count => [MessageType.Count, row[1], row.GetInt32("Count")],
                    _ => throw new NotImplementedException(),
                };
            });
        }

        [StepArgumentTransformation]
        public Keys CreateKeys(Table table)
        {
            return table.CreateInstance<Keys>();
        }

        [StepArgumentTransformation]
        public Dictionary<string, string> CreateHeaders(Table table)
        {
            return table.Rows.ToDictionary(row => row.GetString("Header"), row => row.GetString("Value"));
        }

        public static IEnumerable<Event> CreateEvents(Table table, Client c)
        {
            return table.CreateSet<Event>().Select((e, i) =>
            {
                var tags = table.Rows[i].GetString("Tags");
                return e with
                {
                    Content = e.Content?.Replace("\\b", "\b").Replace("\\r", "\r").Replace("\\t", "\t").Replace("\\\"", "\"").Replace("\\n", "\n") ?? "",
                    CreatedAt = DateTimeOffset.FromUnixTimeSeconds(table.Rows[i].GetInt64("CreatedAt")),
                    PublicKey = string.IsNullOrEmpty(e.PublicKey) ? c.Keys.PublicKey : e.PublicKey,
                    Signature = string.IsNullOrEmpty(e.Signature) ? Helpers.Sign(e.Id, c.Keys.PrivateKey) : e.Signature,
                    Tags = string.IsNullOrWhiteSpace(tags)
                        ? []
                        : JsonSerializer.Deserialize<string[][]>(tags) ?? []
                };
            });
        }
    }
}
