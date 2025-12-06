using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using SmartHR.Models;
using SmartHR;
using SmartHR.Services.Interfaces;

namespace SmartHR.Services.Implementations
{
    public class UtilisateurService : IUtilisateurService
    {
        private readonly ApplicationDbContext _context;

        public UtilisateurService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Utilisateur> GetAll()
        {
            return _context.Utilisateurs.ToList();
        }

        public Utilisateur GetById(int id)
        {
            return _context.Utilisateurs.FirstOrDefault(u => u.Id == id);
        }

        public Utilisateur GetByEmail(string email)
        {
            return _context.Utilisateurs.FirstOrDefault(u => u.Email.ToLower() == email.ToLower());
        }

        public bool EmailExists(string email)
        {
            return _context.Utilisateurs.Any(u => u.Email.ToLower() == email.ToLower());
        }

        public void Create(Utilisateur user)
        {
            _context.Utilisateurs.Add(user);
            _context.SaveChanges();
        }

        public void Update(Utilisateur user)
        {
            _context.Utilisateurs.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.Utilisateurs.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                _context.Utilisateurs.Remove(user);
                _context.SaveChanges();
            }
        }

        public Utilisateur Authenticate(string email, string password)
        {
            var user = GetByEmail(email);
            if (user == null || !user.Actif)
                return null;

            if (!VerifyPassword(password, user.MotDePasse))
                return null;

            return user;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = $"SmartHR_{password}_Salt2025";
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hashedPassword;
        }
    }
}
