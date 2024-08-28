using Netstr.Messaging.Models;

namespace Netstr.Messaging.Events.Validators
{
    /// <summary>
    /// When the "-" tag is present, that means the event is "protected" and can only be published to relays by its author.
    /// </summary>
    public class ProtectedEventValidator : IEventValidator
    {
        private readonly ILogger<ProtectedEventValidator> logger;

        public ProtectedEventValidator(ILogger<ProtectedEventValidator> logger)
        {
            this.logger = logger;
        }

        public string? Validate(Event e, ClientContext context)
        {
            if (e.IsProtected())
            {
                if (!context.IsAuthenticated() || context.PublicKey != e.PublicKey)
                {
                    return Messages.AuthRequiredProtected;
                }
            }

            return null;
        }
    }
}
