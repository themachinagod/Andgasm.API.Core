using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;

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

        protected string CompileFilterOperator(FilterOperator filterType)
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
            string dataoperator = CompileFilterOperator(filtertype);
            var methodInfo = typeof(Expression).GetMethod(dataoperator, types);
            var expression = (BinaryExpression)methodInfo.Invoke(null, new object[] { propertyAccess, valueExpression });
            return source.Where(Expression.Lambda<Func<T, bool>>(expression, roottable));
        }
    }
}
