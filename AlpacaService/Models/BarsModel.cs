namespace BN.TRADER.AlpacaService
{
    public class BarsModel
    {
        public Dictionary<string, List<Bar>> Bars { get; set; }

        public class Bar
        {
            public decimal C { get; set; } // Close
            public decimal H { get; set; } // High
            public decimal L { get; set; } // Low
            public int N { get; set; }
            public decimal O { get; set; } // Open
            public DateTime T { get; set; } // Time
            public int V { get; set; } // Volume
            public double Vw { get; set; } // Volume Weighted Average Price
        }
    }
}