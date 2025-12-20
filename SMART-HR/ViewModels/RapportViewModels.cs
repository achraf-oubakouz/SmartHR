using System;
using System.Collections.Generic;

namespace SmartHR.ViewModels
{
    /// <summary>
    /// ViewModel principal pour le tableau de bord des rapports
    /// </summary>
    public class RapportDashboardViewModel
    {
        // 1. Employés par département
        public List<EmployesParDepartementItem> EmployesParDepartement { get; set; } = new();
        public int TotalEmployes { get; set; }

        // 2. Taux d'utilisation des types de congés
        public List<UtilisationTypeCongeItem> UtilisationTypesConges { get; set; } = new();
        public int TotalDemandesConges { get; set; }

        // 3. Périodes avec le plus de demandes
        public List<PeriodePicItem> PeriodesPic { get; set; } = new();

        // 4. Tendances annuelles
        public List<TendanceAnnuelleItem> TendancesAnnuelles { get; set; } = new();
        public int AnneeSelectionnee { get; set; }
        public List<int> AnneesDisponibles { get; set; } = new();

        // Statistiques globales
        public int CongesEnAttente { get; set; }
        public int CongesApprouves { get; set; }
        public int CongesRefuses { get; set; }
        public double TauxApprobation { get; set; }
    }

    /// <summary>
    /// Nombre d'employés par département
    /// </summary>
    public class EmployesParDepartementItem
    {
        public string Departement { get; set; } = string.Empty;
        public int NombreEmployes { get; set; }
        public double Pourcentage { get; set; }
    }

    /// <summary>
    /// Taux d'utilisation par type de congé
    /// </summary>
    public class UtilisationTypeCongeItem
    {
        public string TypeConge { get; set; } = string.Empty;
        public int NombreDemandes { get; set; }
        public int JoursTotaux { get; set; }
        public double Pourcentage { get; set; }
    }

    /// <summary>
    /// Périodes avec le plus de demandes (par mois)
    /// </summary>
    public class PeriodePicItem
    {
        public int Mois { get; set; }
        public string NomMois { get; set; } = string.Empty;
        public int Annee { get; set; }
        public int NombreDemandes { get; set; }
        public bool EstPeriodePic { get; set; }
    }

    /// <summary>
    /// Tendances mensuelles pour une année
    /// </summary>
    public class TendanceAnnuelleItem
    {
        public int Mois { get; set; }
        public string NomMois { get; set; } = string.Empty;
        public int NombreDemandes { get; set; }
        public int JoursConges { get; set; }
        public double VariationPourcentage { get; set; } // Par rapport au mois précédent
        public string Tendance { get; set; } = string.Empty; // "hausse", "baisse", "stable"
    }
}

