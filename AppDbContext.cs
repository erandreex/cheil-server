using Microsoft.EntityFrameworkCore;


namespace server.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } // Agrega DbSet para la entidad User

        // Puedes agregar más DbSet para otras entidades aquí
    }
}
