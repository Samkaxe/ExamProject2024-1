using System.ComponentModel.DataAnnotations;

namespace Auth.Application.DTOs;

public class RegisterModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
    [RegularExpression("^(?=.*[0-9])(?=.*[a-zA-Z])(?=.*\\W).*$", ErrorMessage = "Password must contain at least one number, one letter, and one special character")]
    public string Password { get; set; }
}
