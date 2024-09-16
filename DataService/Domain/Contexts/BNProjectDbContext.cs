namespace BN.PROJECT.DataService
{
    public class BNProjectDbContext : DbContext
    {
        public DbSet<AlpacaAsset> Assets { get; set; }
        public DbSet<AlpacaOrder> Orders { get; set; }
        public DbSet<AlpacaBar> Bars { get; set; }
        public DbSet<AlpacaPosition> Positions { get; set; }

        public DbSet<User> Users { get; set; }

        public BNProjectDbContext(DbContextOptions<BNProjectDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);         
        }
    }
}