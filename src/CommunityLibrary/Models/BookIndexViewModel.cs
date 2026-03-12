namespace CommunityLibrary.Models;

public class BookIndexViewModel
{
    public IEnumerable<Book> Books { get; set; } = Enumerable.Empty<Book>();

    public string? SearchTerm { get; set; }
    public string? Category { get; set; }
    public string? Availability { get; set; }

    public List<string> Categories { get; set; } = new();
}

