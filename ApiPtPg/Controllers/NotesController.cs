using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiPtPg.Data;
using ApiPtPg.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
namespace ApiPtPg.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Note>>> GetNotes()
        {
            // Get the UserId from the JWT token
            var userIdClaim = HttpContext.User.FindFirst("UserId");
            var roleClaim = HttpContext.User.FindFirst("Role"); // Assuming you have a Role claim

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);
            if (roleClaim != null && roleClaim.Value.Equals("user"))
            {

                // Query the notes that belong to the user
                var userNotes = await _context.Notes
                    .Where(n => n.UserId == userId)  // Assuming Note entity has a UserId property
                    .ToListAsync();

                return Ok(new
                {
                    Notes = userNotes
                });
            }
            // Admins can access all notes
            var allNotes = await _context.Notes.ToListAsync();
            return Ok(new
            {

                Notes = allNotes
            });
        }

        // GET: api/notes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Note>> GetNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);

            if (note == null)
            {
                return NotFound();
            }

            return note;
        }

        // POST: api/notes
        [Authorize]  // Ensure the user is authenticated
        [HttpPost]
        public async Task<ActionResult<Note>> PostNote(Note note)
        {
            // Get the UserId from the JWT token
            var userIdClaim = HttpContext.User.FindFirst("UserId");

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Set the current time for creation
            note.CreatedAt = DateTime.UtcNow;

            // Set the note's UserId to the authenticated user's ID
            note.UserId = userId;

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // Build the custom link (api/@{UserId}n{NoteId})
            var link = $"api/@n{note.Id}";

            // Return the created note along with the custom link
            return CreatedAtAction("GetNote", new { id = note.Id }, new { note, link });
        }

        // Update note folder
        [Authorize]
        [HttpPut("f{noteId}")]
        public async Task<IActionResult> UpdateNoteFolder(int noteId, [FromBody]UpdateNoteDto updateNote)
        {
            // Find the note by its ID
            var note = await _context.Notes.FindAsync(noteId);
            if (note == null)
            {
                return NotFound(); // 404 Not Found
            }

            // Update the folder ID
            note.FolderId = updateNote.FolderId;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
        [Authorize]
        [HttpPut("{customId}")]
        public async Task<IActionResult> PutNote(string customId, [FromBody] Note note)
        {
            // Extract the UserId and NoteId from the custom format @UserIdnNoteId
            var match = Regex.Match(customId, @"n(?<noteId>\d+)");

            if (!match.Success)
            {
                return BadRequest("Invalid ID format. Expected format is nNoteId.");
            }

            int noteId = int.Parse(match.Groups["noteId"].Value);

            // Get UserId from the JWT token
            var userIdClaim = HttpContext.User.FindFirst("UserId");

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);


            // Check if the IDs match
            var existingNote = await _context.Notes.FindAsync(noteId);

            if (existingNote == null)
            {
                return NotFound();
            }

            if (existingNote.UserId != userId)
            {
                return Forbid("You are not authorized to update this note.");
            }

            // Update the note
            existingNote.Heading = note.Heading;
            existingNote.Body = note.Body;

            // Update the note, but ensure CreatedAt remains unchanged
            _context.Entry(existingNote).Property(n => n.CreatedAt).IsModified = false;

            // Update other properties
            existingNote.Heading = note.Heading;
            existingNote.Body = note.Body;

            _context.Entry(existingNote).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(noteId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }


            return NoContent();
        }
        [Authorize]
        [HttpGet("checkOwnership/{noteId}")]
        public async Task<IActionResult> CheckNoteOwnership(int noteId)
        {
            // Get the UserId from the JWT token
            var userIdClaim = HttpContext.User.FindFirst("UserId");

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Find the note by noteId
            var note = await _context.Notes.FindAsync(noteId);

            if (note == null)
            {
                return NotFound("Note does not exist.");
            }

            // Check if the note belongs to the current user
            if (note.UserId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "You are not the owner of this note.");
            }

            return Ok();
        }



        // DELETE: api/notes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(int id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }

            _context.Notes.Remove(note);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool NoteExists(int id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }
        [Authorize]
        [HttpPost("{noteId}/like")]
        public async Task<IActionResult> LikeNote(int noteId)
        {
            // Get the user ID from the JWT token
            var userIdClaim = HttpContext.User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            var note = await _context.Notes
                .Include(n => n.Likes)
                .Include(n => n.Dislikes)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
            {
                return NotFound();
            }

            // Handle likes and dislikes logic as before...
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.NoteId == noteId);
            var existingDislike = await _context.Dislikes.FirstOrDefaultAsync(d => d.UserId == userId && d.NoteId == noteId);

            if (existingDislike != null)
            {
                _context.Dislikes.Remove(existingDislike);
            }

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Like removed successfully." });
            }

            var like = new Like { NoteId = noteId, UserId = userId };
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Note liked successfully." });
        }
        [Authorize]
        [HttpPost("{noteId}/dislike")]
        public async Task<IActionResult> DislikeNote(int noteId)
        {
            // Get the user ID from the JWT token
            var userIdClaim = HttpContext.User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);


            var note = await _context.Notes
                .Include(n => n.Likes)
                .Include(n => n.Dislikes)
                .FirstOrDefaultAsync(n => n.Id == noteId);

            if (note == null)
            {
                return NotFound();
            }

            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.NoteId == noteId);
            var existingDislike = await _context.Dislikes.FirstOrDefaultAsync(d => d.UserId == userId && d.NoteId == noteId);

            // If the user has liked the note, remove the like
            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
            }

            // If the user already disliked the note, remove the dislike
            if (existingDislike != null)
            {
                _context.Dislikes.Remove(existingDislike);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Dislike removed successfully." });
            }

            // If the user hasn't disliked the note yet, dislike it
            var dislike = new Dislike { NoteId = noteId, UserId = userId };
            _context.Dislikes.Add(dislike);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Note disliked successfully." });
        }
        [Authorize]
        [HttpPut("{id}/privacy")]
        public async Task<IActionResult> SwitchPrivacy(int id)
        {
            // Get the UserId from the JWT token
            var userIdClaim = HttpContext.User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Find the note that belongs to this user
            var note = await _context.Notes
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note == null)
            {
                return NotFound("Note not found or access denied.");
            }

            // Toggle the privacy status
            note.Private = !note.Private;

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok(new
            {
                note.Id,
                note.Heading,
                note.Private
            });
        }

        [Authorize]
        [HttpGet("feed")]
        public async Task<IActionResult> GetNotesFeed([FromQuery] string search = "")
        {
            search = Regex.Replace(search, @"\s+", "");
            // Get the user ID from the JWT token
            var userIdClaim = HttpContext.User.FindFirst("UserId");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Start building the query, including Likes and Dislikes
            var notesQuery = _context.Notes
                .Where(n => !n.Private) // Only public notes
                .Include(n => n.Likes)
                .Include(n => n.Dislikes)
                .AsEnumerable(); // Switch to client-side evaluation

            // Apply client-side search filter (after AsEnumerable)
            if (!string.IsNullOrWhiteSpace(search))
            {
                notesQuery = notesQuery.Where(n => n.Heading.Contains(search) || n.Body.ToString().Contains(search));
            }

            // Execute the query and return the results
            var notes = notesQuery
                .Select(n => new
                {
                    n.Id,
                    n.Heading,
                    n.Body,
                    LikesCount = n.Likes.Count(),
                    DislikesCount = n.Dislikes.Count(),
                    IsLiked = n.Likes.Any(l => l.UserId == userId),
                    IsDisliked = n.Dislikes.Any(d => d.UserId == userId),
                    CreatedAt = n.CreatedAt
                })
                .ToList();

            return Ok(notes);
        }



    }
}
