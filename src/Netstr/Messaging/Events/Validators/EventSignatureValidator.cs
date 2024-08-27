using Netstr.Messaging.Models;

namespace Netstr.Messaging.Events.Validators
{
    /// <summary>
    /// <see cref="IEventValidator"/> which validates event's signature.
    /// </summary>
    public class EventSignatureValidator : IEventValidator
    {
        public string? Validate(Event e)
        {
            try
            {
                var pubkey = Convert.FromHexString(e.PublicKey);
                var sig = Convert.FromHexString(e.Signature);
                var id = Convert.FromHexString(e.Id);

                if (!NBitcoin.Secp256k1.SecpSchnorrSignature.TryCreate(sig, out var signature))
                {
                    return Messages.InvalidSignature;
                }

                return NBitcoin.Secp256k1.Context.Instance.CreateXOnlyPubKey(pubkey).SigVerifyBIP340(signature, id)
                    ? null
                    : Messages.InvalidSignature;
            }
            catch
            {
                return Messages.InvalidSignature;
            }
        }
    }
}
