using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Core.Entities;

namespace TimeClockSystem.Infrastructure.Data.Context
{
    public class TimeClockDbContext : DbContext
    {
        public DbSet<TimeClockRecord> TimeClockRecords { get; set; }

        public TimeClockDbContext(DbContextOptions<TimeClockDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TimeClockRecord>(builder =>
            {
                builder.HasKey(r => r.Id);
                builder.Property(r => r.EmployeeId).IsRequired();
                builder.Property(r => r.Type).HasConversion<string>();
                builder.Property(r => r.Status).HasConversion<string>();
            });
            base.OnModelCreating(modelBuilder);
        }
    }
}
