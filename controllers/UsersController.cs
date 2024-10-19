using server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using server.Services;
using System.Linq;

[Route("/api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;

    public UsersController(UserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public ActionResult<List<User>> GetAllUsers(string searchUser = null, int pageNumber = 1, int pageSize = 10)
    {
        var users = _userService.GetAllUsers();

        if (!string.IsNullOrEmpty(searchUser))
        {
            users = users.Where(u => u.FirstName.Contains(searchUser, StringComparison.OrdinalIgnoreCase) ||
                                      u.Email.Contains(searchUser, StringComparison.OrdinalIgnoreCase))
                         .ToList();
        }

        var totalUsers = users.Count;

        var paginatedUsers = users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        var response = new
        {
            TotalUsers = totalUsers,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Users = paginatedUsers
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "User, Admin")]
    public ActionResult<User> GetUser(int id)
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
        {
            return Unauthorized();
        }

        if (currentUser.Role != "Admin" && currentUser.Id != id)
        {
            return Forbid();
        }

        var user = _userService.GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    [Authorize(Policy = "RequireAdmin")]
    public ActionResult<User> CreateUser([FromBody] User newUser)
    {
        // Generar un ID único
        newUser.Id = GenerateUniqueId();

        // Crear el usuario
        _userService.CreateUser(newUser);
        return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "RequireUserOrAdmin")]
    public IActionResult UpdateUser(int id, [FromBody] UserUpdate body)
    {
        if (body == null)
        {
            return BadRequest("Updated user data is null.");
        }

        var currentUser = GetCurrentUser();
        if (currentUser == null || (currentUser.Role != "Admin" && currentUser.Id != id))
        {
            return Forbid();
        }

        var user = _userService.GetUserById(id);
        if (user == null)
        {
            return NotFound();
        }

        user.FirstName = body.FirstName;
        user.LastName = body.LastName;
        user.Email = body.Email;

        _userService.UpdateUser(user);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "RequireAdmin")]
    public IActionResult DeleteUser(int id)
    {
        var user = _userService.GetUserById(id);
        if (user == null)
            return NotFound();

        _userService.DeleteUser(id);
        return NoContent();
    }

    private User GetCurrentUser()
    {
        var idClaim = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        return idClaim != null ? _userService.GetUserById(int.Parse(idClaim)) : null;
    }

    private int GenerateUniqueId()
    {

        var existingIds = _userService.GetAllUsers().Select(u => u.Id).ToList();


        if (existingIds.Count == 0)
        {
            return 1;
        }


        int maxId = existingIds.Max();

        return maxId + 1;
    }
}
