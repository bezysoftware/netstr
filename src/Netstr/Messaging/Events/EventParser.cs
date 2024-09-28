using Netstr.Extensions;
using Netstr.Messaging.Models;
using System.Text.Json;

namespace Netstr.Messaging.Events
{
    public static class EventParser
    {
        public static Event? TryParse(JsonDocument[] parameters, out Exception? exception)
        {
            try
            {
                exception = null;

                if (parameters.Length != 2)
                {
                    return null;
                }

                var e = parameters[1].DeserializeRequired<Event>();

                return e with
                {
                    Tags = e.Tags.Where(x => x.Length > 0).ToArray()
                };
            }
            catch (Exception ex)
            {
                exception = ex;
                return null;
            }
        }
    }
}
