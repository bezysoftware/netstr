namespace Netstr.Tests
{
    public static class ConfigurationBuilderExtensions
    {
        public static IEnumerable<KeyValuePair<string, string?>> ToKeyValuePairs(this Object settings, string settingsRoot)
        {
            if (settings == null)
            {
                yield break;
            }

            foreach (var property in settings.GetType().GetProperties())
            {
                if (property != null)
                {
                    var type = property.PropertyType;
                    var val = property.GetValue(settings);
                    object? defaultValue = type.IsValueType ? Activator.CreateInstance(type) : null;
                    
                    if (!object.Equals(val, defaultValue))
                    {
                        yield return new KeyValuePair<string, string>($"{settingsRoot}:{property.Name}", val?.ToString());
                    }
                }
            }
        }

        public static void AddInMemoryObject(this IConfigurationBuilder configurationBuilder, object settings, string settingsRoot)
        {
            configurationBuilder.AddInMemoryCollection(settings.ToKeyValuePairs(settingsRoot));
        }
    }
}
