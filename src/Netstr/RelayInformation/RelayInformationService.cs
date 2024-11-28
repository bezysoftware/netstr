
using Microsoft.Extensions.Options;
using Netstr.Options;

namespace Netstr.RelayInformation
{
    public interface IRelayInformationService
    {
        RelayInformationModel GetDocument();
    }

    public class RelayInformationService : IRelayInformationService
    {
        private readonly IOptions<RelayInformationOptions> options;
        private readonly IOptions<LimitsOptions> limits;

        public RelayInformationService(IOptions<RelayInformationOptions> options, IOptions<LimitsOptions> limits)
        {
            this.options = options;
            this.limits = limits;
        }

        public RelayInformationModel GetDocument()
        {
            var opts = this.options.Value;
            var limits = this.limits.Value;

            return new RelayInformationModel
            {
                Name = opts.Name ?? RelayInformationDefaults.Name,
                Description = opts.Description ?? RelayInformationDefaults.Description,
                PublicKey = opts.PublicKey,
                Contact = opts.Contact,
                SupportedNips = opts.SupportedNips ?? [],
                Software = RelayInformationDefaults.Software,
                SoftwareVersion = opts.Version,
                Limits = new()
                {
                    MaxMessageLength = limits.MaxPayloadSize,
                    MinPowDifficulty = limits.Events.MinPowDifficulty,
                    CreatedAtLowerLimit = limits.Events.MaxCreatedAtLowerOffset,
                    CreatedAtUpperLimit = limits.Events.MaxCreatedAtUpperOffset,
                    MaxEventTags = limits.Events.MaxEventTags,
                    MaxLimit = limits.Subscriptions.MaxInitialLimit,
                    MaxFilters = limits.Subscriptions.MaxFilters,
                    MaxSubscriptionIdLength = limits.Subscriptions.MaxSubscriptionIdLength,
                    MaxSubscriptions = limits.Subscriptions.MaxSubscriptions
                }
            };
        }
    }
}
