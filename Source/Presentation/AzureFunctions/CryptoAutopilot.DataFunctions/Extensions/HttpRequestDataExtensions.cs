using System.Net;

using Microsoft.Azure.Functions.Worker.Http;

namespace CryptoAutopilot.DataFunctions.Extensions;

internal static class HttpRequestDataExtensions
{
    public static async Task<HttpResponseData> CreateOkJsonResponseAsync<T>(this HttpRequestData request, T instance)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync<T>(instance);
        return response;
    }
}
