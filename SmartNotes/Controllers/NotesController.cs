using Microsoft.AspNetCore.Mvc;
using SmartNotes.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using SmartNotes.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace SmartNotes.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("api/[controller]")]
    public class NotesController : ControllerBase
    {
        private readonly ApplicationDbContext _notes;

        public NotesController(ApplicationDbContext notes)
        {
            _notes = notes;
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : null;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes()
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var notes = await _notes.UserNotes
                    .Where(n => n.UserId == userId)
                    .OrderByDescending(n => n.LastUpdatedAt)
                    .ToListAsync();

                return Ok(notes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var note = await _notes.UserNotes
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

                if (note == null)
                    return NotFound("Note not found or access denied.");

                return Ok(note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateNotes([FromBody] UserNotes usernotes)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                usernotes.UserId = userId.Value;
                usernotes.CreatedAt = DateTime.Now;
                usernotes.LastUpdatedAt = DateTime.Now;

                _notes.UserNotes.Add(usernotes);
                await _notes.SaveChangesAsync();

                return Ok(usernotes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditNotes(int id, [FromBody] UserNotes usernotes)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var note = await _notes.UserNotes
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

                if (note == null)
                    return NotFound("Note not found or user unauthorized.");

                note.Title = usernotes.Title;
                note.Content = usernotes.Content;
                note.LastUpdatedAt = DateTime.Now;

                await _notes.SaveChangesAsync();

                return Ok(note);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = GetUserId();
                if (userId == null)
                    return Unauthorized("User not authenticated");

                var note = await _notes.UserNotes
                    .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

                if (note == null)
                    return NotFound("Note not found or access denied.");

                _notes.UserNotes.Remove(note);
                await _notes.SaveChangesAsync();

                return Ok("Note successfully removed.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
    }
}
