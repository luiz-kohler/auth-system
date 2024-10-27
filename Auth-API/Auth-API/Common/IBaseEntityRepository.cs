using System.Linq.Expressions;

namespace Auth_API.Common
{
    public interface IBaseEntityRepository<T> where T : class, IBaseEntity, new()
    {
        Task<IEnumerable<T>> GetAll();
        Task<IEnumerable<T>> GetAll(Expression<Func<T, bool>> predicate);
        Task<int> Count();
        Task<int> Count(Expression<Func<T, bool>> predicate);
        Task<bool> Any(Expression<Func<T, bool>> predicate);
        Task<T> GetSingle(Expression<Func<T, bool>> predicate);
        Task<T> GetSingle(Expression<Func<T, bool>> predicate, params Expression<Func<T, object>>[] includeProperties);
        Task Add(T entity);
        Task Add(IEnumerable<T> entities);
        Task Update(T entity);
        Task Delete(T entity);
        Task DeleteWhere(Expression<Func<T, bool>> predicate);
        Task Commit();
    }
}
