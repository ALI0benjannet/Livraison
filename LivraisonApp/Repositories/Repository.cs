using LivraisonApp.Data;
using LivraisonApp.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LivraisonApp.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly AppDbContext Context;
    protected readonly DbSet<T> DbSet;

    public Repository(AppDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await DbSet.ToListAsync();
    public virtual async Task<T?> GetByIdAsync(int id) => await DbSet.FindAsync(id);
    public virtual async Task AddAsync(T entity) => await DbSet.AddAsync(entity);
    public virtual void Update(T entity) => DbSet.Update(entity);
    public virtual void Delete(T entity) => DbSet.Remove(entity);
}
