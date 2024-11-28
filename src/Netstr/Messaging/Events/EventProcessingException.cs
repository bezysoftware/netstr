using Netstr.Messaging.Models;

namespace Netstr.Messaging.Events
{
    public class EventProcessingException : MessageProcessingException
    {
        public EventProcessingException(Event e, string message, Exception? innerException = null)
            : base(["OK", e.Id, false, message], $"Event {e.ToStringUnique()} processing failed: {message}", innerException)
        {
        }
    }
}
