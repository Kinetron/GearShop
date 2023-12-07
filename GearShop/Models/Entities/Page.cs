using System;
using System.Collections.Generic;

namespace GearShop.Models.Entities;

public class Page
{
    public int Id { get; set; }

    public int? ParentId { get; set; }

    public string? Name { get; set; } = null!;

    public string? Title { get; set; }

    public string? TitleImage { get; set; }

    public string? Description { get; set; }
	public string Content { get; set; } = null!;

    public int Deleted { get; set; }

    public DateTime? Created { get; set; }

    public string? Creator { get; set; }

    public DateTime? Changed { get; set; }

    public string? Changer { get; set; }
}
