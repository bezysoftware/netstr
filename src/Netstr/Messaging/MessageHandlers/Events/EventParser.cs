using Netstr.Extensions;
using Netstr.Messaging.Models;
using System.Text.Json;

namespace Netstr.Messaging.MessageHandlers.Events
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

                return parameters[1].DeserializeRequired<Event>();
            }
            catch (Exception ex)
            {
                exception = ex;
                return null;
            }
        }
    }
}
