using Netstr.Messaging.Models;
using System.Collections.Concurrent;

namespace Netstr.Messaging.Subscriptions
{
    public class SubscriptionAdapter : IDisposable
    {
        private readonly IWebSocketAdapter webSocketAdapter;
        private readonly string subscriptionId;
        private readonly ConcurrentQueue<Event> eventsQueue;
        private MessageBatch? storedEventsBatch;

        public SubscriptionAdapter(IWebSocketAdapter webSocketAdapter, string subscriptionId, SubscriptionFilter[] filters)
        {
            this.webSocketAdapter = webSocketAdapter;
            this.subscriptionId = subscriptionId;
            this.eventsQueue = new ConcurrentQueue<Event>();
            
            Filters = filters;
        }

        public SubscriptionFilter[] Filters { get; }

        public bool StoredEventsSent => this.storedEventsBatch != null;

        public async Task SendEventAsync(Event e)
        {
            if (StoredEventsSent)
            {
                await this.webSocketAdapter.SendAsync(EventToMessage(e));
            } 
            else
            {
                this.eventsQueue.Enqueue(e);
            }
        }

        public async Task SendStoredEventsAsync(IEnumerable<Event> events)
        {
            if (StoredEventsSent)
            {
                throw new InvalidOperationException($"Cannot call {nameof(SendStoredEventsAsync)} method twice");
            }

            var storedMessages = events.Select(EventToMessage).ToArray();
            var dequeuedMessages = this.eventsQueue.Select(EventToMessage).ToArray();
            
            this.eventsQueue.Clear();

            // stored events, EOSE, queue events
            var batch = new MessageBatch(this.subscriptionId, [
                ..storedMessages,
                [
                    MessageType.EndOfStoredEvents,
                    this.subscriptionId
                ],
                ..dequeuedMessages
            ]);


            await this.webSocketAdapter.SendAsync(batch);

            this.storedEventsBatch = batch;

            // check again in case more messages arrive while initial batch was being sent
            if (!batch.IsCancelled && this.eventsQueue.Count > 0)
            {
                var messages = this.eventsQueue.Select(EventToMessage).ToArray();
                await this.webSocketAdapter.SendAsync(new MessageBatch(this.subscriptionId, [ messages ]));
            }
        }

        public void Dispose()
        {
            this.storedEventsBatch?.Cancel();

        }

        private object[] EventToMessage(Event e)
        {
            return [
                MessageType.Event,
                this.subscriptionId,
                e
            ];
        }
    }
}
