using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Books()
        {
            var books = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .ToListAsync();
            return View(books);
        }

        public async Task<IActionResult> CreateBook()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(Book book)
        {
            ModelState.Remove("Author");
            ModelState.Remove("Genre");

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(book);
            }

            try
            {
                book.AvailableCopies = book.TotalCopies;
                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Книгата беше добавена успешно.";
                return RedirectToAction(nameof(Books));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Неуспешно добавяне на книга. Моля опитай отново.";
                await PopulateDropdownsAsync();
                return View(book);
            }
        }

        public async Task<IActionResult> EditBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            await PopulateDropdownsAsync();
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(Book book)
        {
            ModelState.Remove("Author");
            ModelState.Remove("Genre");
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(book);
            }

            try
            {
                _context.Books.Update(book);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Книгата беше обновена успешно.";
                return RedirectToAction(nameof(Books));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Неуспешно редактиране. Моля опитай отново.";
                await PopulateDropdownsAsync();
                return View(book);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var hasLoans = await _context.BookLoans
                .AnyAsync(bl => bl.BookId == id && !bl.IsReturned);

            if (hasLoans)
            {
                TempData["Error"] = "Не може да изтриеш книга с активни заемания.";
                return RedirectToAction(nameof(Books));
            }

            try
            {
                var book = await _context.Books.FindAsync(id);
                if (book != null)
                {
                    _context.Books.Remove(book);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Книгата беше изтрита успешно.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Неуспешно изтриване. Книгата е свързана с други записи.";
            }

            return RedirectToAction(nameof(Books));
        }

        public async Task<IActionResult> Authors()
        {
            var authors = await _context.Authors.ToListAsync();
            return View(authors);
        }

        public IActionResult CreateAuthor() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAuthor(Author author)
        {
            if (!ModelState.IsValid) return View(author);

            try
            {
                _context.Authors.Add(author);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Авторът беше добавен успешно.";
                return RedirectToAction(nameof(Authors));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Неуспешно добавяне на автор. Моля опитай отново.";
                return View(author);
            }
        }

        public async Task<IActionResult> EditAuthor(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null) return NotFound();
            return View(author);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAuthor(Author author)
        {
            if (!ModelState.IsValid) return View(author);

            try
            {
                _context.Authors.Update(author);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Авторът беше обновен успешно.";
                return RedirectToAction(nameof(Authors));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Неуспешно редактиране. Моля опитай отново.";
                return View(author);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            var hasBooks = await _context.Books.AnyAsync(b => b.AuthorId == id);
            if (hasBooks)
            {
                TempData["Error"] = "Не може да изтриеш автор, към който има свързани книги.";
                return RedirectToAction(nameof(Authors));
            }

            try
            {
                var author = await _context.Authors.FindAsync(id);
                if (author != null)
                {
                    _context.Authors.Remove(author);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Авторът беше изтрит успешно.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Неуспешно изтриване. Авторът е свързан с други записи.";
            }

            return RedirectToAction(nameof(Authors));
        }

        public async Task<IActionResult> Genres()
        {
            var genres = await _context.Genres.ToListAsync();
            return View(genres);
        }

        public IActionResult CreateGenre() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGenre(Genre genre)
        {
            if (!ModelState.IsValid) return View(genre);

            try
            {
                _context.Genres.Add(genre);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Жанрът беше добавен успешно.";
                return RedirectToAction(nameof(Genres));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Неуспешно добавяне на жанр. Моля опитай отново.";
                return View(genre);
            }
        }

        public async Task<IActionResult> EditGenre(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return NotFound();
            return View(genre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGenre(Genre genre)
        {
            if (!ModelState.IsValid) return View(genre);

            try
            {
                _context.Genres.Update(genre);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Жанрът беше обновен успешно.";
                return RedirectToAction(nameof(Genres));
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Неуспешно редактиране. Моля опитай отново.";
                return View(genre);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGenre(int id)
        {
            var hasBooks = await _context.Books.AnyAsync(b => b.GenreId == id);
            if (hasBooks)
            {
                TempData["Error"] = "Не може да изтриеш жанр, към който има свързани книги.";
                return RedirectToAction(nameof(Genres));
            }

            try
            {
                var genre = await _context.Genres.FindAsync(id);
                if (genre != null)
                {
                    _context.Genres.Remove(genre);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Жанрът беше изтрит успешно.";
                }
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Неуспешно изтриване. Жанрът е свързан с други записи.";
            }

            return RedirectToAction(nameof(Genres));
        }

        private async Task PopulateDropdownsAsync()
        {
            ViewBag.Authors = new SelectList(
                await _context.Authors.ToListAsync(), "Id", "LastName");
            ViewBag.Genres = new SelectList(
                await _context.Genres.ToListAsync(), "Id", "Name");
        }
    }
}