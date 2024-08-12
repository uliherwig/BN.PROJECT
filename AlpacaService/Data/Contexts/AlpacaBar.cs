namespace BN.TRADER.AlpacaService
{
    public class AlpacaBar
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public required string Symbol { get; set; }
        public decimal C { get; set; } // Close
        public decimal H { get; set; } // High
        public decimal L { get; set; } // Low
        public ulong N { get; set; }
        public decimal O { get; set; } // Open
        public DateTime T { get; set; } // Time
        public decimal V { get; set; } // Volume
        public decimal Vw { get; set; } // Volume Weighted Average Price
    }
}