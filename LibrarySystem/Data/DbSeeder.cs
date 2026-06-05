using LibrarySystem.Models;
using Microsoft.AspNetCore.Identity;

namespace LibrarySystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("User"))
                await roleManager.CreateAsync(new IdentityRole("User"));

            if (await userManager.FindByEmailAsync("admin@library.com") == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@library.com",
                    Email = "admin@library.com",
                    FirstName = "Admin",
                    LastName = "Library",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            if (!context.Genres.Any())
            {
                context.Genres.AddRange(
                    new Genre { Name = "Фантастика", Description = "Научна фантастика и фентъзи" },
                    new Genre { Name = "История", Description = "Исторически книги и биографии" },
                    new Genre { Name = "Програмиране", Description = "Технически книги" },
                    new Genre { Name = "Роман", Description = "Художествена литература" }
                );
                await context.SaveChangesAsync();
            }

            if (!context.Authors.Any())
            {
                context.Authors.AddRange(
                    new Author { FirstName = "Иван", LastName = "Вазов" },
                    new Author { FirstName = "Елин", LastName = "Пелин" },
                    new Author { FirstName = "Стивън", LastName = "Кинг" }
                );
                await context.SaveChangesAsync();
            }

            if (!context.Books.Any())
            {
                var vazov = context.Authors.First(a => a.LastName == "Вазов");
                var pelin = context.Authors.First(a => a.LastName == "Пелин");
                var king = context.Authors.First(a => a.LastName == "Кинг");
                var roman = context.Genres.First(g => g.Name == "Роман");
                var fantastika = context.Genres.First(g => g.Name == "Фантастика");

                context.Books.AddRange(
                    new Book { Title = "Под игото", AuthorId = vazov.Id, GenreId = roman.Id, Year = 1894, TotalCopies = 3, AvailableCopies = 3 },
                    new Book { Title = "Гераците", AuthorId = pelin.Id, GenreId = roman.Id, Year = 1911, TotalCopies = 2, AvailableCopies = 2 },
                    new Book { Title = "То", AuthorId = king.Id, GenreId = fantastika.Id, Year = 1986, TotalCopies = 4, AvailableCopies = 4 }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}