using Netstr.Messaging.Models;

namespace Netstr.Messaging.Events.Validators
{
    /// <summary>
    /// Ensure older events cannot be republished if user vanished.
    /// </summary>
    public class UserVanishedValidator : IEventValidator
    {
        private readonly ILogger<UserVanishedValidator> logger;
        private readonly IUserCache userCache;

        public UserVanishedValidator(
            ILogger<UserVanishedValidator> logger,
            IUserCache userCache)
        {
            this.logger = logger;
            this.userCache = userCache;
        }

        public string? Validate(Event e, ClientContext context)
        {
            var user = this.userCache.GetByPublicKey(e.PublicKey);
            var vanished = user?.LastVanished ?? DateTimeOffset.MinValue;

            if (e.CreatedAt <= vanished)
            {
                this.logger.LogInformation($"Event {e.Id} is from user who already vanished on {vanished} (this event is from {e.CreatedAt})");
                return Messages.InvalidDeletedEvent;
            }

            return null;
        }
    }
}
