using System.Text.Json;

namespace Netstr.Extensions
{
    public static class JsonExtensions
    {
        /// <summary>
        /// Deserializes given document. If the result is null it throws <see cref="ArgumentNullException"/>.
        /// </summary>
        public static T DeserializeRequired<T>(this JsonDocument document)
        {
            var result = document.Deserialize<T>();

            if (result == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return result;
        }

        /// <summary>
        /// Deserializes given document. If the result is null it throws <see cref="ArgumentNullException"/>.
        /// </summary>
        public static T DeserializeRequired<T>(this JsonElement document)
        {
            var result = document.Deserialize<T>();

            if (result == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return result;
        }
    }
}
