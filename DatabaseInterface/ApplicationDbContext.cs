using DatabaseInterface.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterface
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

//        public DbSet<ApplicationUser> Users { get; set; }

        public DbSet<Settings> Settings { get; set; }
        public DbSet<RaspberryPi> RaspberryPis { get; set; }
        public DbSet<TenSecondMetric> TenSecondMetrics { get; set; }
        public DbSet<MinuteMetric> MinuteMetrics { get; set; }
        public DbSet<HourMetric> HourMetrics { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<ApplicationUser>()
                .HasOne(a => a.RaspberryPi)
                .WithOne(r => r.User)
                .HasForeignKey<RaspberryPi>(r => r.UserId);
            
            builder.Entity<ApplicationUser>()
                .HasOne(a => a.Settings)
                .WithOne(r => r.User)
                .HasForeignKey<Settings>(r => r.UserId);
            
            builder.Entity<RaspberryPi>()
                .HasMany(r => r.HourMetrics)
                .WithOne(t => t.RaspberryPi)
                .HasForeignKey(t => t.RaspberryPiId);
            
            builder.Entity<RaspberryPi>()
                .HasMany(r => r.MinuteMetrics)
                .WithOne(t => t.RaspberryPi)
                .HasForeignKey(t => t.RaspberryPiId);
            
            builder.Entity<RaspberryPi>()
                .HasMany(r => r.TenSecondMetrics)
                .WithOne(t => t.RaspberryPi)
                .HasForeignKey(t => t.RaspberryPiId);

            builder.Entity<HourMetric>()
                .HasIndex(t => t.Created);

            builder.Entity<MinuteMetric>()
                .HasIndex(t => t.Created);

            builder.Entity<TenSecondMetric>()
                .HasIndex(t => t.Created);

            base.OnModelCreating(builder);
        }
    }
}