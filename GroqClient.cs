using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIHider
{
    public static class GroqClient
    {
        // ⚠️ Paste your Groq key here
        private const string ApiKey = "";

        private const string Model = "openai/gpt-oss-120b";
        private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";

        private static readonly HttpClient _http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        public static async Task<string> AskAsync(string systemPrompt, string userMessage)
        {
            var body = new
            {
                model = Model,
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user",   content = userMessage  }
                },
                temperature = 0.5
            };

            string json = JsonSerializer.Serialize(body);

            using var req = new HttpRequestMessage(HttpMethod.Post, Endpoint)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            req.Headers.Add("Authorization", "Bearer " + ApiKey);

            using var resp = await _http.SendAsync(req);
            string respText = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"HTTP {(int)resp.StatusCode}: {respText}");

            using var doc = JsonDocument.Parse(respText);
            return doc.RootElement
                      .GetProperty("choices")[0]
                      .GetProperty("message")
                      .GetProperty("content")
                      .GetString() ?? "(empty)";
        }
    }
}
