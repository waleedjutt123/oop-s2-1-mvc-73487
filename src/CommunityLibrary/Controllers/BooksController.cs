using CommunityLibrary.Data;
using CommunityLibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommunityLibrary.Controllers;

[Authorize(Roles = "Staff,Admin")]
public class BooksController : Controller
{
    private readonly ApplicationDbContext _context;

    public BooksController(ApplicationDbContext context)
    {
        _context = context;
    }

    
    public async Task<IActionResult> Index(string? searchTerm, string? category, string? availability)
    {
        var query = _context.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(b => b.Title.Contains(term) || b.Author.Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(b => b.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(availability) && !string.Equals(availability, "All", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(availability, "Available", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(b => b.IsAvailable);
            }
            else if (string.Equals(availability, "OnLoan", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(b => !b.IsAvailable);
            }
        }

        var categories = await _context.Books
            .Select(b => b.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var model = new BookIndexViewModel
        {
            Books = await query
                .OrderBy(b => b.Title)
                .ToListAsync(),
            SearchTerm = searchTerm,
            Category = category,
            Availability = availability,
            Categories = categories
        };

        return View(model);
    }

    
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .FirstOrDefaultAsync(m => m.Id == id);
        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    
    public IActionResult Create()
    {
        return View();
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Author,Isbn,Category,IsAvailable")] Book book)
    {
        if (ModelState.IsValid)
        {
            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(book);
    }

    
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }
        return View(book);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Isbn,Category,IsAvailable")] Book book)
    {
        if (id != book.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(book);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(book.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(book);
    }

    
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var book = await _context.Books
            .FirstOrDefaultAsync(m => m.Id == id);
        if (book == null)
        {
            return NotFound();
        }

        return View(book);
    }

    
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book != null)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool BookExists(int id)
    {
        return _context.Books.Any(e => e.Id == id);
    }
}

