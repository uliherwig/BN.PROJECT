namespace BN.PROJECT.DataService;

public class Role
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid RoleId { get; set; }

    [Required]
    [MaxLength(100)]
    public string RoleName { get; set; }

    public string Description { get; set; }
}
