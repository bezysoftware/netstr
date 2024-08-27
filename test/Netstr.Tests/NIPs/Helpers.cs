using NBitcoin.Secp256k1;
using Netstr.Messaging.Models;
using System.Text.Json;

namespace Netstr.Tests.NIPs
{
    public static class Helpers
    {
        /// <summary>
        /// If the <paramref name="verify"/> action throws it wait for <paramref name="delay"/> amount of time and tries again.
        /// </summary>
        public static async Task VerifyWithDelayAsync(Action verify, TimeSpan? delay = null)
        {
            try
            {
                verify();
            }
            catch
            {
                await Task.Delay(delay ?? TimeSpan.FromSeconds(2));
                verify();
            }
        }

        public static string Sign(string id, string privateKey)
        {
            var hash = Convert.FromHexString(id);
            var privkey = Convert.FromHexString(privateKey);

            var buf = new ArraySegment<byte>(new byte[64]);
            ECPrivKey.Create(privkey).SignBIP340(hash).WriteToSpan(buf);

            return Convert.ToHexString(buf).ToLowerInvariant();
        }

        public static string GenerateId(Event e)
        {
            var obj = (object[])[
                0,
                e.PublicKey,
                e.CreatedAt.ToUnixTimeSeconds(),
                e.Kind,
                e.Tags,
                e.Content
            ];

            return Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(obj))).ToLower();
        }

        public static Event FinalizeEvent(Event e, string privateKey)
        {
            var id = GenerateId(e);

            return e with
            {
                Id = id,
                Signature = Sign(id, privateKey)
            };
        }
    }
}
