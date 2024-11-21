namespace BN.PROJECT.StrategyService;

public class StrategyDbContext : DbContext
{
    public DbSet<StrategySettingsModel> Strategies { get; set; }
    public DbSet<Position> Positions { get; set; }

    public StrategyDbContext(DbContextOptions<StrategyDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);   
    }
}