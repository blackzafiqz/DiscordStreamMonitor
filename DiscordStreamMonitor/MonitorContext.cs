using DiscordStreamMonitor.Model;
using Microsoft.EntityFrameworkCore;

namespace DiscordStreamMonitor
{
    public class MonitorContext : DbContext
    {
        
        public DbSet<User> Users { get; set; }
        public DbSet<Model.Stream> Streams { get; set; }
        public MonitorContext(DbContextOptions<MonitorContext> options)
        : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasMany(x => x.Streams)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId);
        }
    }
}
