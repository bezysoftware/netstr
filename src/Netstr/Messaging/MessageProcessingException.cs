namespace Netstr.Messaging
{
    public abstract class MessageProcessingException : Exception
    {
        protected readonly object[] reply = [];

        protected MessageProcessingException(object[] reply, string message, Exception? innerException = null) 
            : base(message, innerException)
        {
            this.reply = reply;
        }

        protected MessageProcessingException(object[] reply)
        {
            this.reply = reply;
        }

        public virtual object[] GetSenderReply()
        {
            return this.reply;
        }
    }

    public class UnknownMessageProcessingException : MessageProcessingException
    {
        public UnknownMessageProcessingException(string message, Exception? innerException = null) 
            : base(["NOTICE", message], message, innerException)
        {
        }
    }
}
