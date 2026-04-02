// Repositories/BaseRepository.cs
using Microsoft.EntityFrameworkCore;
using SmartWarehouse.API.Data;
using SmartWarehouse.API.Entities;
using SmartWarehouse.API.Repositories.Interfaces;

namespace SmartWarehouse.API.Repositories;

/// <summary>
/// Generic repository implementasyonu.
/// TEntity : BaseEntity kısıtı sayesinde CompanyId ve IsDeleted
/// her zaman erişilebilir olur.
/// </summary>
public class BaseRepository<TEntity> : IBaseRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly SmartWarehouseDbContext _context;
    protected readonly DbSet<TEntity> _dbSet;

    public BaseRepository(SmartWarehouseDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, string companyId)
    {
        // Global query filter (IsDeleted) zaten uygulanmış durumda.
        // CompanyId kontrolü ek güvenlik katmanı olarak ekleniyor.
        return await _dbSet
            .FirstOrDefaultAsync(e => e.Id == id && e.CompanyId == companyId);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(string companyId)
    {
        return await _dbSet
            .Where(e => e.CompanyId == companyId)
            .ToListAsync();
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;

        // KIRMIZI ÇİZGİ: Güncelleme EntityState.Modified ile yapılır.
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public virtual async Task SoftDeleteAsync(Guid id, string companyId)
    {
        var entity = await GetByIdAsync(id, companyId);
        if (entity is null) return;

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;

        // Soft delete de EntityState.Modified ile yapılır
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public virtual async Task<bool> ExistsAsync(Guid id, string companyId)
    {
        return await _dbSet
            .AnyAsync(e => e.Id == id && e.CompanyId == companyId);
    }
}