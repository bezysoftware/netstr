using Netstr.Messaging.Models;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Netstr.Messaging.Events.Validators
{
    /// <summary>
    /// <see cref="IEventValidator"/> which validates event's id.
    /// </summary>
    public class EventHashValidator : IEventValidator
    {
        private static JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public string? Validate(Event e, ClientContext context)
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
            var hash = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(obj, serializerOptions))).ToLower();

            return hash.Equals(e.Id)
                ? null
                : Messages.InvalidId;
        }
    }
}
