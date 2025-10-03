using Microsoft.AspNetCore.Mvc;
using SmartNotes.Services;
using System.Text;
using UglyToad.PdfPig;

namespace SmartNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SummarizerController : ControllerBase
    {
        private readonly SummarizerService _summarizer;

        public SummarizerController(SummarizerService summarizer)
        {
            _summarizer = summarizer;
        }

        [HttpPost("summarize")]
        public async Task<IActionResult> Summarize(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                // Read PDF into memory
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;

                string extractedText;
                try
                {
                    var sb = new StringBuilder();
                    using var doc = PdfDocument.Open(ms);
                    foreach (var page in doc.GetPages())
                        sb.AppendLine(page.Text);

                    extractedText = sb.ToString();
                }
                catch (Exception pdfEx)
                {
                    return BadRequest($"Failed to read PDF: {pdfEx.Message}");
                }

                if (string.IsNullOrWhiteSpace(extractedText))
                    return BadRequest("No readable text in the PDF.");

                // Summarize
                var summary = await _summarizer.SummarizeText(extractedText);
                return Ok(new { summary });
            }
            catch (Exception ex)
            {
                // Return full stack trace for debugging (remove in production)
                return StatusCode(500, $"Server error: {ex}");
            }
        }
    }
}
