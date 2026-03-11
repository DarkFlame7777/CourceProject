using System.ComponentModel.DataAnnotations;

namespace MyProject.Models.ViewModels.Auth
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Enter your email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; }
    }
}