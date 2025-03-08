using System.Text.Json;

namespace Playground.EventSourcing;

public static class ObjectExtensions
{
    private static JsonSerializerOptions? _options = null;
    private static JsonSerializerOptions Options => _options ??= new JsonSerializerOptions { WriteIndented = true };
    
    public static string ToPrettyJson<T>(this T obj) => JsonSerializer.Serialize(obj, Options);
}