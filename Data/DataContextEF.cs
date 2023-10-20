using DotnetAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DotnetAPI.Data
{
    public class DataContextEF : DbContext
    {
        private readonly IConfiguration _config;
        public DataContextEF(IConfiguration config)
        {
            _config = config;
        }

        // to map model back to table in db
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserJobInfo> UserJobInfo { get; set; }
        public virtual DbSet<UserSalary> UsersSalary { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // To have access to right schema
            modelBuilder.HasDefaultSchema("TutorialAppSchema"); //Need that .Relational package for this function

            // To have access to right table
            modelBuilder.Entity<User>()
                .ToTable("Users", "TutorialAppSchema") // Here model name is User but table name is Users that's why using ToTable
                .HasKey(u => u.UserId);

            modelBuilder.Entity<UserSalary>()
                .HasKey(u => u.UserId);

            modelBuilder.Entity<UserJobInfo>()
                .HasKey(u => u.UserId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // .SqlServer package for this function
                optionsBuilder.UseSqlServer(_config.GetConnectionString("DefaultConnection"), options => options.EnableRetryOnFailure());
            }
        }

    }
}