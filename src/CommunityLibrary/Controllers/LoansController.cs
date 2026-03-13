using CommunityLibrary.Data;
using CommunityLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunityLibrary.Controllers;

[Authorize(Roles = "Staff,Admin")]
public class LoansController : Controller
{
    private readonly ApplicationDbContext _context;

    public LoansController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var loans = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();

        var overdueCount = loans.Count(l => l.ReturnedDate == null && l.DueDate < today);
        ViewData["OverdueCount"] = overdueCount;

        return View(loans);
    }

   
    public async Task<IActionResult> Create()
    {
        var model = new LoanCreateViewModel
        {
            Members = await _context.Members
                .OrderBy(m => m.FullName)
                .ToListAsync(),
            AvailableBooks = await GetAvailableBooksQuery().ToListAsync()
        };

        return View(model);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LoanCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Members = await _context.Members.OrderBy(m => m.FullName).ToListAsync();
            model.AvailableBooks = await GetAvailableBooksQuery().ToListAsync();
            return View(model);
        }

        var book = await _context.Books.FindAsync(model.BookId);
        var member = await _context.Members.FindAsync(model.MemberId);

        if (book == null || member == null)
        {
            return NotFound();
        }

        var hasActiveLoan = await _context.Loans
            .AnyAsync(l => l.BookId == model.BookId && l.ReturnedDate == null);

        if (hasActiveLoan || !book.IsAvailable)
        {
            ModelState.AddModelError(string.Empty, "This book is already on an active loan.");
            model.Members = await _context.Members.OrderBy(m => m.FullName).ToListAsync();
            model.AvailableBooks = await GetAvailableBooksQuery().ToListAsync();
            return View(model);
        }

        var today = DateTime.Today;

        var loan = new Loan
        {
            BookId = model.BookId,
            MemberId = model.MemberId,
            LoanDate = today,
            DueDate = today.AddDays(14),
            ReturnedDate = null
        };

        book.IsAvailable = false;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    
    public async Task<IActionResult> Return(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var loan = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
        {
            return NotFound();
        }

        if (loan.ReturnedDate != null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(loan);
    }

   
    [HttpPost, ActionName("Return")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReturnConfirmed(int id)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loan == null)
        {
            return NotFound();
        }

        if (loan.ReturnedDate == null)
        {
            loan.ReturnedDate = DateTime.Today;
            if (loan.Book != null)
            {
                loan.Book.IsAvailable = true;
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private IQueryable<Book> GetAvailableBooksQuery()
    {
        return _context.Books
            .Where(b => b.IsAvailable && !_context.Loans.Any(l => l.BookId == b.Id && l.ReturnedDate == null))
            .OrderBy(b => b.Title);
    }
}

