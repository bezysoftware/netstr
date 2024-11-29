namespace Netstr.Messaging.Models
{
    public static class MessageType
    {
        public const string Req = "REQ";
        public const string Event = "EVENT";
        public const string Auth = "AUTH";
        public const string Close = "CLOSE";
        public const string Closed = "CLOSED";
        public const string Notice = "NOTICE";
        public const string EndOfStoredEvents = "EOSE";
        public const string Ok = "OK";
        public const string Count = "COUNT";

        public static class Negentropy
        {
            public const string Open = "NEG-OPEN";
            public const string Error = "NEG-ERR";
            public const string Message = "NEG-MSG";
            public const string Close = "NEG-CLOSE";
        }
    }
}
