using Gherkin;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Netstr.Tests.NIPs
{
    public static class WebSocketExtensions
    {
        public static async Task SendAsync(this WebSocket ws, object[] message, CancellationToken? cancellationToken = null)
        {
            var token = cancellationToken ?? CancellationToken.None;
            await ws.SendAsync(JsonSerializer.SerializeToUtf8Bytes(message), WebSocketMessageType.Text, true, token);
        }

        public static async Task ReceiveAsync(this WebSocket ws, Action<JsonElement[]> action, CancellationToken? cancellationToken = null)
        {
            var token = cancellationToken ?? CancellationToken.None;

            try {
                while (ws.State == WebSocketState.Open)
                {
                    var buffer = new ArraySegment<byte>(new byte[65536]);

                    using var stream = new MemoryStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8);

                    while (true)
                    {
                        var result = await ws.ReceiveAsync(buffer, token);
                        stream.Write(buffer.Array, buffer.Offset, result.Count);
                        if (result.EndOfMessage) break;
                    }

                    stream.Seek(0, SeekOrigin.Begin);

                    var data = await reader.ReadToEndAsync();
                    var obj = JsonSerializer.Deserialize<JsonElement[]>(data);

                    if (obj == null)
                    {
                        throw new JsonException($"Couldn't deserialize response '{data}'");
                    }

                    action(obj);
                } 
            } 
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
