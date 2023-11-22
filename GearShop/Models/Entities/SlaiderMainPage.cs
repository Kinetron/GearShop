using System;
using System.Collections.Generic;

namespace GearShop.Models.Entities;

public partial class SlaiderMainPage
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string FileName { get; set; } = null!;
}
