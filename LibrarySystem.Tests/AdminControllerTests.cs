using LibrarySystem.Controllers;
using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibrarySystem.Tests
{
    public class AdminControllerTests
    {
        private ApplicationDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private AdminController GetController(ApplicationDbContext db)
        {
            var controller = new AdminController(db);
            controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
            return controller;
        }

        [Fact]
        public async Task Books_ReturnsViewWithAllBooks()
        {
            var db = GetInMemoryDb();
            var author = new Author { FirstName = "Иван", LastName = "Вазов" };
            var genre = new Genre { Name = "Роман" };
            db.Authors.Add(author);
            db.Genres.Add(genre);
            db.Books.Add(new Book
            {
                Title = "Под игото",
                Author = author,
                Genre = genre,
                Year = 1894,
                TotalCopies = 3,
                AvailableCopies = 3
            });
            await db.SaveChangesAsync();

            var result = await GetController(db).Books() as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Book>>(result.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task DeleteGenre_WhenHasBooks_SetsErrorTempData()
        {
            var db = GetInMemoryDb();
            var author = new Author { FirstName = "Иван", LastName = "Вазов" };
            var genre = new Genre { Id = 1, Name = "Роман" };
            db.Authors.Add(author);
            db.Genres.Add(genre);
            db.Books.Add(new Book
            {
                Title = "Под игото",
                Author = author,
                Genre = genre,
                Year = 1894,
                TotalCopies = 3,
                AvailableCopies = 3
            });
            await db.SaveChangesAsync();

            var controller = GetController(db);
            await controller.DeleteGenre(1);

            Assert.Equal("Не може да изтриеш жанр, към който има свързани книги.",
                controller.TempData["Error"]);
        }

        [Fact]
        public async Task DeleteAuthor_WhenHasBooks_SetsErrorTempData()
        {
            var db = GetInMemoryDb();
            var author = new Author { Id = 1, FirstName = "Иван", LastName = "Вазов" };
            var genre = new Genre { Name = "Роман" };
            db.Authors.Add(author);
            db.Genres.Add(genre);
            db.Books.Add(new Book
            {
                Title = "Под игото",
                Author = author,
                Genre = genre,
                Year = 1894,
                TotalCopies = 3,
                AvailableCopies = 3
            });
            await db.SaveChangesAsync();

            var controller = GetController(db);
            await controller.DeleteAuthor(1);

            Assert.Equal("Не може да изтриеш автор, към който има свързани книги.",
                controller.TempData["Error"]);
        }

        [Fact]
        public async Task CreateBook_ValidModel_RedirectsToBooks()
        {
            var db = GetInMemoryDb();
            var author = new Author { Id = 1, FirstName = "Иван", LastName = "Вазов" };
            var genre = new Genre { Id = 1, Name = "Роман" };
            db.Authors.Add(author);
            db.Genres.Add(genre);
            await db.SaveChangesAsync();

            var controller = GetController(db);
            var book = new Book
            {
                Title = "Под игото",
                AuthorId = 1,
                GenreId = 1,
                Year = 1894,
                TotalCopies = 3
            };

            var result = await controller.CreateBook(book) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Books", result.ActionName);
            Assert.Equal(1, await db.Books.CountAsync());
        }
    }
}