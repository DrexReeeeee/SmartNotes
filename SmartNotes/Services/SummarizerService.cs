using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SmartNotes.Services
{
    public class SummarizerService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public SummarizerService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<string> SummarizeText(string fullText)
        {
            var chunks = SplitIntoChunks(fullText, 2000);
            var summaries = new List<string>();

            foreach (var chunk in chunks)
            {
                var summary = await SummarizeChunk(chunk);
                summaries.Add(summary);
            }

            var combined = string.Join(" ", summaries);
            var finalSummary = await SummarizeChunk(combined);

            return finalSummary;
        }

        private async Task<string> SummarizeChunk(string text)
        {
            var apiKey = _config["OpenRouter:ApiKey"];

            var prompt = $@"
                You are an intelligent summarizer for a Blazor web-based note-taking app.

                Please summarize the following document into a detailed(All parts are covered), clean, formatted HTML content that will be rendered inside a <div contenteditable='true'>. Use:

                - <h2>, <h3> for section headers
                - <b>, <i>, <u> for emphasis
                - <ul>/<ol>/<li> for lists
                - <table> for tabular content

                ⚠️ IMPORTANT:
                - DO NOT include <!DOCTYPE>, <html>, <head>, or <body> tags.
                - Only return valid HTML that should appear inside a body tag — not raw text or escaped HTML.
                - The output will be directly injected into the DOM, so formatting must render properly.

                Document:
                {text}
";

            var payload = new
            {
                model = "mistralai/mistral-7b-instruct",
                messages = new[] {
                    new { role = "user", content = prompt }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://openrouter.ai/api/v1/chat/completions"),
                Headers = {
                    { "Authorization", $"Bearer {apiKey}" },
                    { "HTTP-Referer", "https://your-site.com" },
                    { "X-Title", "SmartNotes" }
                },
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenRouter error: {response.StatusCode} - {body}");

            using var doc = JsonDocument.Parse(body);
            var summary = doc.RootElement
                             .GetProperty("choices")[0]
                             .GetProperty("message")
                             .GetProperty("content")
                             .GetString();

            return summary?.Trim() ?? "<p>No summary found.</p>";
        }

        private List<string> SplitIntoChunks(string text, int chunkSize)
        {
            var chunks = new List<string>();
            for (int i = 0; i < text.Length; i += chunkSize)
            {
                int size = Math.Min(chunkSize, text.Length - i);
                chunks.Add(text.Substring(i, size));
            }
            return chunks;
        }
    }
}
