using System;
using System.Collections.Generic;

namespace GearShop.Models.Entities;

/// <summary>
/// Данные магазина. Торгующего рядом продуктов.
/// </summary>
public class Shop
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
}
