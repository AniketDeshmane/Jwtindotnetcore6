using APIwithSQLLite.Model;
using Microsoft.EntityFrameworkCore;

namespace APIwithSQLLite.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<UserDetails> Users => Set<UserDetails>();
    }
}
