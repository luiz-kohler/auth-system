using Auth_API.Infra;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Auth_API.Common
{
    public class BaseEntityRepository<T> : IBaseEntityRepository<T>
            where T : class, IBaseEntity, new()
    {
        protected readonly Context _context;

        public BaseEntityRepository(Context context)
        {
            _context = context;
        }

        public async Task Add(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public async Task Add(IEnumerable<T> entities)
        {
            await _context.Set<T>().AddRangeAsync(entities);
        }

        public async Task<IEnumerable<T>> AllIncluding(params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();

            foreach (var includeProperty in includeProperties)
                query = query.Include(includeProperty);

            return await query.ToListAsync();
        }

        public async Task Commit()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<int> Count()
        {
            return await _context.Set<T>().CountAsync();
        }

        public async Task<int> Count(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().CountAsync(predicate);
        }

        public async Task Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
            await Commit();
        }

        public async Task DeleteWhere(Expression<Func<T, bool>> predicate)
        {
            var entities = _context.Set<T>().Where(predicate);
            _context.Set<T>().RemoveRange(entities);
            await Commit();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().Where(predicate).ToListAsync();
        }

        public async Task<T> GetSingle(Expression<Func<T, bool>> predicate)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(predicate);
        }

        public async Task<T> GetSingle(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _context.Set<T>();

            foreach (var includeProperty in includeProperties)
                query = query.Include(includeProperty);

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task Update(T entity)
        {
            _context.Set<T>().Update(entity);
            await Commit();
        }
    }
}
