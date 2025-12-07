using System.ComponentModel.DataAnnotations;
using SmartHR.Models;

namespace SmartHR.ViewModels
{
    // Dashboard Statistics
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int AdminCount { get; set; }
        public int HRCount { get; set; }
        public int ManagerCount { get; set; }
        public int EmployeeCount { get; set; }
        public List<UserListItemViewModel> RecentUsers { get; set; } = new List<UserListItemViewModel>();
    }

    // User List Item for display
    public class UserListItemViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool Actif { get; set; }
        public string Status => Actif ? "Actif" : "Inactif";
        public string StatusClass => Actif ? "badge bg-success" : "badge bg-danger";
        public string RoleBadgeClass => Role switch
        {
            "Admin" => "badge bg-danger",
            "RH" => "badge bg-primary",
            "Manager" => "badge bg-warning",
            "Employe" => "badge bg-info",
            _ => "badge bg-secondary"
        };
    }

    // Create User ViewModel
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(50, ErrorMessage = "Le prénom ne peut pas dépasser 50 caractères")]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(50, ErrorMessage = "Le nom ne peut pas dépasser 50 caractères")]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Format d'email invalide")]
        [StringLength(100, ErrorMessage = "L'email ne peut pas dépasser 100 caractères")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le mot de passe est requis")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe")]
        public string MotDePasse { get; set; }

        [Required(ErrorMessage = "Le rôle est requis")]
        [Display(Name = "Rôle")]
        public string Role { get; set; }

        [Display(Name = "Compte actif")]
        public bool Actif { get; set; } = true;

        // Role-specific fields
        [StringLength(100)]
        [Display(Name = "Département")]
        public string Departement { get; set; }

        [StringLength(100)]
        [Display(Name = "Poste")]
        public string Poste { get; set; }

        [StringLength(15)]
        [Display(Name = "Téléphone")]
        public string Telephone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email professionnel")]
        public string EmailProfessionnel { get; set; }

        // For Employee role
        [Display(Name = "Manager")]
        public int? ManagerId { get; set; }
    }

    // Edit User ViewModel
    public class EditUserViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le prénom est requis")]
        [StringLength(50)]
        [Display(Name = "Prénom")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(50)]
        [Display(Name = "Nom")]
        public string Nom { get; set; }

        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Le rôle est requis")]
        [Display(Name = "Rôle")]
        public string Role { get; set; }

        [Display(Name = "Compte actif")]
        public bool Actif { get; set; }

        // Role-specific fields
        [StringLength(100)]
        [Display(Name = "Département")]
        public string Departement { get; set; }

        [StringLength(100)]
        [Display(Name = "Poste")]
        public string Poste { get; set; }

        [StringLength(15)]
        [Display(Name = "Téléphone")]
        public string Telephone { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email professionnel")]
        public string EmailProfessionnel { get; set; }

        // For Employee role
        [Display(Name = "Manager")]
        public int? ManagerId { get; set; }
    }

    // Reset Password ViewModel
    public class ResetPasswordViewModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "Le nouveau mot de passe est requis")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères")]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "La confirmation du mot de passe est requise")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Les mots de passe ne correspondent pas")]
        [Display(Name = "Confirmer le mot de passe")]
        public string ConfirmPassword { get; set; }
    }

    // User Details ViewModel
    public class UserDetailsViewModel
    {
        public int Id { get; set; }
        public string Prenom { get; set; }
        public string Nom { get; set; }
        public string FullName => $"{Prenom} {Nom}";
        public string Email { get; set; }
        public string Role { get; set; }
        public bool Actif { get; set; }
        public string StatusLabel => Actif ? "Actif" : "Inactif";
        
        // Role-specific details
        public string Departement { get; set; }
        public string Poste { get; set; }
        public string Telephone { get; set; }
        public string EmailProfessionnel { get; set; }
        
        // For Employee
        public string ManagerName { get; set; }
        public int? ManagerId { get; set; }
        
        // For Manager
        public int EmployeeCount { get; set; }
    }
}

