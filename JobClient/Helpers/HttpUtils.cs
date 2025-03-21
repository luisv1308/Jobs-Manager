using JobClient.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobClient.Helpers
{
    public static class HttpUtils
    {
        public static async Task<string> HandleErrorResponse(HttpResponseMessage response)
        {
            var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            string details = "";
            if (errorResponse.Details != null)
            {
                details = string.Join(", ", errorResponse.Details);
            }

            return $"{errorResponse.Message} == Error Code: {errorResponse.ErrorCode} == Details: {details}";
        }
    }
}