using System;
using System.Collections.Generic;

namespace GearShop.Models.Entities;

public partial class InfoSource
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int Deleted { get; set; }

    public DateTime? Created { get; set; }

    public string? Creator { get; set; }

    public DateTime? Changed { get; set; }

    public string? Changer { get; set; }
}
