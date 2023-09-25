using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebApplicationRestfulApi.Authentication;

namespace WebApplicationRestfulApi.Context
{
  public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
  {
    private readonly IConfiguration _configuration;
    public ApplicationDbContext(IConfiguration configuration, DbContextOptions<ApplicationDbContext> options) { 
      _configuration = configuration;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
      base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseSqlServer(_configuration.GetConnectionString("ConnStr"));
      base.OnConfiguring(optionsBuilder);
    }
  }
}
