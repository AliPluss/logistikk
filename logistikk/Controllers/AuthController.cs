using logistikk.DTOs;
using logistikk.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace logistikk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto dto)
        {
            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            var response = new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                FirstName = user.FirstName
            };

            return StatusCode(201, response);
        }

        // Ny metode: logger inn brukeren og returnerer et JWT-token
        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginDto dto)
        {
            // Finn brukeren basert på e-post
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null)
            {
                // Generisk feilmelding, avslører ikke om det er e-post eller passord som er feil
                return Unauthorized("Feil e-post eller passord");
            }

            // Sjekk passordet, og tell mislykkede forsøk (låser konto etter 5 forsøk)
            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                return Unauthorized("Feil e-post eller passord");
            }

            // Passordet er riktig: generer og returner et JWT-token
            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        // Ny hjelpemetode: bygger selve JWT-tokenet
        private string GenerateJwtToken(AppUser user)
        {
            // Informasjon som legges inn i selve tokenet
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Name, user.FirstName)
            };

            // Nøkkelen som signerer tokenet (hentes fra secrets.json)
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Hvor lenge tokenet er gyldig (hentes fra appsettings.json)
            var expiresMinutes = _configuration.GetValue<int>("Jwt:ExpirationMinutes");

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [Authorize]
        [HttpGet("me")]
        public ActionResult<string> Me()
        {
            return Ok("Du er logget inn");
        }
    }
}