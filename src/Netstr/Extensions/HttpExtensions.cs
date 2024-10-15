namespace Netstr.Extensions
{
    public static class HttpExtensions
    {
        /// <summary>
        /// Gets the current normalized URL (host+path) where the relay is running. 
        /// </summary>
        public static string GetNormalizedUrl(this HttpRequest ctx)
        {
            return $"{ctx.Host}{ctx.Path}".TrimEnd('/');
        }
    }
}
