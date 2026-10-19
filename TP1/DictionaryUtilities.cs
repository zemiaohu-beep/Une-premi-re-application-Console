using System.ComponentModel;
using System.Globalization;
using System.Text.Json;

namespace TP1;

// Dans un seul projet, internal suffit pour la classe et ses methodes.
internal static class DictionaryUtilities
{
    internal static int GetIntOrDefault(Dictionary<string, string> values, string key, int defaultValue)
        => GetValueOrDefault(values, key, defaultValue);

    // Un convertisseur facultatif permet de prendre en charge les formats metier particuliers.
    internal static T GetValueOrDefault<T>(
        IReadOnlyDictionary<string, string> values,
        string key,
        T defaultValue,
        Func<string, T>? converter = null)
    {
        if (!values.TryGetValue(key, out string? text))
            return defaultValue;

        try
        {
            if (converter is not null)
                return converter(text);
            if (typeof(T) == typeof(string))
                return (T)(object)text;

            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            TypeConverter typeConverter = TypeDescriptor.GetConverter(targetType);
            if (typeConverter.CanConvertFrom(typeof(string)))
            {
                object? value = typeConverter.ConvertFromInvariantString(text);
                return value is T result ? result : defaultValue;
            }

            // Les objets complexes sont fournis en JSON, sans imposer un constructeur specifique.
            T? parsed = JsonSerializer.Deserialize<T>(text, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            });
            return parsed is null ? defaultValue : parsed;
        }
        catch (Exception error) when (error is FormatException or OverflowException
            or ArgumentException or JsonException or NotSupportedException)
        {
            return defaultValue;
        }
    }
}
