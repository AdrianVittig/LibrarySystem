using LibrarySystem.Data;
using LibrarySystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Controllers
{
    public class BookController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? search, int? genreId)
        {
            var books =  _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                books = books.Where(b =>
                b.Title.Contains(search) ||
                b.Author.FirstName.Contains(search) ||
                b.Author.LastName.Contains(search));
            }

            if (genreId.HasValue)
            {
                books = books.Where(b => b.GenreId == genreId);
            }

            ViewBag.Genres = await _context.Genres.ToListAsync();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentGenre = genreId;

            return View(await books.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .FirstOrDefaultAsync(b => b.Id == id);

            if(book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Loan(int id)
        {
            var book = await _context.Books.FindAsync(id);

            if (book == null || !book.IsAvailable)
            {
                return RedirectToAction("Index");
            }

            var userId = _userManager.GetUserId(User);

            var alreadyLoaned = await _context.BookLoans
                .AnyAsync(bl => bl.BookId == id &&  bl.UserId == userId && !bl.IsReturned);

            if (alreadyLoaned)
            {
                return RedirectToAction("Index");
            }

            var rowsAffected = await _context.Books
                .Where(b => b.Id == id && b.AvailableCopies > 0)
                .ExecuteUpdateAsync(s => s.SetProperty(
                    b => b.AvailableCopies, b => b.AvailableCopies - 1));

            if(rowsAffected == 0)
            {
                return RedirectToAction("Index");
            }

            var loan = new BookLoan
            {
                BookId = id,
                UserId = userId,
                LoanDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(14),
                IsReturned = false
            };

            book.AvailableCopies--;
            _context.BookLoans.Add(loan);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Return(int id)
        {
            var userId = _userManager.GetUserId(User);

            var loan = await _context.BookLoans
                .Include(bl => bl.Book)
                .FirstOrDefaultAsync(bl => 
                bl.BookId == id &&
                !bl.IsReturned &&
                bl.UserId == userId);

            if(loan != null)
            {
                loan.IsReturned = true;
                loan.ReturnDate = DateTime.UtcNow;
                loan.Book.AvailableCopies++;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("MyLoans");
        }

        [Authorize]
        public async Task<IActionResult> MyLoans()
        {
            var userId = _userManager.GetUserId(User);

            var loans = await _context.BookLoans
                .Include(bl => bl.Book)
                    .ThenInclude(b => b.Author)
                .Where(bl => bl.UserId == userId)
                .OrderByDescending(bl => bl.LoanDate)
                .ToListAsync();

            return View(loans);
        }
    }
}