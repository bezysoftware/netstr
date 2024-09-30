namespace Netstr.Messaging
{
    public static class Messages
    {
        public const string ErrorInternal = "error: internal error while processing your message";
        public const string ErrorProcessingEvent = "error: unable to process the event";
        public const string InvalidId = "invalid: event id does not match";
        public const string InvalidSignature = "invalid: event signature verification failed";
        public const string InvalidCreatedAt = "invalid: event creation date is too far off from the current time";
        public const string InvalidSubscriptionIdTooLong = "invalid: subscription id is too long";
        public const string InvalidTooManyFilters = "invalid: too many filters";
        public const string InvalidTooManySubscriptions = "invalid: too many subscriptions";
        public const string InvalidLimitTooHigh = "invalid: filter limit is too high";
        public const string InvalidPayloadTooLarge = "invalid: message is too large";
        public const string InvalidEventExpired = "invalid: event is expired";
        public const string InvalidTooFewTagFields = "invalid: too few fields in tag";
        public const string InvalidTooManyTags = "invalid: too many tags";
        public const string InvalidCannotDelete = "invalid: cannot delete deletions and someone else's events";
        public const string AuthRequired = "auth-required: we only allow publishing and subscribing to authenticated clients";
        public const string AuthRequiredProtected = "auth-required: this event may only be published by its author";
        public const string AuthRequiredPublishing = "auth-required: we only allow publishing to authenticated clients";
        public const string AuthRequiredKind = "auth-required: subscribing to specified kind(s) requires authentication";
        public const string AuthRequiredWrongKind = "auth-required: event has a wrong kind";
        public const string AuthRequiredWrongTags = "auth-required: event has a challenge or relay";
        public const string DuplicateEvent = "duplicate: already have this event";
        public const string DuplicateReplaceableEvent = "duplicate: already have a newer version of this event";
        public const string AlreadyDeletedEvent = "duplicate: future version of this event was already deleted";
        public const string DuplicateDeletedEvent = "duplicate: this event was already deleted";
        public const string PowNotEnough = "pow: difficulty {0} is less than {1}";
        public const string PowNoMatch = "pow: difficulty {0} doesn't match target of {1}";
        public const string UnsupportedFilter = "unsupported: filter contains unknown elements";

        public const string CannotParseMessage = "unable to parse the message";
        public const string CannotProcessMessageType = "unknown message type";
    }
}
