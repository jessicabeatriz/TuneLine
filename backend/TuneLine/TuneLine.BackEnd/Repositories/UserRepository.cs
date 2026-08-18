using Microsoft.EntityFrameworkCore;
using TuneLine.BackEnd.Data;
using TuneLine.BackEnd.Models;

namespace TuneLine.BackEnd.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly TuneLineDbContext _context;
        
        public UserRepository(TuneLineDbContext context)
        {
            _context = context;
        }
        public async Task<User?> GetByIdAsync(string id)
        {
            return await _context.Users.FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }
    }
}
