using Netstr.Messaging.Models;

namespace Netstr.Messaging.Events.Validators
{
    public static class EventValidatorsExtensions
    {
        /// <summary>
        /// Runs validations for the given event and returns the first error or null.
        /// </summary>
        public static string? ValidateEvent(this IEnumerable<IEventValidator> validators, Event e, ClientContext context)
        {
            foreach (var validator in validators)
            {
                var error = validator.Validate(e, context);
                if (error != null)
                {
                    return error;
                }
            }

            return null;
        }
    }
}
