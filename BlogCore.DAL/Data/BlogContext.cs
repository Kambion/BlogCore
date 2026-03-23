namespace BlogCore.DAL.Data;

using BlogCore.DAL.Models;
using Microsoft.EntityFrameworkCore;

public class BlogContext : DbContext
{
    // Konstruktor pozwalaj¹cy na wstrzykniêcie konfiguracji (np. z Testcontainers) 
    public BlogContext(DbContextOptions<BlogContext> options) : base(options)
    {
    }

    // Definicje tabel w bazie danych 
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfiguracja modelu Post 
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id); // Definicja klucza g³ównego 

            // SQL Server sam generuje ID (zalecane przy Bogus):
            entity.Property(p => p.Id).ValueGeneratedOnAdd();

            entity.HasMany(p => p.Comments)
             .WithOne(c => c.Post)
             .HasForeignKey(c => c.PostId) // Klucz obcy w Comment
             .OnDelete(DeleteBehavior.Cascade); // Usuniêcie posta usuwa komentarze
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Id).ValueGeneratedOnAdd();
        });
    }
}

