using System;
using System.Collections.Generic;

namespace GearShop.Models.Entities;

public class OrderInfo
{
    public long Id { get; set; }

    public long OrderId { get; set; }

    public string? BuyerName { get; set; }

    public string? BuyerPhone { get; set; }

    public string? BuyerEmail { get; set; }

    /// <summary>
    /// Шифрованные пользовательские данные.
    /// </summary>
    public byte[]? BuyerInfo { get; set; }
}
