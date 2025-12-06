using System.ComponentModel.DataAnnotations;

namespace SmartHR.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Le prénom est requis")]
        [MaxLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        [Display(Name = "Prénom")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [MaxLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        [Display(Name = "Nom")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        [Display(Name = "Email professionnel")]
        public string Email { get; set; }

        [Required(ErrorMessage = "L'entreprise est requise")]
        [MaxLength(100, ErrorMessage = "Le nom d'entreprise ne peut pas dépasser 100 caractères")]
        [Display(Name = "Entreprise")]
        public string Company { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [MinLength(8, ErrorMessage = "Le mot de passe doit contenir au moins 8 caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string Password { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "Vous devez accepter les conditions d'utilisation")]
        public bool AcceptTerms { get; set; }
    }
}

