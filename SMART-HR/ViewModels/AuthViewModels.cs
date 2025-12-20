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

    public class ProfileViewModel
    {
        public int Id { get; set; }
        public string Prenom { get; set; }
        public string Nom { get; set; }
        public string FullName => $"{Prenom} {Nom}";
        public string Email { get; set; }
        public string Role { get; set; }
        public bool Actif { get; set; }
        
        // Role-specific information
        public string Departement { get; set; }
        public string Poste { get; set; }
        public string Telephone { get; set; }
        public string EmailProfessionnel { get; set; }
        
        // For Employee
        public string ManagerName { get; set; }
        public int? ManagerId { get; set; }
        
        // For Manager
        public int EmployeeCount { get; set; }
        
        // Display properties
        public string RoleBadgeClass => Role switch
        {
            "Admin" => "badge bg-danger",
            "RH" => "badge bg-primary",
            "Manager" => "badge bg-warning text-dark",
            "Employe" => "badge bg-info",
            _ => "badge bg-secondary"
        };
        
        public string RoleDisplay => Role switch
        {
            "Admin" => "Administrateur",
            "RH" => "Ressources Humaines",
            "Manager" => "Manager",
            "Employe" => "Employé",
            _ => Role
        };
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Le mot de passe actuel est requis")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [MinLength(6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        public string ConfirmPassword { get; set; }
    }
}

