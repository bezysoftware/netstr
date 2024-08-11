using TechTalk.SpecFlow;

namespace Netstr.Tests.NIPs.Steps
{
    [Binding]
    public partial class Steps : IClassFixture<WebApplicationFactory>
    {
        private readonly WebApplicationFactory factory;
        private readonly ScenarioContext scenarioContext;

        public Steps(
            WebApplicationFactory factory,
            ScenarioContext scenarioContext)
        {
            this.factory = factory;
            this.scenarioContext = scenarioContext;

            scenarioContext.Set(new Clients());
        }

        [Given(@"a relay is running")]
        public void GivenARelayIsRunning()
        {
            // start server
            this.factory.CreateDefaultClient();
        }

        [Given(@"(.*) is connected to relay")]
        public async Task GivenAliceIsConnectedToRelay(string name, Keys keys)
        {
            var wsClient = this.factory.Server.CreateWebSocketClient();
            var httpClient = this.factory.CreateClient();

            wsClient.ConfigureRequest = http => http.Headers["sec-websocket-key"] = name;

            var ws = await wsClient.ConnectAsync(new Uri($"ws://localhost"), CancellationToken.None);

            var client = new Client(httpClient, ws, keys);

            _ = Task.Run(() => ws.ReceiveAsync(client.AddReceivedMessage));

            this.scenarioContext.Get<Clients>().Add(name, client);
        }
    }
}
