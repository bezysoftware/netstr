using Microsoft.Extensions.Options;
using Netstr.Messaging.Models;
using Netstr.Options;

namespace Netstr.Messaging.Events.Validators
{
    /// <summary>
    /// <see cref="IEventValidator"/> which validates event's proof of work against congiured limits.
    /// </summary>
    public class EventPowValidator : IEventValidator
    {
        private readonly IOptions<LimitsOptions> limits;

        public EventPowValidator(IOptions<LimitsOptions> limits)
        {
            this.limits = limits;
        }

        public string? Validate(Event e, ClientContext context)
        {
            if (this.limits.Value.MinPowDifficulty <= 0)
            {
                return null;
            }

            var difficulty = e.GetDifficulty();

            if (difficulty < this.limits.Value.MinPowDifficulty)
            {
                return string.Format(Messages.PowNotEnough, difficulty, this.limits.Value.MinPowDifficulty);
            }

            var nonce = e.Tags.FirstOrDefault(x => x.Length == 3 && x[0] == EventTag.Nonce);

            // if there is a target difficulty check if it matches the actual one
            if (nonce != null && int.TryParse(nonce[2], out var expectedDiff) && expectedDiff != difficulty)
            {
                return string.Format(Messages.PowNoMatch, difficulty, this.limits.Value.MinPowDifficulty);
            }

            return null;
        }
    }
}
