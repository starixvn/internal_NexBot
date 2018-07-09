namespace NexBot.Models
{
    using Microsoft.EntityFrameworkCore;

    public class BotDbContext : DbContext
    {
        public BotDbContext()
        {
        }

        public BotDbContext(DbContextOptions<BotDbContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public virtual DbSet<MessageInfo> MessageInfo { get; set; }
    }
}