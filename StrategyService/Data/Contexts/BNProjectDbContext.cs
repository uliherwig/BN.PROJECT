namespace BN.PROJECT.StrategyService;

public class BNProjectDbContext : DbContext
{
 

    public BNProjectDbContext(DbContextOptions<BNProjectDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);         
    }
}