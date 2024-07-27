namespace Netstr.Options
{
    public class ConnectionOptions
    {
        public required string WebSocketsPath { get; init; }
        
        public required int WebSocketsReceiveBufferSize { get; init; }
    }
}
