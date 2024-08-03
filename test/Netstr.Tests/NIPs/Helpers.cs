using Nano.Bech32;
using NBitcoin.Secp256k1;

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
            Bech32Encoder.Decode(privateKey, out var hrp, out var privkey);
            var hash = Convert.FromHexString(id);

            var buf = new ArraySegment<byte>(new byte[64]);
            ECPrivKey.Create(privkey).SignBIP340(hash).WriteToSpan(buf);

            return Convert.ToHexString(buf).ToLowerInvariant();
        }
    }
}
