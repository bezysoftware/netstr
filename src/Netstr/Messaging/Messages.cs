namespace Netstr.Messaging
{
    public static class Messages
    {
        public const string ErrorInternal = "error: internal error while processing your message";
        public const string ErrorProcessingEvent = "error: unable to process the event";
        public const string InvalidId = "invalid: event id does not match";
        public const string InvalidSignature = "invalid: event signature verification failed";
        public const string DuplicateEvent = "duplicate: already have this event";
        public const string DuplicateReplaceableEvent = "duplicate: already have a newer version of this event";

        public const string CannotParseMessage = "unable to parse the message";
        public const string CannotProcessMessageType = "unknown message type";
    }
}
