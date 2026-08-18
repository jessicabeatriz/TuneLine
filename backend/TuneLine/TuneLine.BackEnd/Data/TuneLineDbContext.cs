using Microsoft.EntityFrameworkCore;
using TuneLine.BackEnd.Models;

namespace TuneLine.BackEnd.Data
{
    public class TuneLineDbContext : DbContext
    {
        public TuneLineDbContext(DbContextOptions<TuneLineDbContext> options) : base(options) { }

        public DbSet<User> Users {  get; set; }
    }
}
