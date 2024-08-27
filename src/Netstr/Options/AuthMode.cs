namespace Netstr.Options
{
    public enum AuthMode
    {
        /// <summary>
        /// Auth is only required for specific usages. This is the default.
        /// </summary>
        WhenNeeded,

        /// <summary>
        /// Auth is always required for publishing and subscribing.
        /// </summary>
        Always,

        /// <summary>
        /// Auth is required when publishing events and when needed.
        /// </summary>
        Publishing,

        /// <summary>
        /// Auth is completely disabled. When set, even the AUTH message isn't sent.
        /// </summary>
        Disabled
    }
}
