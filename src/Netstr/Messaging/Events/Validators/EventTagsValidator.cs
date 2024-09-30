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
            if (e.Tags.Length > this.limits.Value.MaxEventTags)
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
