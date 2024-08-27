namespace Netstr.Messaging.Models
{
    /// <summary>
    /// Holds basic info about a client.
    /// </summary>
    public class ClientContext
    {
        public ClientContext(string clientId)
        {
            ClientId = clientId;
            Challenge = Guid.NewGuid().ToString();
        }

        public string ClientId { get; }

        public string Challenge { get; }
        
        public string? PublicKey { get; private set; }

        public bool IsAuthenticated() => !string.IsNullOrEmpty(PublicKey); 

        public void Authenticate(string publicKey)
        {
            lock (Challenge)
            {
                if (PublicKey != null && PublicKey != publicKey)
                {
                    throw new InvalidOperationException($"Client {ClientId} is already authenticated with a different pubkey");
                }

                PublicKey = publicKey;
            }
        }
    }
}
