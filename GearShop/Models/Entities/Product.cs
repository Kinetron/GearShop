using System;
using System.Collections.Generic;

namespace GearShop.Models.Entities;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public decimal? PurchaseCost { get; set; }

    public decimal? RetailCost { get; set; }

    public decimal? WholesaleCost { get; set; }

    public int? Rest { get; set; }

    public string? ImageName { get; set; }

    public string? Available { get; set; }

    public int Deleted { get; set; }

    public DateTime? Created { get; set; }

    public string? Creator { get; set; }

    public DateTime? Changed { get; set; }

    public string? Changer { get; set; }
}
