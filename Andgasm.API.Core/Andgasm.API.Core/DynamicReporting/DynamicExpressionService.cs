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

        public IQueryable<T> DynamicWhere<T>(IQueryable<T> source, string columnName, object value, string filterType)
        {
            try
            {
                ParameterExpression table = Expression.Parameter(typeof(T), "obj");
                Expression column = Expression.PropertyOrField(table, columnName);
                Expression valueExpression = Expression.ConvertChecked(Expression.Constant(value), column.Type); // TODO: this is a failure point due to potential garbage on request! Need to test for or catch!
                Expression where = null;
                switch (filterType)
                {
                    case "neq":
                        where = Expression.NotEqual(column, valueExpression);
                        break;
                    case "eq":
                        where = Expression.Equal(column, valueExpression);
                        break;
                    case "lt":
                        where = Expression.LessThan(column, valueExpression);
                        break;
                    case "lte":
                        where = Expression.LessThanOrEqual(column, valueExpression);
                        break;
                    case "gt":
                        where = Expression.GreaterThan(column, valueExpression);
                        break;
                    case "gte":
                        where = Expression.GreaterThanOrEqual(column, valueExpression);
                        break;
                    case "contains":
                        where = ExpressionFunction<T>(table, columnName, "Contains", value.ToString()).Body;
                        break;
                    case "startswith":
                        where = ExpressionFunction<T>(table, columnName, "StartsWith", value.ToString()).Body;
                        break;
                    case "endswith":
                        where = ExpressionFunction<T>(table, columnName, "EndsWith", value.ToString()).Body;
                        break;
                    default:
                        _logger.LogWarning($"Specified filter function '{filterType}' is not supported by the dynamic expression service. Filter for field '{columnName}' has been ignored!");
                        return source;
                }
                Expression lambda = Expression.Lambda(where, new ParameterExpression[] { table });
                Type[] exprArgTypes = { source.ElementType };
                MethodCallExpression methodCall = Expression.Call(typeof(Queryable),
                                                                    "Where",
                                                                    exprArgTypes,
                                                                    source.Expression,
                                                                    lambda);
                return source.Provider.CreateQuery<T>(methodCall);
            }
            catch (Exception e)
            {
                // TODO: dont like this general catch all approach!
                _logger.LogError(e, $"An exception was encountered trying to perform a dynamic filter: chances are this is related to an invalid type cast!!");
                return source;
            }
        }

        public IQueryable<T> DynamicOrder<T>(IQueryable<T> source, string columnName, string direction)
        {
            var propertyInfo = typeof(T).GetProperty(columnName);
            if (propertyInfo == null) return source;
            switch (direction)
            {
                case "asc":
                    return source.OrderBy(x => propertyInfo.GetValue(x, null));
                case "desc":
                    return source.OrderByDescending(x => propertyInfo.GetValue(x, null));
                default:
                    return source;
            }
        }

        static Expression<Func<T, bool>> ExpressionFunction<T>(ParameterExpression parameterExp, string propertyName, string funcname, string value)
        {
            var propertyExp = Expression.Property(parameterExp, propertyName);
            MethodInfo method = typeof(string).GetMethod(funcname, new[] { typeof(string) });
            var someValue = Expression.Constant(value, typeof(string));
            var containsMethodExp = Expression.Call(propertyExp, method, someValue);
            return BinaryExpression.Lambda<Func<T, bool>>(containsMethodExp, parameterExp);
        }
    }
}
