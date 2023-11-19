using System.ComponentModel.DataAnnotations;

namespace GearShop.Models.Entities;

/// <summary>
/// Не зарегистрированые покупатели.
/// </summary>
public class NonRegisteredBuyer
{
	[Key]
	public long ClusterId { get; set; }

    public Guid? BuyerGuid { get; set; }

    public string IpAddress { get; set; } = null!;

    public DateTime? Created { get; set; }

    public string? Creator { get; set; }

    public DateTime? Changed { get; set; }

    public string? Changer { get; set; }
}
