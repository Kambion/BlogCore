using BlogCore.DAL.Models;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogCore.DAL.Tests
{
    [TestClass]
    public class BlogRepositoryTests : IntegrationTestBase
    {
        [TestMethod]
        public async Task GetAllPosts_WhenDatabaseHasData_ReturnsAllRecords()
        {
            // Arrange: Generujemy i zapisujemy 5 losowych postów
            var fakePosts = DataGenerator.GetPostFaker().Generate(5);
            _context.Posts.AddRange(fakePosts);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker
            // Act
            var result = _repository.GetAllPosts(); // Pobranie danych przez repozytorium
            // Assert
            Assert.AreEqual(5, result.Count());
        }

        [TestMethod]
        public async Task AddOnePost_NumberOfPostsIncreasesByOne()
        {
            // Arrange: Generujemy i zapisujemy 5 losowych postów
            var fakePosts = DataGenerator.GetPostFaker().Generate(5);
            var newPost = DataGenerator.GetPostFaker().Generate(1).First();
            _context.Posts.AddRange(fakePosts);
            await _context.SaveChangesAsync(); // Zapis do kontenera Docker

            // Act
            var before = _repository.GetAllPosts().Count();
            _repository.AddPost(newPost);
            var after = _repository.GetAllPosts().Count();
            // Assert
            Assert.AreEqual(5, after - before);
        }

        [TestMethod]
        public async Task GetPostComments_WhenPostHasComments_ReturnTheseComments()
        {
            // Arrange: Generujemy i zapisujemy 5 losowych postów
            var post = DataGenerator.GetPostFaker().Generate(1).First();
            _context.Posts.Add(post);

            await _context.SaveChangesAsync();

            var fakeComments = DataGenerator.GetCommentFaker(post.Id).Generate(3);
            post.Comments.AddRange(fakeComments);

            await _context.SaveChangesAsync(); // Zapis do kontenera Docker

            // Act
            var comments = _repository.GetCommentsByPostId(post.Id);
            // Assert
            CollectionAssert.AreEqual(
                fakeComments.Select(c => c.Content).ToList(),
                comments.Select(c => c.Content).ToList()
            );
        }

        [TestMethod]
        public async Task GetAllPosts_EmptyDb_ReturnsZero()
        {
            //Arrange: Pusta baza

            //Act
            var posts = _repository.GetAllPosts();
            //Assert
            Assert.AreEqual(0, posts.Count());
        }

        [TestMethod]
        public async Task AddPost_LongContent_SavesCorrectly()
        {
            //Arrange: Długi content
            var longContentPost = new Faker<Post>()
                .RuleFor(p => p.Content, f => f.Lorem.Paragraphs(5)) // długa treść
                .RuleFor(p => p.Author, f => f.Person.FullName)
                .Generate();
            //Act
            _repository.AddPost(longContentPost);
            var postFromDb = _repository.GetAllPosts().FirstOrDefault(p => p.Id == longContentPost.Id);

            //Assert
            Assert.AreEqual(longContentPost.Content, postFromDb!.Content);
        }

        [TestMethod]
        public async Task AddPost_SpecialCharactersInAuthor_SavesCorrectly()
        {
            // Arrange: post z autorem zawierającym polskie znaki i symbole
            var specialAuthorPost = new Post
            {
                Content = "Treść testowa",
                Author = "Zażółć Gęślą Jaźń 123!"
            };

            // Act
            _repository.AddPost(specialAuthorPost);

            var postFromDb = _repository.GetAllPosts()
                .FirstOrDefault(p => p.Id == specialAuthorPost.Id);

            // Assert
            Assert.AreEqual("Zażółć Gęślą Jaźń 123!", postFromDb!.Author);
        }

        [TestMethod]
        public async Task AddComment_ValidData_IncreasesCountForPost()
        {
            // Arrange: dodajemy nowy post
            var post = DataGenerator.GetPostFaker().Generate(1).First();
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var comment = DataGenerator.GetCommentFaker(post.Id).Generate(1).First();

            // Act: dodaj komentarz
            _repository.AddComment(comment);

            var commentsFromDb = _repository.GetCommentsByPostId(post.Id);

            // Assert: dokładnie 1 komentarz
            Assert.AreEqual(1, commentsFromDb.Count());
        }

        [TestMethod]
        public async Task GetCommentsByPostId_NonExistentPost_ReturnsEmpty()
        {
            // Arrange: losowe ID posta, którego nie ma
            int nonExistentPostId = 999999;

            // Act
            var comments = _repository.GetCommentsByPostId(nonExistentPostId);

            // Assert: zwraca pustą kolekcję, nie null
            Assert.AreEqual(0, comments.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(Microsoft.EntityFrameworkCore.DbUpdateException))]
        public async Task AddComment_OrphanComment_ThrowsException()
        {
            // Arrange: komentarz z nieistniejącym PostId
            var orphanComment = new Comment
            {
                Content = "Niepoprawny komentarz",
                PostId = 999999 // brak takiego posta
            };

            // Act
            _repository.AddComment(orphanComment);

            //Assert
        }

        [TestMethod]
        public async Task MultipleComments_DifferentPosts_ReturnsOnlyCorrectOnes()
        {
            // Arrange: 2 posty
            var post1 = DataGenerator.GetPostFaker().Generate(1).First();
            var post2 = DataGenerator.GetPostFaker().Generate(1).First();
            _context.Posts.AddRange(post1, post2);
            await _context.SaveChangesAsync();

            // 5 komentarzy dla post1, 2 dla post2
            var commentsForPost1 = DataGenerator.GetCommentFaker(post1.Id).Generate(5);
            var commentsForPost2 = DataGenerator.GetCommentFaker(post2.Id).Generate(2);

            _context.Comments.AddRange(commentsForPost1);
            _context.Comments.AddRange(commentsForPost2);
            await _context.SaveChangesAsync();

            // Act
            var retrievedComments = _repository.GetCommentsByPostId(post1.Id);

            // Assert: tylko komentarze dla post1
            CollectionAssert.AreEqual(
                commentsForPost1.Select(c => c.Content).ToList(),
                retrievedComments.Select(c => c.Content).ToList()
            );
        }

        [TestMethod]
        [ExpectedException(typeof(Microsoft.EntityFrameworkCore.DbUpdateException))]
        public async Task AddPost_NullAuthor_ThrowsDbUpdateException()
        {
            // Arrange: post bez autora (null)
            var invalidPost = new Post
            {
                Content = "Treść testowa",
                Author = null! // celowe złamanie reguły [Required]
            };

            // Act
            _repository.AddPost(invalidPost);

            //Assert
        }

        [TestMethod]
        [ExpectedException(typeof(Microsoft.EntityFrameworkCore.DbUpdateException))]
        public async Task AddComment_NullContent_ThrowsDbUpdateException()
        {
            // Arrange: najpierw post (bo komentarz potrzebuje istniejącego PostId)
            var post = DataGenerator.GetPostFaker().Generate(1).First();
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var invalidComment = new Comment
            {
                Content = null!, // brak treści
                PostId = post.Id
            };

            // Act
            _repository.AddComment(invalidComment);

            //Assert
        }

        [TestMethod]
        public async Task DeletePost_CascadeDeleteComments()
        {
            // Arrange: dodaj post z komentarzami
            var post = DataGenerator.GetPostFaker().Generate(1).First();
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            var comments = DataGenerator.GetCommentFaker(post.Id).Generate(3);
            _context.Comments.AddRange(comments);
            await _context.SaveChangesAsync();

            // Act
            _repository.DeletePost(post);

            // Assert
            var remainingComments = _context.Comments.Where(c => c.PostId == post.Id).ToList();
            Assert.AreEqual(0, remainingComments.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(Microsoft.EntityFrameworkCore.DbUpdateException))]
        public async Task AddPost_NullContent_ThrowsDbUpdateException()
        {
            // Arrange: Tworzymy post bez wymaganej treści (Content)
            var invalidPost = new Post
            {
                Author = "Jan Kowalski",
                Content = null! // Celowe złamanie reguły [Required]
            };
            // Act: Próba dodania rekordu do bazy w kontenerze
            _repository.AddPost(invalidPost);
            // Assert: Atrybut ExpectedException automatycznie zweryfikuje wynik
        }
    }
}
