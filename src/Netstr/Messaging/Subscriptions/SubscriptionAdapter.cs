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

        public void SendEvent(Event e)
        {
            if (StoredEventsSent)
            {
                this.webSocketAdapter.Send(EventToMessage(e));
            } 
            else
            {
                this.eventsQueue.Enqueue(e);
            }
        }

        public void SendStoredEvents(IEnumerable<Event> events)
        {
            if (StoredEventsSent)
            {
                throw new InvalidOperationException($"Cannot call {nameof(SendStoredEvents)} method twice");
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


            this.webSocketAdapter.Send(batch);

            this.storedEventsBatch = batch;

            // check again in case more messages arrive while initial batch was being sent
            if (!batch.IsCancelled && !this.eventsQueue.IsEmpty)
            {
                var messages = this.eventsQueue.Select(EventToMessage).ToArray();
                this.webSocketAdapter.Send(new MessageBatch(this.subscriptionId, [ messages ]));
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
