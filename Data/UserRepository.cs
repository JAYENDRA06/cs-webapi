// Added AddScoped to builder in Program.cs for this
using DotnetAPI.Models;

namespace DotnetAPI.Data
{
    public class UserRepository : IUserRepository
    {
        readonly DataContextEF _enitiyFramework;
        public UserRepository(IConfiguration config)
        {
            _enitiyFramework = new DataContextEF(config);
        }

        public bool SaveChanges()
        {
            return _enitiyFramework.SaveChanges() > 0;
        }
        public void AddEntity<T>(T entityToAdd)
        {
            if (entityToAdd != null)
            {
                _enitiyFramework.Add(entityToAdd);
            }
        }
        public void RemoveEntity<T>(T entityToRemove)
        {
            if (entityToRemove != null)
            {
                _enitiyFramework.Remove(entityToRemove);
            }
        }
        public IEnumerable<User> GetUsers()
        {
            IEnumerable<User> users = _enitiyFramework.Users.ToList<User>();
            return users;
        }
        public User GetSingleUser(int userId)
        {
            User? user = _enitiyFramework.Users.Where(u => u.UserId == userId).FirstOrDefault<User>();
            if (user != null) return user;
            throw new Exception("Failed to get user");
        }
    }
}