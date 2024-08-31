using Netstr.Messaging.Models;

namespace Netstr.Messaging.Events.Validators
{
    /// <summary>
    /// <see cref="IEventValidator"/> which checks the event isn't expired.
    /// </summary>
    public class ExpiredEventValidator : IEventValidator
    {
        public string? Validate(Event e, ClientContext context)
        {
            var exp = e
                .GetExpirationValue()
                .GetValueOrDefault(DateTimeOffset.MaxValue);

            return exp < DateTimeOffset.UtcNow
                ? Messages.InvalidEventExpired
                : null;
        }
    }
}
