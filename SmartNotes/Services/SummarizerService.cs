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
            if (string.IsNullOrWhiteSpace(fullText))
                return "<p>No text to summarize.</p>";

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
            if (string.IsNullOrWhiteSpace(apiKey))
                throw new Exception("OpenRouter API key is missing in configuration.");

            var prompt = $@"
                You are an intelligent summarizer for this app.

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
                {text}";

            var payload = new
            {
                model = "openai/gpt-3.5-turbo",
                messages = new[] { new { role = "user", content = prompt } }
            };

            var json = JsonSerializer.Serialize(payload);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions")
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            HttpResponseMessage response;
            string body;
            try
            {
                response = await _http.SendAsync(request);
                body = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"OpenRouter response: {body}");
            }
            catch (Exception httpEx)
            {
                throw new Exception($"HTTP request to OpenRouter failed: {httpEx.Message}");
            }

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OpenRouter API error: {response.StatusCode} - {body}");

            try
            {
                using var doc = JsonDocument.Parse(body);
                var summary = doc.RootElement
                                 .GetProperty("choices")[0]
                                 .GetProperty("message")
                                 .GetProperty("content")
                                 .GetString();

                return summary?.Trim() ?? "<p>No summary found.</p>";
            }
            catch (Exception parseEx)
            {
                throw new Exception($"Failed to parse OpenRouter response: {parseEx.Message} - Body: {body}");
            }
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
