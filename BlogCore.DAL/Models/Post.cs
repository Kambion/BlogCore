namespace BlogCore.DAL.Models;

using System.ComponentModel.DataAnnotations;

public class Post
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Author { get; set; } = string.Empty;
    [Required]
    public string Content { get; set; } = string.Empty;

    public List<Comment> Comments { get; set; } = new List<Comment>();
}
