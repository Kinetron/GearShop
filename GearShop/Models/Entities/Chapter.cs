using System;
using System.Collections.Generic;

namespace GearShop.Models.Entities;

public partial class Chapter
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string Title { get; set; } = null!;

    public string? TitleImage { get; set; } = null!;

    public string Content { get; set; } = null!;

    public int Deleted { get; set; }

    public DateTime? Created { get; set; }

    public string? Creator { get; set; }

    public DateTime? Changed { get; set; }

    public string? Changer { get; set; }
}
