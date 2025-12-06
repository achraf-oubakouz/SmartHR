using System.Collections.Generic;
using SmartHR.Models;

namespace SmartHR.Services.Interfaces
{
    public interface IUtilisateurService
    {
        List<Utilisateur> GetAll();
        Utilisateur GetById(int id);
        Utilisateur GetByEmail(string email);
        bool EmailExists(string email);
        void Create(Utilisateur user);
        void Update(Utilisateur user);
        void Delete(int id);
        Utilisateur Authenticate(string email, string password);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}