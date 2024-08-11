using FluentAssertions;
using Microsoft.Net.Http.Headers;
using System.Linq;
using System.Text.Json;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Netstr.Tests.NIPs.Steps
{
    public partial class Steps
    {
        [When(@"(.*) sends a (.*) HTTP request to its websockets endpoint")]
        public async Task WhenClientSendsAnHTTPRequest(string client, string method, Dictionary<string, string> headers)
        {
            var c = this.scenarioContext.Get<Clients>()[client];
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Parse(method)
            };

            headers.ToList().ForEach(x => message.Headers.Add(x.Key, x.Value));
            message.Headers.TryAddWithoutValidation(HeaderNames.Origin, "test");
            
            var response = await c.http.SendAsync(message);

            c.AddResponse(response);
        }

        [Then(@"(.*) receives a response with headers")]
        public void ThenResponseFromRelayContainsHeaders(string client, Dictionary<string, string> headers)
        {
            var c = this.scenarioContext.Get<Clients>()[client];
            var response = c.GetHttpResponses().Last();

            response.Headers
                .Select(x => KeyValuePair.Create(x.Key, string.Join(",", x.Value)))
                .ToDictionary()
                .Should()
                .Contain(headers);
        }

        [Then(@"(.*) receives a response with json content")]
        public async Task ThenAliceReceivesAResponseWithJsonContent(string client, Table table)
        {
            var c = this.scenarioContext.Get<Clients>()[client];
            var response = c.GetHttpResponses().Last();

            var content = await response.Content.ReadAsStringAsync();
            var fields = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);

            foreach (var row in table.Rows)
            {
                var field = row.GetString("Field");
                var type = row.GetString("Type") switch
                {
                    "string" => JsonValueKind.String,
                    "int[]" => JsonValueKind.Array,
                    _ => throw new NotImplementedException()
                };

                fields?[field].ValueKind.Should().Be(type);
            }
        }
    }
}
