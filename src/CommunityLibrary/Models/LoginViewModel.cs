using System.ComponentModel.DataAnnotations;

namespace CommunityLibrary.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Email required")]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Password required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
