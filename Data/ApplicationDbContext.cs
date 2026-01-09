using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using SGP_Freelancing.Models;

namespace SGP_Freelancing.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
    }
}
