using System.Text.Json;

namespace CommandStore
{
    public static class DeserialiserHelper
    {
        public static T Deserialize<T>(byte[] item)
        {
            var utf8Reader = new Utf8JsonReader(item);
            return JsonSerializer.Deserialize<T>(ref utf8Reader);
        }
    }
}
