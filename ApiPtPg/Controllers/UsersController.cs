using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiPtPg.Data;
using ApiPtPg.Models;
using Microsoft.AspNetCore.Cors;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
namespace ApiPtPg.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public UsersController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpPost("login")]
        public async Task<ActionResult<dynamic>> Login(UserLoginModel model)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Password == model.Password && u.AccountStatus != "deleted");

            if (user == null)
            {
                return NotFound("Invalid username or password");
            }

            // Generate the JWT token with the user's role
            var tokenService = new TokenService(_configuration);
            var token = tokenService.GenerateToken(user);

            return new
            {
                user.Username,
                user.Email,
                user.Role,  // Send role to the client
                Token = token  // Return the JWT token
            };
        }

        // GET: api/users
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users
                .Where(u => u.AccountStatus != "deleted")
                .ToListAsync();
        }

        // GET: api/users/5

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users
                .Where(u => u.Id == id && u.AccountStatus != "deleted")
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }
        //Put api/makeAdmin
        [Authorize(Roles = "admin")]
        [HttpPut("{id}/makeAdmin")]
        public async Task<IActionResult> MakeAdmin(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            user.Role = "admin";

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok($"User with ID {id} has been updated to admin.");
        }
        // POST: api/users

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // DELETE: api/users/5

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Marca o usuário como deletado
            user.AccountStatus = "deleted";
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
        // GET: api/users/{id}/username
        [Authorize]
        [HttpGet("{id}/username")]
        public async Task<ActionResult<string>> GetUsername(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user.Username);
        }
    }
}
