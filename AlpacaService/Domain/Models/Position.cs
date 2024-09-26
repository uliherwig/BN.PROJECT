namespace BN.PROJECT.AlpacaService;

public class Position
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public required string Symbol { get; set; }
    public int Quantity { get; set; }
    public Side Side { get; set; }
    public decimal PriceOpen { get; set; }
    public decimal PriceClose { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal StopLoss { get; set; }
    public DateTime StampOpened { get; set; }
    public DateTime StampClosed { get; set; }
    public string CloseSignal { get; set; } = "";
}
