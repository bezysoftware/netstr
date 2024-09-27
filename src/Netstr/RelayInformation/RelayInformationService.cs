
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
                    MinPowDifficulty = limits.MinPowDifficulty,
                    CreatedAtLowerLimit = limits.MaxCreatedAtLowerOffset,
                    CreatedAtUpperLimit = limits.MaxCreatedAtUpperOffset,
                    MaxEventTags = limits.MaxEventTags,
                    MaxLimit = limits.MaxInitialLimit,
                    MaxFilters = limits.MaxFilters,
                    MaxSubscriptionIdLength = limits.MaxSubscriptionIdLength,
                    MaxSubscriptions = limits.MaxSubscriptions
                }
            };
        }
    }
}
