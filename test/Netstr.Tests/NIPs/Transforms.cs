using Netstr.Messaging;
using Netstr.Messaging.Models;
using System.Reflection;
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
                    MessageType.Closed => [MessageType.Ok, row[1], row.GetString("Message")],
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
    }
}
