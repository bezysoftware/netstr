using Negentropy;

namespace Netstr.Messaging.Negentropy
{
    public record NegentropyEvent(string Id, long Timestamp) : INegentropyItem
    {
    }
}
