using System.Text.Json;

namespace ChatApp.Common.Protocol
{
    public class Envelope
    {
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public string Type { get; set; } = "";
        public string? RequestId { get; set; }
        public JsonElement Data { get; set; }

        public static Envelope Create<T>(string type, T payload, string? requestId = null)
        {
            return new Envelope
            {
                Type = type,
                RequestId = requestId,
                Data = JsonSerializer.SerializeToElement(payload, Options)
            };
        }

        public T? ReadData<T>()
        {
            return Data.Deserialize<T>(Options);
        }
    }
}