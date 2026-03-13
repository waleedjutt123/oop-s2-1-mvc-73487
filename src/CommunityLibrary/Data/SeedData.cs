using Bogus;
using CommunityLibrary.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CommunityLibrary.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        await EnsureRolesAndUsersAsync(userManager, roleManager);
        await SeedDomainDataAsync(context);
    }

    private static async Task EnsureRolesAndUsersAsync(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        const string adminRoleName = "Admin";
        const string staffRoleName = "Staff";

        if (!await roleManager.RoleExistsAsync(adminRoleName))
            await roleManager.CreateAsync(new IdentityRole(adminRoleName));
        if (!await roleManager.RoleExistsAsync(staffRoleName))
            await roleManager.CreateAsync(new IdentityRole(staffRoleName));

        const string adminEmail = "admin@community.local";
        const string adminPassword = "Admin123!";
        const string staffEmail = "staff@community.local";
        const string staffPassword = "Staff123!";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, adminPassword);
        }
        else
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
        }
        if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
            await userManager.AddToRoleAsync(adminUser, adminRoleName);

        var staffUser = await userManager.FindByEmailAsync(staffEmail);
        if (staffUser is null)
        {
            staffUser = new IdentityUser { UserName = staffEmail, Email = staffEmail, EmailConfirmed = true };
            await userManager.CreateAsync(staffUser, staffPassword);
        }
        else
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(staffUser);
            await userManager.ResetPasswordAsync(staffUser, token, staffPassword);
        }
        if (!await userManager.IsInRoleAsync(staffUser, staffRoleName))
            await userManager.AddToRoleAsync(staffUser, staffRoleName);
    }

    private static async Task SeedDomainDataAsync(ApplicationDbContext context)
    {
        if (await context.Books.AnyAsync() ||
            await context.Members.AnyAsync() ||
            await context.Loans.AnyAsync())
        {
            return;
        }

        var categories = new[] { "Fiction", "Non-fiction", "Science", "History", "Children", "Technology" };

        var bookFaker = new Faker<Book>()
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3, 3))
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.Isbn, f => f.Random.Replace("###-##########"))
            .RuleFor(b => b.Category, f => f.PickRandom(categories))
            .RuleFor(b => b.IsAvailable, true);

        var memberFaker = new Faker<Member>()
            .RuleFor(m => m.FullName, f => f.Name.FullName())
            .RuleFor(m => m.Email, f => f.Internet.Email())
            .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber("###########"));

        var books = bookFaker.Generate(20);
        var members = memberFaker.Generate(10);

        await context.Books.AddRangeAsync(books);
        await context.Members.AddRangeAsync(members);
        await context.SaveChangesAsync();

        var loans = new List<Loan>();
        var now = DateTime.Today;

        for (var i = 0; i < 15; i++)
        {
            var book = books[i % books.Count];
            var member = members[i % members.Count];

            var loanDate = now.AddDays(-new Random().Next(1, 30));
            var dueDate = loanDate.AddDays(14);

            DateTime? returnedDate = null;

            if (i < 5)
            {
                returnedDate = loanDate.AddDays(7);
            }
            else if (i < 10)
            {
                if (dueDate < now)
                {
                    returnedDate = null;
                }
            }

            if (returnedDate != null)
            {
                book.IsAvailable = true;
            }
            else
            {
                book.IsAvailable = false;
            }

            loans.Add(new Loan
            {
                BookId = book.Id,
                MemberId = member.Id,
                LoanDate = loanDate,
                DueDate = dueDate,
                ReturnedDate = returnedDate
            });
        }

        await context.Loans.AddRangeAsync(loans);
        await context.SaveChangesAsync();
    }
}

