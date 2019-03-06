using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Andgasm.API.Core
{
    public class DynamicExpressionService
    {
        ILogger _logger;

        public DynamicExpressionService(ILogger<DynamicExpressionService> logger)
        {
            _logger = logger;
        }

        public IQueryable<T> DynamicWhere<T>(IQueryable<T> source, string propertypath, object filtervalue, FilterOperator filtertype)
        {
            ParameterExpression roottableExpression = Expression.Parameter(typeof(T), "p");
            MemberExpression propertyAccessExpression = CompilePropertyExpression<T>(propertypath, roottableExpression);
            Expression valueExpression = Expression.ConvertChecked(Expression.Constant(filtervalue), filtervalue.GetType()); // TODO: this is a potential failure point due to potential garbage on request! Need to test for or catch!
            return CompileWhereExpression(source, roottableExpression, propertyAccessExpression, valueExpression, filtertype);
        }

        public IQueryable<T> DynamicOrder<T>(IQueryable<T> source, string propertypath, SortDirection filtertype)
        {
            ParameterExpression roottableExpression = Expression.Parameter(typeof(T), "p");
            MemberExpression propertyAccessExpression = CompilePropertyExpression<T>(propertypath, roottableExpression);
            return CompileOrderExpression(source, roottableExpression, propertyAccessExpression, filtertype);
        }

        protected MemberExpression CompilePropertyExpression<T>(string propertypath, ParameterExpression roottable)
        {
            string[] columns = propertypath.Split('.');
            var property = typeof(T).GetProperty(columns[0]);
            var propertyAccess = Expression.MakeMemberAccess(roottable, property);
            if (columns.Length > 1)
            {
                for (int i = 1; i < columns.Length; i++)
                {
                    propertyAccess = Expression.MakeMemberAccess(propertyAccess, propertyAccess.Type.GetProperty(columns[i]));
                }
            }
            return propertyAccess;
        }

        protected string CompileFilterFunction(FilterOperator filterType)
        {
            string dataoperator = "Equal";
            switch (filterType)
            {
                case FilterOperator.neq:
                    dataoperator = "NotEqual";
                    break;
                case FilterOperator.eq:
                    dataoperator = "Equal";
                    break;
                case FilterOperator.lt:
                    dataoperator = "LessThan";
                    break;
                case FilterOperator.lte:
                    dataoperator = "LessThanOrEqual";
                    break;
                case FilterOperator.gt:
                    dataoperator = "GreaterThan";
                    break;
                case FilterOperator.gte:
                    dataoperator = "GreaterThanOrEqual";
                    break;
                case FilterOperator.contains:
                    dataoperator = "Contains";
                    break;
                case FilterOperator.startswith:
                    dataoperator = "StartsWith";
                    break;
                case FilterOperator.endswith:
                    dataoperator = "EndsWith";
                    break;
                default:
                    _logger.LogWarning($"Specified filter function '{filterType}' is not supported by the dynamic expression service.");
                    break;
            }
            return dataoperator;
        }

        protected IQueryable<T> CompileWhereExpression<T>(IQueryable<T> source, ParameterExpression roottable, MemberExpression propertyAccess, Expression valueExpression, FilterOperator filtertype)
        {
            Type[] types = new Type[2];
            types.SetValue(typeof(Expression), 0);
            types.SetValue(typeof(Expression), 1);
            string dataoperator = CompileFilterFunction(filtertype);
            var methodInfo = typeof(Expression).GetMethod(dataoperator, types);
            var expression = (BinaryExpression)methodInfo.Invoke(null, new object[] { propertyAccess, valueExpression });
            return source.Where(Expression.Lambda<Func<T, bool>>(expression, roottable));
        }

        protected IQueryable<T> CompileOrderExpression<T>(IQueryable<T> source, ParameterExpression roottable, MemberExpression propertyAccess, SortDirection sortdir)
        {
            // TODO: this is not how i want it done: tried with generics & expressions but hitting type issues
            //       we need to get this into one block and kill the ifs - issue is the return type of column needs to be set at runtime which the below wont support
            try
            {
                var t = propertyAccess.Type;
                if (t == typeof(string))
                {
                    switch (sortdir)
                    {
                        case SortDirection.asc:
                            return source.OrderBy(Expression.Lambda<Func<T, string>>(propertyAccess, roottable));
                        case SortDirection.desc:
                            return source.OrderByDescending(Expression.Lambda<Func<T, string>>(propertyAccess, roottable)); 
                        default:
                            return source;
                    }
                }
                else if (t == typeof(int))
                {
                    switch (sortdir)
                    {
                        case SortDirection.asc:
                            return source.OrderBy(Expression.Lambda<Func<T, int>>(propertyAccess, roottable));
                        case SortDirection.desc:
                            return source.OrderByDescending(Expression.Lambda<Func<T, int>>(propertyAccess, roottable)); 
                        default:
                            return source;
                    }
                }
                else if (t == typeof(int?))
                {
                    switch (sortdir)
                    {
                        case SortDirection.asc:
                            return source.OrderBy(Expression.Lambda<Func<T, int?>>(propertyAccess, roottable));
                        case SortDirection.desc:
                            return source.OrderByDescending(Expression.Lambda<Func<T, int?>>(propertyAccess, roottable)); 
                        default:
                            return source;
                    }
                }
                else if (t == typeof(decimal))
                {
                    switch (sortdir)
                    {
                        case SortDirection.asc:
                            return source.OrderBy(Expression.Lambda<Func<T, decimal>>(propertyAccess, roottable));
                        case SortDirection.desc:
                            return source.OrderByDescending(Expression.Lambda<Func<T, decimal>>(propertyAccess, roottable)); 
                        default:
                            return source;
                    }
                }
                else if (t == typeof(decimal?))
                {
                    switch (sortdir)
                    {
                        case SortDirection.asc:
                            return source.OrderBy(Expression.Lambda<Func<T, decimal?>>(propertyAccess, roottable));
                        case SortDirection.desc:
                            return source.OrderByDescending(Expression.Lambda<Func<T, decimal?>>(propertyAccess, roottable)); 
                        default:
                            return source;
                    }
                }
                else if (t == typeof(DateTime))
                {
                    switch (sortdir)
                    {
                        case SortDirection.asc:
                            return source.OrderBy(Expression.Lambda<Func<T, DateTime>>(propertyAccess, roottable));
                        case SortDirection.desc:
                            return source.OrderByDescending(Expression.Lambda<Func<T, DateTime>>(propertyAccess, roottable)); 
                        default:
                            return source;
                    }
                }
                else if (t == typeof(DateTime?))
                {
                    switch (sortdir)
                    {
                        case SortDirection.asc:
                            return source.OrderBy(Expression.Lambda<Func<T, DateTime?>>(propertyAccess, roottable));
                        case SortDirection.desc:
                            return source.OrderByDescending(Expression.Lambda<Func<T, DateTime?>>(propertyAccess, roottable));
                        default:
                            return source;
                    }
                }
                else if (t == typeof(bool))
                {
                    switch (sortdir)
                    {
                        case SortDirection.asc:
                            return source.OrderBy(Expression.Lambda<Func<T, bool>>(propertyAccess, roottable));
                        case SortDirection.desc:
                            return source.OrderByDescending(Expression.Lambda<Func<T, bool>>(propertyAccess, roottable));
                        default:
                            return source;
                    }
                }
                else if (t == typeof(bool?))
                {
                    switch (sortdir)
                    {
                        case SortDirection.asc:
                            return source.OrderBy(Expression.Lambda<Func<T, bool?>>(propertyAccess, roottable));
                        case SortDirection.desc:
                            return source.OrderByDescending(Expression.Lambda<Func<T, bool?>>(propertyAccess, roottable)); 
                        default:
                            return source;
                    }
                }
                return source;
            }
            catch(Exception ex)
            {
                return source;
            }
        }

        
    }
}
