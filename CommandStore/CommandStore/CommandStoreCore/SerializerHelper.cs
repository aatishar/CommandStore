using System.Text.Json;

namespace CommandStore
{
    class SerializerHelper
    {
        public static byte[] Serialize<T>(T item)
        {
            return JsonSerializer.SerializeToUtf8Bytes<T>(item);
        }
    }
}
