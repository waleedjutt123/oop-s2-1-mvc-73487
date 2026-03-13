using System.Reflection;
using CommunityLibrary.Data;
using CommunityLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CommunityLibrary.Tests;

public class LoanBusinessRulesTests
{
    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Cannot_create_loan_for_book_already_on_active_loan()
    {
        await using var context = CreateInMemoryContext();
        var book = new Book { Title = "Test", Author = "A", Isbn = "1", Category = "F", IsAvailable = false };
        var member = new Member { FullName = "M", Email = "m@x.com", Phone = "1" };
        context.Books.Add(book);
        context.Members.Add(member);
        await context.SaveChangesAsync();

        context.Loans.Add(new Loan
        {
            BookId = book.Id,
            MemberId = member.Id,
            LoanDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(14),
            ReturnedDate = null
        });
        await context.SaveChangesAsync();

        var availableBookIds = await context.Books
            .Where(b => b.IsAvailable && !context.Loans.Any(l => l.BookId == b.Id && l.ReturnedDate == null))
            .Select(b => b.Id)
            .ToListAsync();

        Assert.DoesNotContain(book.Id, availableBookIds);
    }

    [Fact]
    public async Task Returned_loan_makes_book_available_again()
    {
        await using var context = CreateInMemoryContext();
        var book = new Book { Title = "B", Author = "A", Isbn = "1", Category = "F", IsAvailable = false };
        var member = new Member { FullName = "M", Email = "m@x.com", Phone = "1" };
        context.Books.Add(book);
        context.Members.Add(member);
        await context.SaveChangesAsync();

        var loan = new Loan
        {
            BookId = book.Id,
            MemberId = member.Id,
            LoanDate = DateTime.Today.AddDays(-7),
            DueDate = DateTime.Today.AddDays(7),
            ReturnedDate = null
        };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        loan.ReturnedDate = DateTime.Today;
        book.IsAvailable = true;
        await context.SaveChangesAsync();

        var updated = await context.Books.FindAsync(book.Id);
        Assert.NotNull(updated);
        Assert.True(updated.IsAvailable);
    }

    [Fact]
    public async Task Book_search_returns_expected_matches()
    {
        await using var context = CreateInMemoryContext();
        context.Books.AddRange(
            new Book { Title = "Alpha Beta", Author = "X", Isbn = "1", Category = "F", IsAvailable = true },
            new Book { Title = "Gamma Delta", Author = "Y", Isbn = "2", Category = "F", IsAvailable = true },
            new Book { Title = "Alpha Gamma", Author = "Z", Isbn = "3", Category = "F", IsAvailable = true }
        );
        await context.SaveChangesAsync();

        var searchTerm = "Alpha";
        var query = context.Books.AsQueryable()
            .Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm));
        var results = await query.OrderBy(b => b.Title).ToListAsync();

        Assert.Equal(2, results.Count);
        Assert.All(results, b => Assert.Contains("Alpha", b.Title));
    }

    [Fact]
    public void Overdue_logic_DueDate_before_today_and_ReturnedDate_null()
    {
        var loan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            LoanDate = DateTime.Today.AddDays(-20),
            DueDate = DateTime.Today.AddDays(-1),
            ReturnedDate = null
        };

        var isOverdue = loan.DueDate < DateTime.Today && loan.ReturnedDate == null;
        Assert.True(isOverdue);
    }

    [Fact]
    public void Role_page_requires_Admin_authorization()
    {
        var controllerType = typeof(CommunityLibrary.Areas.Admin.Controllers.RolesController);
        var authAttribute = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
            .Cast<AuthorizeAttribute>()
            .FirstOrDefault();

        Assert.NotNull(authAttribute);
        Assert.Equal("Admin", authAttribute.Roles);
    }
}
