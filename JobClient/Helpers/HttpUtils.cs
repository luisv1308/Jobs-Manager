using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobClient.Helpers
{
    public static class HttpUtils
    {
        public static async Task<string> HandleErrorResponse(HttpResponseMessage response)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
            string errorCode = errorResponse.GetProperty("errorCode").GetString();
            string message = errorResponse.GetProperty("message").GetString();
            return $"{message} Error Code: {errorCode}";
        }
    }
}