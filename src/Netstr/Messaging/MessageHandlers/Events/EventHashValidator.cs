using Netstr.Messaging.Models;
using System.Security.Cryptography;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers.Events
{
    public class EventHashValidator : IEventValidator
    {
        public string? Validate(Event e)
        {
            var obj = (object[])[
                0,
                e.PublicKey,
                e.CreatedAt.ToUnixTimeSeconds(),
                e.Kind,
                e.Tags,
                e.Content
            ];

            // TODO: ToHexStringLower in .NET 9
            var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(obj))).ToLower();

            return hash.Equals(e.Id)
                ? null
                : Messages.InvalidId;
        }
    }
}
