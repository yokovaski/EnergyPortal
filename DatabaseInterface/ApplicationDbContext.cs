using Microsoft.AspNetCore.Identity;
using DatabaseInterface.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DatabaseInterface
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
    {
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
                .Property(e => e.Id)
                .UseIdentityAlwaysColumn();
            
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

            // The existing schema uses varchar(128) for Identity login/token keys; newer Identity defaults to
            // unbounded text. Pin the lengths so the model keeps matching the migrations snapshot.
            builder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.Property(l => l.LoginProvider).HasMaxLength(128);
                b.Property(l => l.ProviderKey).HasMaxLength(128);
            });
            builder.Entity<IdentityUserToken<string>>(b =>
            {
                b.Property(t => t.LoginProvider).HasMaxLength(128);
                b.Property(t => t.Name).HasMaxLength(128);
            });
        }
    }
}