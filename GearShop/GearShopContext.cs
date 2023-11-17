using System;
using System.Collections.Generic;
using GearShop.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearShop;

public partial class GearShopContext : DbContext
{
    public GearShopContext()
    {
    }

    public GearShopContext(DbContextOptions<GearShopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-HDC13O8;Persist Security Info=False;TrustServerCertificate=true;Database=GearShop;User=sa;Password=232323;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.Available)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Changed).HasColumnType("datetime");
            entity.Property(e => e.Changer)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.Created).HasColumnType("datetime");
            entity.Property(e => e.Creator)
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.ImageName)
                .HasMaxLength(64)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(2048)
                .IsUnicode(false);
            entity.Property(e => e.PurchaseCost).HasColumnType("money");
            entity.Property(e => e.RetailCost).HasColumnType("money");
            entity.Property(e => e.WholesaleCost).HasColumnType("money");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
