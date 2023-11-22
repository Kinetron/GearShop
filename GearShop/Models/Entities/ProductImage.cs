using System;
using System.Collections.Generic;

namespace GearShop.Models.Entities;

public class ProductImage
{
    public long Id { get; set; }

    public string ImageName { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public int Deleted { get; set; }

    public DateTime? Created { get; set; }

    public string? Creator { get; set; }

    public DateTime? Changed { get; set; }

    public string? Changer { get; set; }
}
