// Data/SmartWarehouseDbContext.cs
using Microsoft.EntityFrameworkCore;
using SmartWarehouse.API.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SmartWarehouse.API.Data;

public class SmartWarehouseDbContext : DbContext
{
    public SmartWarehouseDbContext(DbContextOptions<SmartWarehouseDbContext> options)
        : base(options) { }

    // DbSet'ler
    public DbSet<ProductCategory> ProductCategories { get; set; }
    public DbSet<WarehouseZone> WarehouseZones { get; set; }
    public DbSet<WarehouseRack> WarehouseRacks { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<WarehouseStock> WarehouseStocks { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── ProductCategory ──────────────────────────────────────────────
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ColorCode).HasMaxLength(20).HasDefaultValue("#607D8B");
            entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(100);

            // Aynı şirkette aynı kategori adı olamaz (soft-delete dışındakiler)
            entity.HasIndex(e => new { e.CompanyId, e.Name, e.IsDeleted })
                  .HasDatabaseName("IX_ProductCategory_CompanyId_Name");

            // Global query filter: IsDeleted = false olanları otomatik filtrele
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ── WarehouseZone ────────────────────────────────────────────────
        modelBuilder.Entity<WarehouseZone>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ZoneCode).IsRequired().HasMaxLength(20);
            entity.Property(e => e.ZoneName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TemperatureRequirement).HasMaxLength(50);

            // Aynı şirkette aynı zone kodu olamaz
            entity.HasIndex(e => new { e.CompanyId, e.ZoneCode, e.IsDeleted })
                  .HasDatabaseName("IX_WarehouseZone_CompanyId_ZoneCode");

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ── WarehouseRack ────────────────────────────────────────────────
        modelBuilder.Entity<WarehouseRack>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RackCode).IsRequired().HasMaxLength(30);
            entity.Property(e => e.RackName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RackType).HasMaxLength(50).HasDefaultValue("Standard");
            entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(100);

            // Aynı şirkette aynı raf kodu olamaz
            entity.HasIndex(e => new { e.CompanyId, e.RackCode, e.IsDeleted })
                  .HasDatabaseName("IX_WarehouseRack_CompanyId_RackCode");

            // İlişki: Raf → Bölge (Zone silinince raf silinmez, güvenli)
            entity.HasOne(r => r.WarehouseZone)
                  .WithMany(z => z.Racks)
                  .HasForeignKey(r => r.WarehouseZoneId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ── Product ──────────────────────────────────────────────────────
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SKU).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(30).HasDefaultValue("Adet");
            entity.Property(e => e.Barcode).HasMaxLength(50);
            entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,4)");

            // CompanyId bazında SKU unique olmalı
            entity.HasIndex(e => new { e.CompanyId, e.SKU, e.IsDeleted })
                  .IsUnique()
                  .HasDatabaseName("IX_Product_CompanyId_SKU");

            // İlişki: Ürün → Kategori
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.ProductCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ── WarehouseStock ───────────────────────────────────────────────
        modelBuilder.Entity<WarehouseStock>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(100);

            // Hesaplanan alan DB'ye yazılmaz
            entity.Ignore(e => e.AvailableQuantity);

            // Aynı raf + ürün kombinasyonu şirket bazında unique
            entity.HasIndex(e => new { e.CompanyId, e.ProductId, e.WarehouseRackId, e.IsDeleted })
                  .IsUnique()
                  .HasDatabaseName("IX_WarehouseStock_CompanyId_Product_Rack");

            entity.HasOne(s => s.Product)
                  .WithMany(p => p.Stocks)
                  .HasForeignKey(s => s.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.WarehouseRack)
                  .WithMany(r => r.Stocks)
                  .HasForeignKey(s => s.WarehouseRackId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ── InventoryTransaction ─────────────────────────────────────────
        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
            entity.Property(e => e.CompanyId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UnitCost).HasColumnType("decimal(18,4)");

            // Hesaplanan alan DB'ye yazılmaz
            entity.Ignore(e => e.TotalCost);

            // TransactionCode şirket bazında unique
            entity.HasIndex(e => new { e.CompanyId, e.TransactionCode })
                  .IsUnique()
                  .HasDatabaseName("IX_InventoryTransaction_CompanyId_TransactionCode");

            entity.HasOne(t => t.Product)
                  .WithMany(p => p.Transactions)
                  .HasForeignKey(t => t.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(t => t.WarehouseRack)
                  .WithMany(r => r.Transactions)
                  .HasForeignKey(t => t.WarehouseRackId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Transaction kayıtları soft-delete filtresinden MUAF tutulur
            // (Audit trail bozulmamalı, sadece IsDeleted manuel kullanılır)
            entity.HasQueryFilter(e => !e.IsDeleted);
        });
    }
}