using Microsoft.Extensions.Options;
using Netstr.Messaging.Models;
using Netstr.Options;

namespace Netstr.Messaging.Events.Validators
{
    /// <summary>
    /// <see cref="IEventValidator"/> which validates event's tags.
    /// </summary>
    public class EventTagsValidator : IEventValidator
    {
        private readonly IOptions<LimitsOptions> limits;

        public EventTagsValidator(IOptions<LimitsOptions> limits)
        {
            this.limits = limits;
        }

        public string? Validate(Event e, ClientContext context)
        {
            var limits = this.limits.Value.Events;

            if (limits.MaxEventTags > 0 && e.Tags.Length > limits.MaxEventTags)
            {
                return Messages.InvalidTooManyTags;
            }

            if (e.Tags.Any(x => x.Length == 0))
            {
                return Messages.InvalidTooFewTagFields;
            }

            return null;
        }
    }
}
