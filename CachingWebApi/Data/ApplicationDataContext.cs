using CachingWebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace CachingWebApi.Data
{
    public class ApplicationDataContext : DbContext
    {

        public DbSet<Driver> Drivers { get; set; }

        public ApplicationDataContext(DbContextOptions<ApplicationDataContext> options) : base(options)
        {

        }
    }
}
