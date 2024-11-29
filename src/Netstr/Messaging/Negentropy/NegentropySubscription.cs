using Negentropy;

namespace Netstr.Messaging.Negentropy
{
    public record NegentropySubscription
    {
        private global::Negentropy.Negentropy negentropy;

        public NegentropySubscription(IEnumerable<INegentropyItem> items, uint frameSizeLimit)
        {
            this.negentropy = new NegentropyBuilder(new NegentropyOptions { FrameSizeLimit = frameSizeLimit }).AddRange(items).Build();
            
            LastMessageOn = DateTimeOffset.UtcNow;
            StartedOn = DateTimeOffset.UtcNow;
        }

        public DateTimeOffset LastMessageOn { get; private set; }
        
        public DateTimeOffset StartedOn { get; init; }

        public NegentropyReconciliation Reconcile(string q)
        {
            LastMessageOn = DateTimeOffset.UtcNow;

            return this.negentropy.Reconcile(q);
        }
    }
}
