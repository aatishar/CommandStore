using System.Text.Json.Serialization;

namespace CommandStore
{
    public class Command
    {
        public int CommandNumber { get; set; }

        public string CommandType { get; set; }

        public byte[] ItemInBytes { get; set; }

        [JsonIgnore]
        public object ItemObject { get; set; }

    }
}
