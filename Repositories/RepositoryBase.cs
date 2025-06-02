using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories.Interfaces;

namespace Repositories
{
    public class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected AppDbContext _appDbContext { get; set; }

        public RepositoryBase(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public virtual async Task<IQueryable<T>> FindAll()
        {
            return await Task.Run(() => _appDbContext.Set<T>().AsNoTracking());
        }

        public async Task<IQueryable<T>> FindByCondition(Expression<Func<T, bool>> expression)
        {
            return await Task.Run(() => _appDbContext.Set<T>().Where(expression));
        }

        public async Task Create(T entity)
        {
            await _appDbContext.Set<T>().AddAsync(entity);
        }

        public async Task Update(T entity)
        {
            await Task.Run(() => _appDbContext.Set<T>().Update(entity));
        }

        public async Task Delete(T entity)
        {
            _appDbContext.Set<T>().Remove(entity);
            await Task.CompletedTask;
        }
    }
}
