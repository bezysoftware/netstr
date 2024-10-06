using System.Text.Encodings.Web;

namespace Netstr.Json
{
    /// <summary>
    /// Json encoder for nostr events which follows NIP-01's character escaping rules.
    /// </summary>
    public class NostrJsonEncoder : JavaScriptEncoder
    {
        private static int[] EscapableCharacters = [0x0A, 0x22, 0x5C, 0x0D, 0x09, 0x08, 0x0C];

        public override int MaxOutputCharactersPerInputCharacter => JavaScriptEncoder.Default.MaxOutputCharactersPerInputCharacter;

        public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
        {
            return JavaScriptEncoder.UnsafeRelaxedJsonEscaping.FindFirstCharacterToEncode(text, textLength);
        }

        public override unsafe bool TryEncodeUnicodeScalar(int unicodeScalar, char* buffer, int bufferLength, out int numberOfCharactersWritten)
        {
            return JavaScriptEncoder.UnsafeRelaxedJsonEscaping.TryEncodeUnicodeScalar(unicodeScalar, buffer, bufferLength, out numberOfCharactersWritten);
        }

        public override bool WillEncode(int unicodeScalar)
        {
            return EscapableCharacters.Contains(unicodeScalar);
        }
    }
}
