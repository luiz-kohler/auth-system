using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Auth_Background_Service
{
    internal abstract class HttpClientBase
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _options;

        public HttpClientBase()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        protected async Task<TResponse> PostAsync<TRequest, TResponse>(string url, TRequest content, string token = null)
        {
            var jsonContent = new StringContent(JsonSerializer.Serialize(content), Encoding.UTF8, "application/json");

            if(!string.IsNullOrEmpty(token))
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.PostAsync(url, jsonContent);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception(jsonResponse);

            return JsonSerializer.Deserialize<TResponse>(jsonResponse, _options);
        }
    }
}
