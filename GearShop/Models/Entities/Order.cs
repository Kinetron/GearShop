namespace GearShop.Models.Entities;

public class Order
{
    public long Id { get; set; }
    
    public long NonRegisteredBuyerId { get; set; }

    /// <summary>
    /// Общая сумма заказа.
    /// </summary>
    public decimal TotalAmount { get; set; }

	public int Deleted { get; set; }

    public DateTime? Created { get; set; }

    public string? Creator { get; set; }

    public DateTime? Changed { get; set; }

    public string? Changer { get; set; }

    /// <summary>
    /// Позиции(ссылки на продукты) в заказе.
    /// </summary>
    public List<OrderItem> OrderItems { get; set; }
}
