namespace BN.PROJECT.AlpacaService
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<AlpacaAsset> Assets { get; set; }
        public DbSet<AlpacaOrder> Orders { get; set; }
        public DbSet<AlpacaBar> Bars { get; set; }
        public DbSet<AlpacaPosition> Positions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Weitere Konfigurationen
        }
    }
}