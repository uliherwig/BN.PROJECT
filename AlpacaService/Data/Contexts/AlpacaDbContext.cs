namespace BN.PROJECT.AlpacaService;

public class AlpacaDbContext : DbContext
{
    public DbSet<AlpacaAsset> Assets { get; set; }
    public DbSet<AlpacaOrder> Orders { get; set; }
    public DbSet<AlpacaBar> Bars { get; set; }
    public DbSet<AlpacaPosition> Positions { get; set; }
    public DbSet<UserSettings> UserSettings { get; set; }

    public AlpacaDbContext(DbContextOptions<AlpacaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);         
    }
}