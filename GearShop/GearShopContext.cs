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

    public virtual DbSet<SlaiderMainPage> SlaiderMainPages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-HDC13O8;Persist Security Info=False;TrustServerCertificate=true;Database=GearShop;User=sa;Password=232323;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SlaiderMainPage>(entity =>
        {
            entity.ToTable("SlaiderMainPage");

            entity.Property(e => e.Description)
                .HasMaxLength(1024)
                .IsUnicode(false);
            entity.Property(e => e.FileName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Title)
                .HasMaxLength(1024)
                .IsUnicode(false);
        });
        modelBuilder.HasSequence("SequenceOrderNumber");

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
