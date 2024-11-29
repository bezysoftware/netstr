using Microsoft.Extensions.Options;
using Netstr.Options;

namespace Netstr.Messaging.Negentropy
{
    public interface INegentropyAdapterFactory
    {
        INegentropyAdapter CreateAdapter(IWebSocketAdapter adapter);
    }

    public class NegentropyAdapterFactory : INegentropyAdapterFactory
    {
        private readonly ILogger<NegentropyAdapter> logger;
        private readonly IOptions<LimitsOptions> options;

        public NegentropyAdapterFactory(ILogger<NegentropyAdapter> logger, IOptions<LimitsOptions> options)
        {
            this.logger = logger;
            this.options = options;
        }

        public INegentropyAdapter CreateAdapter(IWebSocketAdapter adapter)
        {
            return new NegentropyAdapter(this.logger, adapter, this.options);
        }
    }
}
