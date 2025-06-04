namespace BN.PROJECT.Core;

public class BrokerAccount
{
    public AccountStatusEnum AccountStatus { get; set; }
    public Guid UserId { get; set; }
    public Guid? AccountId { get; set; }
    public string? AccountNumber { get; set; }
    public decimal? AccruedFees { get; set; }
    public decimal? BuyingPower { get; set; }
    public DateTime? CreatedAtUtc { get; set; }


}
