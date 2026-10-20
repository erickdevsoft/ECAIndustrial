using ECA.Domain.Machines;
using Microsoft.EntityFrameworkCore;

namespace ECA.Infrastructure.Persistence
{
    public sealed class EcaDbContext : DbContext
    {
        public EcaDbContext(DbContextOptions<EcaDbContext> options)
            : base(options)
        {
        }

        public DbSet<Machine> Machines => Set<Machine>();


    }
}
