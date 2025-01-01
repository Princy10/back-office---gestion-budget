using System.Security.Cryptography;
using System.Text;
using gestion_budget.DAL;
using gestion_budget.Models;
using Microsoft.EntityFrameworkCore;

namespace gestion_budget.Services
{
    public class UserService
    {
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(AppDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> RegisterAsync(string userName, string email, string password)
        {
            if (await _dbContext.Users.AnyAsync(u => u.Email == email))
            {
                return false; // Email déjà utilisé
            }

            var user = new User
            {
                UserName = userName,
                Email = email,
                PasswordHash = HashPassword(password),
                RoleId = 2 // Par défaut 'User'
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }
        public async Task<bool> LoginAsync(string email, string password)
        {
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == email);

            if (user == null || user.PasswordHash != HashPassword(password))
            {
                return false; // Identifiants invalides
            }

            var session = _httpContextAccessor.HttpContext.Session;
            session.SetInt32("UserId", user.UserId);
            session.SetString("UserName", user.UserName);
            session.SetInt32("RoleId", user.RoleId);

            return true;
        }

        public void Logout()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
        }

        public bool IsAuthenticated()
        {
            return _httpContextAccessor.HttpContext.Session.GetInt32("UserId") != null;
        }

        public int? GetAuthenticatedUserId()
        {
            return _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
        public string GetUserName()
        {
            return _httpContextAccessor.HttpContext.Session.GetString("UserName");
        }

        public int? GetUserId()
        {
            return _httpContextAccessor.HttpContext.Session.GetInt32("UserId");
        }

        public int? GetRoleId()
        {
            return _httpContextAccessor.HttpContext.Session.GetInt32("RoleId");
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _dbContext.Users.ToListAsync();
        }

        public async Task<List<User>> GetUserAdminAsync()
        {
            return await _dbContext.Users
                .Where(u => u.RoleId == 2)
                .ToListAsync();
        }
    }
}
