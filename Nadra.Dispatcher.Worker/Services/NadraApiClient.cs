using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Nadra.Dispatcher.Worker.Services
{
    public sealed class NadraApiClient
    {
        private readonly HttpClient _http;
        private readonly string _endpointPath;

        public NadraApiClient(
            HttpClient http,
            IConfiguration configuration)
        {
            _http = http;
            _endpointPath = configuration["NadraApi:EndpointPath"]!;
        }

        public async Task<HttpResponseMessage> SendAsync(
            string payload,
            CancellationToken ct)
        {
            using var content = new StringContent(
                payload,
                Encoding.UTF8,
                "application/json");

            return await _http.PostAsync(
                _endpointPath,
                content,
                ct);
        }
    }

}
