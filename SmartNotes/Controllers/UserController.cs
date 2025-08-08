using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SmartNotes.Models;
using SmartNotes.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;


namespace SmartNotes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public UserController(ApplicationDbContext context, IConfiguration config)
        {
            this._context = context;
            this._config = config;
        }

        [HttpGet]
        public List<Users> GetUser() {
            return _context.Users.OrderBy(x => x.Id).ToList();  
        }
        [HttpGet("{Id}")]

        public IActionResult GetUserById(int id) {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            else { 
                return Ok(user);
            }
        }

        [HttpPost("register")]
        public IActionResult Register(RegisterDto userdto) {

            var user = _context.Users.FirstOrDefault(x => x.Email == userdto.Email);
            if (user != null)
            {
                return BadRequest("Email already registered");
            }
            Users newUser = new Users
            {
                UserName = userdto.UserName,
                Email = userdto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(userdto.Password),
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(newUser);
            _context.SaveChanges();

            return Ok(newUser);
        }

        [HttpPost("login")]
        public IActionResult Login(LoginDto userdto)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == userdto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(userdto.Password, user.Password))
            {
                return BadRequest("Invalid Credentials");
            }

            var token = JwtHelper.GenerateJwtToken(user, _config);

            return Ok(new { token });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return Ok("Logged out");
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) {
                return NotFound();
            }
            return Ok(new
            {
                user.Id,
                user.UserName,
                user.Email
            });
        }
    }

}

