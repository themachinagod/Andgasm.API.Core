using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Andgasm.API.Core.Tests
{
    // TODO: flesh out!!
    public class DynamicExpressionServiceTests
    {
        IQueryable<TestObject> _testsource;
        DynamicExpressionService _svc;

        public DynamicExpressionServiceTests()
        {
            Setup();
        }

        #region Dynamic Where Tests
        [Fact]
        public void DynamicWhere_ValidEqualsRequest()
        {
            var result = _svc.DynamicWhere(_testsource, "Property1", "test1", FilterOperator.eq);
            Assert.Single(result);
            Assert.Equal("test1", result.First().Property1);
        }

        [Fact]
        public void DynamicWhere_ValidNotEqualsRequest()
        {
            var result = _svc.DynamicWhere(_testsource, "Property1", "test1", FilterOperator.neq);
            Assert.Single(result);
            Assert.Equal("test2", result.First().Property1);
        }

        [Fact]
        public void DynamicWhere_ValidContainsRequest()
        {
            var result = _svc.DynamicWhere(_testsource, "Property1", "st1", FilterOperator.contains);
            Assert.Single(result);
            Assert.Equal("test1", result.First().Property1);
        }

        [Fact]
        public void DynamicWhere_ValidStartsWithRequest()
        {
            var result = _svc.DynamicWhere(_testsource, "Property1", "test", FilterOperator.startswith);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void DynamicWhere_ValidEndsWithRequest()
        {
            var result = _svc.DynamicWhere(_testsource, "Property1", "st1", FilterOperator.endswith);
            Assert.Single(result);
            Assert.Equal("test1", result.First().Property1);
        }

        [Fact]
        public void DynamicWhere_ValidGreaterThanRequest()
        {
            var result = _svc.DynamicWhere(_testsource, "Property3", 1, FilterOperator.gt);
            Assert.Single(result);
            Assert.Equal("test2", result.First().Property1);
        }

        [Fact]
        public void DynamicWhere_ValidGreaterThanEqualsRequest()
        {
            var result = _svc.DynamicWhere(_testsource, "Property3", 2, FilterOperator.gte);
            Assert.Single(result);
            Assert.Equal("test2", result.First().Property1);
        }

        [Fact]
        public void DynamicWhere_ValidLessThanRequest()
        {
            var result = _svc.DynamicWhere(_testsource, "Property3", 2, FilterOperator.lt);
            Assert.Single(result);
            Assert.Equal("test1", result.First().Property1);
        }

        [Fact]
        public void DynamicWhere_ValidLessThanEqualsRequest()
        {
            var result = _svc.DynamicWhere(_testsource, "Property3", 1, FilterOperator.lte);
            Assert.Single(result);
            Assert.Equal("test1", result.First().Property1);
        }

        [Fact]
        public void DynamicWhere_InvalidRequest_BadProperty()
        {
            var result = _svc.DynamicWhere(_testsource, "badproperty", "avalue", FilterOperator.eq);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void DynamicWhere_InvalidRequest_BadOperation()
        {
            var result = _svc.DynamicWhere(_testsource, "Property1", "avalue", FilterOperator.eq); // DBr: dont have bad enum val
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void DynamicWhere_InvalidRequest_BadValueType()
        {
            var result = _svc.DynamicWhere(_testsource, "Property3", "badval", FilterOperator.eq); // DBr: dont have bad enum val
            Assert.Equal(2, result.Count());
        }
        #endregion


        #region Dynamic Order
        [Fact]
        public void DynamicOrder_ValidAscendingRequest()
        {
            var result = _svc.DynamicOrder(_testsource, "Property1", SortDirection.asc);
            Assert.Equal(2, result.Count());
            Assert.Equal("test1", result.First().Property1);
            Assert.Equal("test2", result.Last().Property1);
        }

        [Fact]
        public void DynamicOrder_ValidDescendingRequest()
        {
            var result = _svc.DynamicOrder(_testsource, "Property1", SortDirection.desc);
            Assert.Equal(2, result.Count());
            Assert.Equal("test2", result.First().Property1);
            Assert.Equal("test1", result.Last().Property1);
        }

        [Fact]
        public void DynamicOrder_InvalidRequest_BadDirection()
        {
            var result = _svc.DynamicOrder(_testsource, "Property1", SortDirection.asc); // DBr: dont have bad enum val
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void DynamicOrder_InvalidRequest_BadProperty()
        {
            var result = _svc.DynamicOrder(_testsource, "badproperty", SortDirection.asc); // DBr: dont have bad enum val
            Assert.Equal(2, result.Count());
        }
        #endregion

        #region Setup
        private void Setup()
        {
            _svc = new DynamicExpressionService(new NullLogger<DynamicExpressionService>());
            var tmpsrc = new List<TestObject>();
            tmpsrc.Add(new TestObject() { Property1 = "test1", Property2 = "test1", Property3 = 1 });
            tmpsrc.Add(new TestObject() { Property1 = "test2", Property2 = "test2", Property3 = 2 });
            _testsource = tmpsrc.AsQueryable();
        }
        #endregion
    }
}
