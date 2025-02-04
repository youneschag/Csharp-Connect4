using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Connect_API.Models
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options)
            : base(options)
        {
        }

        public DbSet<AspNetUser> AspNetUsers { get; set; }
        public DbSet<AspNetGame> AspNetGames { get; set; }
        public DbSet<AspNetGameMove> GameMoves { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=Connect.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Instancier PasswordHasher pour hacher les mots de passe
            var passwordHasher = new PasswordHasher<AspNetUser>();

            // Ajouter des données initiales pour AspNetUser
            modelBuilder.Entity<AspNetUser>().HasData(
                new AspNetUser
                {
                    Id = Guid.NewGuid(),
                    Username = "Younes",
                    PasswordHash = passwordHasher.HashPassword(null, "Younes123&"), // Hache le mot de passe
                    CreationDate = DateTime.UtcNow
                },
                new AspNetUser
                {
                    Id = Guid.NewGuid(),
                    Username = "Maria",
                    PasswordHash = passwordHasher.HashPassword(null, "Maria456@"), // Hache le mot de passe
                    CreationDate = DateTime.UtcNow
                }
            );

            // Ajouter des données initiales pour AspNetGame
            modelBuilder.Entity<AspNetGame>().HasData(
                new AspNetGame
                {
                    Id = Guid.NewGuid(),
                    HostName = "Younes",
                    GuestName = "Maria",
                    CurrentTurn = "red",
                    Status = AspNetGameStatus.InProgress,
                    GameCode = "gyxmd4c7m",
                    Winner = null,
                    CreationDate = DateTime.UtcNow,
                    ModificationDate = DateTime.UtcNow
                },
                new AspNetGame
                {
                    Id = Guid.NewGuid(),
                    HostName = "Younes",
                    GuestName = null,
                    CurrentTurn = "red",
                    Status = AspNetGameStatus.AwaitingGuest,
                    GameCode = "bga0dgz47",
                    Winner = null,
                    CreationDate = DateTime.UtcNow,
                    ModificationDate = DateTime.UtcNow
                }
            );
        }
    }
}