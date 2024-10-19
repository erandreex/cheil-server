using server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using server.Services;

[Route("/api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly UserService _userService;

    public AuthController(IConfiguration configuration, UserService userService)
    {
        _configuration = configuration;
        _userService = userService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest body)
    {
        // Validar las credenciales del usuario
        var user = AuthenticateUser(body);
        if (user == null)
        {
            return Unauthorized();
        }

        // Generar el token JWT
        var token = GenerateJwtToken(user);

        // Crear la respuesta de login
        var response = new LoginResponse
        {
            Id = user.Id,
            Token = token,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Email = user.Email,
        };

        // Enviar la respuesta con el token y los datos del usuario
        return Ok(response);
    }

    [HttpPost("renewtoken")]
    public IActionResult Renewtoken([FromBody] RenewTokenRequest request)
    {

        var principal = ValidateJwtToken(request.Token);
        if (principal == null)
        {
            return Unauthorized();
        }

        var userIdClaim = principal.FindFirst("id")?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }


        var user = _userService.GetUserById(userId);
        if (user == null)
        {
            return Unauthorized();
        }

        var newToken = GenerateJwtToken(user);


        var response = new LoginResponse
        {
            Id = user.Id,
            Token = newToken,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Email = user.Email,
        };

        return Ok(response);
    }

    [HttpPost("validate")]
    public IActionResult Validate([FromBody] ValidateTokenRequest request)
    {

        var principal = ValidateJwtToken(request.Token);
        if (principal == null)
        {
            return Unauthorized();
        }

        var userIdClaim = principal.FindFirst("id")?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized();
        }


        var user = _userService.GetUserById(userId);
        if (user == null)
        {
            return Unauthorized();
        }


        var response = new LoginResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Email = user.Email,
        };

        return Ok(response);
    }

    private User AuthenticateUser(LoginRequest body)
    {
        // Validar el usuario utilizando el servicio UserService
        var user = _userService.GetUserByEmail(body.Email); // Asegúrate de tener este método en UserService
        if (user != null && user.Password == body.Password) // Aquí deberías usar un hash en lugar de comparar contraseñas directamente
        {
            return user;
        }

        return null;
    }

    private string GenerateJwtToken(User user)
    {
        // Se recomienda obtener la clave de configuración
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        var claims = new[]
        {
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddMinutes(60),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal ValidateJwtToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero // No permitir tiempo de tolerancia
        };

        try
        {
            return tokenHandler.ValidateToken(token, validationParameters, out _);
        }
        catch
        {
            return null; // El token no es válido
        }
    }
}
