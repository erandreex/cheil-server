using System.Collections.Generic;
using System.Linq;
using server.Models;

namespace server.Services
{
    public class UserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // Método para obtener todos los usuarios
        public List<User> GetAllUsers()
        {
            return _context.Users.ToList(); // Asegúrate de que 'Users' es el DbSet de usuarios en tu AppDbContext
        }

        // Método para obtener un usuario por ID
        public User GetUserById(int id)
        {
            return _context.Users.Find(id);
        }

        public User GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        // Método para crear un nuevo usuario
        public void CreateUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        // Método para actualizar un usuario existente
        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        // Método para eliminar un usuario
        public void DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}
