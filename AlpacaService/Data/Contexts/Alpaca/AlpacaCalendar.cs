namespace BN.PROJECT.AlpacaService;

public class AlpacaCalendar
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }


    public DateOnly TradingDate { get; set; }


    public TimeSpan TradingOpen { get; set; }


    public TimeSpan TradingClose { get; set; }


    public TimeSpan SessionOpen { get; set; }


    public TimeSpan SessionClose { get; set; }



}

