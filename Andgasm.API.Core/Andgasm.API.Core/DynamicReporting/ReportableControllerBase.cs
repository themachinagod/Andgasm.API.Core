using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SE.ACT.API.Models;
using SE.ACT.API.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Andgasm.API.Core
{
    public class ReportableControllerBase : ControllerBase
    {
        #region Properties
        protected DbContext _dbContext;
        protected ILogger _logger { get; set; }
        protected IMapper _datamap { get; set; }
        protected DynamicExpressionService _reporting { get; set; }
        #endregion

        #region Constructors
        public ReportableControllerBase(IMapper datamap, DynamicExpressionService expsvc, ILogger<ReportableControllerBase> logger, DbContext dbcontext) : base()
        {
            _datamap = datamap;
            _reporting = expsvc;
            _logger = logger;
            _dbContext = dbcontext;
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
            try
            {
                foreach (var f in filters)
                {

                    var mappedPropertyName = GetDestinationPropertyFor<S, T>(_datamap, f.field);
                    if (mappedPropertyName != null)
                    {
                        query = _reporting.DynamicWhere(query, mappedPropertyName, f.value, f.@operator);
                    }
                    else _logger.LogWarning($"Specified filter field '{f.field}' could not be mapped to the resource. Filter for field '{f.field}' has been ignored!");
                }
                return query;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        protected IQueryable<T> GetQueryForSort<T, S>(IQueryable<T> query, SortOptions[] sorts)
        {
            foreach (var s in sorts)
            {
                var mappedPropertyName = GetDestinationPropertyFor<S, T>(_datamap, s.field);
                if (mappedPropertyName != null)
                {
                    query = _reporting.DynamicOrder(query, mappedPropertyName, s.dir);
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
                var map = mapper.ConfigurationProvider.FindTypeMapFor<TDst, TSrc>();
                var propertyMap = map.PropertyMaps.FirstOrDefault(pm => pm.DestinationMember.Name == mappedname);
                var filterattribute = propertyMap.DestinationMember.CustomAttributes.FirstOrDefault(x => x.AttributeType == typeof(FilterAttribute));
                if (filterattribute != null)
                {
                    var att = filterattribute.NamedArguments.FirstOrDefault(x => x.MemberName == "AlternativeColumntoFilter");
                    if (att != null)
                    {
                        var ac = att.TypedValue.Value.ToString();
                        return GetDestinationPropertyFor<TSrc, TDst>(mapper, ac);
                    }
                }
                else if (propertyMap != null)
                {
                    return propertyMap.CustomMapExpression.Body.ToString().Replace("y.", "");
                }
            }
            return null;
        }
        #endregion

        #region Mapping Helpers
        protected S MapToResource<T, S>(T rate, S res)
        {
            return _datamap.Map(rate, res, opt => opt.Items["Host"] = $"{Request.Scheme}://{Request.Host}");
        }

        protected S MapToResource<T, S>(T rate)
        {
            return _datamap.Map<S>(rate, opt => opt.Items["Host"] = $"{Request.Scheme}://{Request.Host}");
        }

        protected List<S> MapToResource<T, S>(List<T> rate)
        {
            var mappedresources = _datamap.Map<List<S>>(rate, opt => opt.Items["Host"] = $"{Request.Scheme}://{Request.Host}");

            return mappedresources;
        }

        public async Task<R> SaveChangesAndRemapResource<R, E>(R resource, E entity, EntityState optype, Func<R, E, string, Task<int>> remapcallback = null) where E : class
        {
            UpdateDatabaseCollectionForOperation(entity, optype);
            await _dbContext.SaveChangesAsync();

            if (optype == EntityState.Added || optype == EntityState.Deleted) resource = MapToResource<E, R>(entity);
            else MapToResource(entity, resource);
            if (remapcallback != null) await remapcallback(resource, entity, optype.ToString().ToLowerInvariant());
            return resource;
        }

        private void UpdateDatabaseCollectionForOperation<T>(T entity, EntityState optype) where T : class
        {
            if (optype == EntityState.Added) _dbContext.Add(entity);
            else if (optype == EntityState.Deleted) _dbContext.Remove(entity);
            else _dbContext.Entry(entity).State = EntityState.Modified;
        }
        #endregion

        #region Request Helpers
        protected IActionResult InvalidIdBadRequest(int id)
        {
            return BadRequest($"The specified id '{id}' was not valid");
        }

        protected IActionResult NoPayloadBadRequest()
        {
            return BadRequest("No payload data was recieved to action the request");
        }

        protected IActionResult NoTakeSpecifiedBadRequest()
        {
            return BadRequest("Request must specify number of records to retrieve in the take option, must be greater than 0!");
        }

        protected IActionResult NoSkipSpecifiedBadRequest()
        {
            return BadRequest("Request must specify number of records to retrieve in the take option, must be greater than 0!");
        }

        protected IActionResult IdNotFound(int id)
        {
            return NotFound($"The specified id '{id}' was not found in the data store");
        }

        protected IActionResult PrimaryKeyConflict(int id)
        {
            return Conflict($"Cannot store to data store: Primary key '{id}' already exists!");
        }
        #endregion

        #region Query Helpers
        protected async Task<bool> EntityExists<T>(int id) where T : class
        {
            return await _dbContext.EntityExists<T>(id);
        }

        protected async Task<T> GetEntityById<T>(int id) where T : class
        {
            return await _dbContext.GetEntityById<T>(id);
        }

        protected async Task<TTarget> SetNullableNavigationProperty<TEntity, TTarget>(TEntity entity, Expression<Func<TEntity, TTarget>> navigationProperty,
                                                                                                      Expression<Func<TTarget, bool>> query) where TEntity : class
                                                                                                                                             where TTarget : class
        {
            return await _dbContext.SetNullableNavigationProperty(entity, navigationProperty, query);
        }
        #endregion
    }
}
