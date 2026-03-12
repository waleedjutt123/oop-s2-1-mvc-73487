using System.ComponentModel.DataAnnotations;

namespace CommunityLibrary.Models;

public class LoanCreateViewModel
{
    [Required]
    [Display(Name = "Member")]
    public int MemberId { get; set; }

    [Required]
    [Display(Name = "Book")]
    public int BookId { get; set; }

    public IEnumerable<Member> Members { get; set; } = Enumerable.Empty<Member>();
    public IEnumerable<Book> AvailableBooks { get; set; } = Enumerable.Empty<Book>();
}

