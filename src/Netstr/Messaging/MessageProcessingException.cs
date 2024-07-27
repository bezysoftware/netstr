using Netstr.Messaging.Models;

namespace Netstr.Messaging
{
    public class MessageProcessingException : Exception
    {
        private readonly object[] reply = [];

        public MessageProcessingException(Event e, string message) 
            : base($"Event {e.ToStringUnique()} processing failed: {message}")
        {
            this.reply = ["OK", e.Id, false, message];
        }

        public MessageProcessingException(Event e, string message, Exception innerException)
            : base(message, innerException)
        {
            this.reply = ["OK", e.Id, false, message];
        }

        public MessageProcessingException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
            this.reply = ["NOTICE", message];
        }

        public object[] GetSenderReply()
        {
            return this.reply;
        }
    }
}
