using Netstr.Messaging.Models;
using TechTalk.SpecFlow;

namespace Netstr.Tests.NIPs.Steps
{
    public partial class Steps
    {
        [When(@"(.*) sends a count message (.*)")]
        public async Task WhenAliceSendsACountMessageAbcd(string client, string subscriptionId, IEnumerable<SubscriptionFilterRequest> filters)
        {
            var now = DateTimeOffset.UtcNow;
            var c = this.scenarioContext.Get<Clients>()[client];

            await c.WebSocket.SendCountAsync(subscriptionId, filters);
            await c.WaitForMessageAsync(now, ["COUNT", subscriptionId], ["CLOSED", subscriptionId]);
        }
    }
}
