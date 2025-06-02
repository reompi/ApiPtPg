using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiPtPg.Data;
using ApiPtPg.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ApiPtPg.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoldersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public FoldersController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetFolders()
        {
            // Get the UserId from the JWT token
            var userIdClaim = HttpContext.User.FindFirst("UserId");

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Query the folders that belong to this user
            var folders = await _context.Folders
                .Where(f => f.UserId == userId)  // Assuming Folder entity has a UserId property
                .ToListAsync();

            return Ok(new
            {
                Folders = folders
            });
        }


        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateFolder([FromBody] Folder folder)
        {
            // Check if the folder data is null
            if (folder == null)
            {
                return BadRequest("Folder data is null");
            }

            // Validate the folder name
            if (string.IsNullOrWhiteSpace(folder.Name))
            {
                return BadRequest("Folder name is required");
            }

            // Get the UserId from the JWT token
            var userIdClaim = HttpContext.User.FindFirst("UserId");

            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Assign the UserId to the folder
            folder.UserId = userId;
            folder.CreatedAt = DateTime.UtcNow;

            // Save the folder to the database
            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetFolders), new { id = folder.FolderId }, folder);
        }
        [Authorize]
        [HttpPut("{folderId}")]
        public async Task<IActionResult> UpdateFolderParent(int folderId, [FromBody]  UpdateFolderDto updateFolder)
        {
            // Find the folder by its ID
            var folder = await _context.Folders.FindAsync(folderId);
            if (folder == null)
            {
                return NotFound(); // 404 Not Found
            }

            // Update the parent folder ID
            folder.ParentFolderId = updateFolder.ParentFolderId;

            // Save changes to the database
            await _context.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
        // DELETE: api/Folders/{id}
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFolder(int id)
        {
            var folder = await _context.Folders.FindAsync(id);
            if (folder == null)
            {
                return NotFound();
            }

            // Listar todos os claims para depuração
            var allClaims = string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}"));
            // Tente encontrar o claim "UserId" (case sensitive)
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                return Unauthorized($"UserId não encontrado no token. Claims disponíveis: {allClaims}");
            }

            int userId = int.Parse(userIdClaim.Value);

            if (folder.UserId != userId)
            {
                return Forbid("Você não tem permissão para excluir esta pasta.");
            }

            _context.Folders.Remove(folder);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        // DELETE:  {id}
        [Authorize(Roles = "admin")]
        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteAllFoldersFromUser(int userId)
        {
            var folders = await _context.Folders.Where(f => f.UserId == userId).ToListAsync();

            if (!folders.Any())
            {
                return NotFound("Nenhuma pasta encontrada para este usuário.");
            }

            _context.Folders.RemoveRange(folders);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
