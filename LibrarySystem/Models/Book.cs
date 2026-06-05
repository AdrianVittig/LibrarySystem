using System.ComponentModel.DataAnnotations;

namespace LibrarySystem.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(13)]
        public string? ISBN { get; set; }

        public int Year { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public int TotalCopies { get; set; } = 1;

        public int AvailableCopies {  get; set; }

        public bool IsAvailable => AvailableCopies > 0;

        public int AuthorId { get; set; }
        public Author Author { get; set; } = null!;

        public int GenreId { get; set; }
        public Genre Genre { get; set; } = null!;

        public ICollection<BookLoan> BookLoans { get; set; } = new List<BookLoan>();
    }
}
