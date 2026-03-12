    using System.ComponentModel.DataAnnotations;

    namespace MyProject.Models.ViewModels.Auth
    {
        public class LoginViewModel
        {
            [Required(ErrorMessage = "Enter your email")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Enter your password")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }
    }