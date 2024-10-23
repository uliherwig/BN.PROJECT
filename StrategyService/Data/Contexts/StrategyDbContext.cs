namespace BN.PROJECT.StrategyService;

public class StrategyDbContext : DbContext
{
    public DbSet<BacktestSettings> BacktestSettings { get; set; }
    public DbSet<Position> Positions { get; set; }

    public StrategyDbContext(DbContextOptions<StrategyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BacktestSettings>()
          .HasIndex(b => b.Name)
          .IsUnique();
    }
}