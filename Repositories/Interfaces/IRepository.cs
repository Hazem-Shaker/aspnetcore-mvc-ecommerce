using e_commerce.Models;

namespace e_commerce.Repositories.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<T?> GetByIdAsync(int id);
        IQueryable<T> GetAll();
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
    }
}
