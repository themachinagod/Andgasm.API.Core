using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Andgasm.API.Core
{
    public static class DbContextExtensions
    {
        public static async Task<TTarget> SetNullableNavigationProperty<TEntity, TTarget>(this DbContext dbcontext, TEntity entity, Expression<Func<TEntity, TTarget>> navigationProperty,
                                                                                                                                    Expression<Func<TTarget, bool>> query) where TEntity : class
                                                                                                                                                                           where TTarget : class
        {
            MemberExpression member = query.Body as MemberExpression;
            PropertyInfo pi = member.Member as PropertyInfo;
            var prevValue = (TTarget)pi.GetValue(entity);
            return await dbcontext.Set<TTarget>().FirstOrDefaultAsync(query);
        }

        public static async Task<bool> EntityExists<T>(this DbContext dbcontext, int id) where T : class
        {
            return await dbcontext.FindAsync<T>(id) != null;
        }

        public static async Task<T> GetEntityById<T>(this DbContext dbcontext, int id) where T : class
        {
            return await dbcontext.FindAsync<T>(id);
        }
    }
}
