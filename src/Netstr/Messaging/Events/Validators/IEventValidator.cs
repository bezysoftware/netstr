using Netstr.Messaging.Models;

namespace Netstr.Messaging.Events.Validators
{
    public interface IEventValidator
    {
        /// <summary>
        /// Validates given event, returns null if validation passes, or error message.
        /// </summary>
        string? Validate(Event e, ClientContext context);
    }
}
