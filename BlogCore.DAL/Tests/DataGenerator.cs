using BlogCore.DAL.Models;
using Bogus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogCore.DAL.Tests
{
    public static class DataGenerator
    {
        // Szablon dla modelu Post
        public static Faker<Post> GetPostFaker() => new Faker<Post>()
        .RuleFor(p => p.Author, f => f.Name.FullName()) // Generuje realistyczne imię i nazwisko
        .RuleFor(p => p.Content, f => f.Lorem.Paragraph()); // Generuje tekst typu Lorem Ipsum

        // Szablon dla modelu Comment
        public static Faker<Comment> GetCommentFaker(int postId) => new
        Faker<Comment>()
        .RuleFor(c => c.PostId, _ => postId) // Wiąże komentarz z konkretnym postem
        .RuleFor(c => c.Content, f => f.Lorem.Sentence()); // Generuje treść komentarza
    }

}
