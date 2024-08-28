using Microsoft.Extensions.Options;
using Netstr.Messaging.Models;
using Netstr.Options;

namespace Netstr.Messaging.Events.Validators
{
    /// <summary>
    /// <see cref="IEventValidator"/> which validates event's created_at is not too far in the past or in the future.
    /// </summary>
    public class EventCreatedAtValidator : IEventValidator
    {
        private readonly IOptions<LimitsOptions> limits;

        public EventCreatedAtValidator(IOptions<LimitsOptions> limits)
        {
            this.limits = limits;
        }

        public string? Validate(Event e, ClientContext context)
        {
            var limits = this.limits.Value;
            var now = DateTimeOffset.Now;

            if (limits.MaxCreatedAtLowerOffset > 0 && e.CreatedAt < now.AddSeconds(-limits.MaxCreatedAtLowerOffset))
            {
                return Messages.InvalidCreatedAt;
            }

            if (limits.MaxCreatedAtUpperOffset > 0 && e.CreatedAt > now.AddSeconds(limits.MaxCreatedAtUpperOffset))
            {
                return Messages.InvalidCreatedAt;
            }

            return null;
        }
    }
}
