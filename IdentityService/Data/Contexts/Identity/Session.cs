namespace BN.PROJECT.IdentityService;

public class Session
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid SessionId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime LastActive { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime ExpiresAt { get; set; }

    [Required]
    public DateTime SignedOutAt { get; set; }

    // Navigation property
    [ForeignKey("UserId")]
    public User User { get; set; }
}
