using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Andgasm.API.Core
{
    public class ReportableControllerBase : ControllerBase
    {
        #region Properties
        protected ILogger _logger { get; set; }
        protected IMapper _datamap { get; set; }
        protected DynamicExpressionService _reporting { get; set; }
        #endregion

        #region Constructors
        public ReportableControllerBase(IMapper datamap, DynamicExpressionService expsvc, ILogger<ReportableControllerBase> logger) : base()
        {
            _datamap = datamap;
            _reporting = expsvc;
            _logger = logger;
        }
        #endregion

        #region Dynamic Expression Parsers
        public IQueryable<T> GetQueryForReportOptions<T, S>(IQueryable<T> queryroot, ReportOptions options)
        {
            if (options.filter != null && options.filter.Count() > 0)
            {
                queryroot = GetQueryForFilter<T, S>(queryroot, options.filter);
            }
            if (options.sort != null && options.sort.Count() > 0)
            {
                queryroot = GetQueryForSort<T, S>(queryroot, options.sort);
            }
            return GetQueryForPageOptions(queryroot, options);
        }

        protected IQueryable<T> GetQueryForFilter<T, S>(IQueryable<T> query, FilterOptions[] filters)
        {
            foreach (var f in filters)
            {
                var mappedPropertyName = GetDestinationPropertyFor<S, T>(_datamap, f.field);
                if (mappedPropertyName != null)
                {
                    query = _reporting.DynamicWhere(query, mappedPropertyName, f.value, f.@operator.ToString());
                }
                else _logger.LogWarning($"Specified filter field '{f.field}' could not be mapped to the resource. Filter for field '{f.field}' has been ignored!");
            }
            return query;
        }

        protected IQueryable<T> GetQueryForSort<T, S>(IQueryable<T> query, SortOptions[] sorts)
        {
            foreach (var s in sorts)
            {
                var mappedPropertyName = GetDestinationPropertyFor<S, T>(_datamap, s.field);
                if (mappedPropertyName != null)
                {
                    query = _reporting.DynamicOrder(query, mappedPropertyName, s.dir.ToString());
                }
                else _logger.LogWarning($"Specified sort field '{s.field}' could not be mapped to the resource. Sort for field '{s.field}' has been ignored!");
            }
            return query;
        }

        protected IQueryable<T> GetQueryForPageOptions<T>(IQueryable<T> query, ReportOptions options)
        {
            return query.Skip(options.skip).Take(options.take);
        }

        protected static string GetDestinationPropertyFor<TSrc, TDst>(IMapper mapper, string sourceProperty)
        {
            var mappedproperty = typeof(TSrc).GetProperties().FirstOrDefault(x => x.Name.ToLowerInvariant() == sourceProperty.ToLowerInvariant());
            if (mappedproperty != null)
            {
                var mappedname = mappedproperty.Name;
                var map = mapper.ConfigurationProvider.FindTypeMapFor<TSrc, TDst>();
                var propertyMap = map.GetPropertyMaps().First(pm => pm.SourceMember.Name == mappedname);
                return propertyMap.DestinationProperty.Name;
            }
            return null;
        }
        #endregion

        #region Mapping Helpers
        protected T MapToResource<T>(T rate)
        {
            return _datamap.Map<T>(rate, opt => opt.Items["Host"] = $"{Request.Scheme}://{Request.Host}");
        }

        protected List<T> MapToResource<T>(List<T> rate)
        {
            return _datamap.Map<List<T>>(rate, opt => opt.Items["Host"] = $"{Request.Scheme}://{Request.Host}");
        }
        #endregion
    }
}
