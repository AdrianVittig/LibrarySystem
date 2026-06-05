using LibrarySystem.Controllers;
using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace LibrarySystem.Tests
{
    public class BookControllerTests
    {
        private ApplicationDbContext GetInMemoryDb()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Index_ReturnsViewWithBooks()
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

            var controller = new BookController(db, GetMockUserManager().Object);

            var result = await controller.Index(null, null) as ViewResult;

            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Book>>(result.Model);
            Assert.Single(model);
        }

        [Fact]
        public async Task Loan_WhenBookNotAvailable_RedirectsToIndex()
        {
            var db = GetInMemoryDb();
            var author = new Author { FirstName = "Иван", LastName = "Вазов" };
            var genre = new Genre { Name = "Роман" };
            db.Authors.Add(author);
            db.Genres.Add(genre);
            db.Books.Add(new Book
            {
                Id = 1,
                Title = "Под игото",
                Author = author,
                Genre = genre,
                Year = 1894,
                TotalCopies = 1,
                AvailableCopies = 0
            });
            await db.SaveChangesAsync();

            var userManager = GetMockUserManager();
            userManager.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("test-user-id");

            var controller = new BookController(db, userManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.Loan(1) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task Loan_WhenAlreadyLoaned_RedirectsToIndex()
        {
            var db = GetInMemoryDb();
            var author = new Author { FirstName = "Иван", LastName = "Вазов" };
            var genre = new Genre { Name = "Роман" };
            db.Authors.Add(author);
            db.Genres.Add(genre);
            db.Books.Add(new Book
            {
                Id = 1,
                Title = "Под игото",
                Author = author,
                Genre = genre,
                Year = 1894,
                TotalCopies = 3,
                AvailableCopies = 3
            });
            db.BookLoans.Add(new BookLoan
            {
                BookId = 1,
                UserId = "test-user-id",
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                IsReturned = false
            });
            await db.SaveChangesAsync();

            var userManager = GetMockUserManager();
            userManager.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("test-user-id");

            var controller = new BookController(db, userManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await controller.Loan(1) as RedirectToActionResult;

            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
        }

        [Fact]
        public async Task Return_WhenLoanExists_MarksAsReturned()
        {
            var db = GetInMemoryDb();
            var author = new Author { FirstName = "Иван", LastName = "Вазов" };
            var genre = new Genre { Name = "Роман" };
            db.Authors.Add(author);
            db.Genres.Add(genre);
            db.Books.Add(new Book
            {
                Id = 1,
                Title = "Под игото",
                Author = author,
                Genre = genre,
                Year = 1894,
                TotalCopies = 3,
                AvailableCopies = 2
            });
            db.BookLoans.Add(new BookLoan
            {
                BookId = 1,
                UserId = "test-user-id",
                LoanDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(14),
                IsReturned = false
            });
            await db.SaveChangesAsync();

            var userManager = GetMockUserManager();
            userManager.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .Returns("test-user-id");

            var controller = new BookController(db, userManager.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            await controller.Return(1);

            var loan = await db.BookLoans.FirstAsync();
            Assert.True(loan.IsReturned);
            Assert.NotNull(loan.ReturnDate);
        }
    }
}