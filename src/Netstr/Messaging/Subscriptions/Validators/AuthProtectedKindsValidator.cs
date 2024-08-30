using Microsoft.Extensions.Options;
using Netstr.Extensions;
using Netstr.Messaging.Models;
using Netstr.Options;

namespace Netstr.Messaging.Subscriptions.Validators
{
    /// <summary>
    /// Checks if any of the filters contains a protected kind. If it does authentication is required.
    /// </summary>
    public class AuthProtectedKindsValidator : ISubscriptionRequestValidator
    {
        private readonly IOptions<AuthOptions> auth;

        public AuthProtectedKindsValidator(IOptions<AuthOptions> auth)
        {
            this.auth = auth;
        }

        public string? CanSubscribe(string id, ClientContext context, IEnumerable<SubscriptionFilter> filters)
        {
            var auth = this.auth.Value;

            if (auth.Mode == AuthMode.Disabled)
            {
                return null;
            }

            var kinds = auth.ProtectedKinds.EmptyIfNull();

            if (!kinds.Any())
            {
                return null;
            }

            var anyProtectedKinds = filters.Any(x => x.Kinds.Any(kind => kinds.Contains(kind)));

            return anyProtectedKinds && !context.IsAuthenticated()
                ? Messages.AuthRequiredKind
                : null;
        }
    }
}
