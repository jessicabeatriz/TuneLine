using TuneLine.BackEnd.Models;

namespace TuneLine.BackEnd.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
