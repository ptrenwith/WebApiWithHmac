using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Company.Product.IntegrationTests.Extentions
{
    public static class HttpResponseMessageExtensions
    {
        public static async Task<TContent> GetContent<TContent>(this HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.Content == null)
                return default;

            var jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());

            var json = await httpResponseMessage.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TContent>(json, jsonSerializerOptions);
        }
    }
}
