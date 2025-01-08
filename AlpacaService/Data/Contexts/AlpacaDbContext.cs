namespace BN.PROJECT.AlpacaService;

public class AlpacaDbContext : DbContext
{
    public virtual DbSet<AlpacaAsset> Assets { get; set; }
    public virtual DbSet<AlpacaOrder> Orders { get; set; }
    public virtual DbSet<AlpacaBar> Bars { get; set; }
    public virtual DbSet<AlpacaPosition> Positions { get; set; }
    public virtual DbSet<AlpacaQuote> Quotes { get; set; }
    public virtual DbSet<UserSettings> UserSettings { get; set; }

    public AlpacaDbContext()
    { }

    public AlpacaDbContext(DbContextOptions<AlpacaDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}