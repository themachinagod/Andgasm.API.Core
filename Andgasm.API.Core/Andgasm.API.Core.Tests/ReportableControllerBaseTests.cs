using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Andgasm.API.Core.Tests
{
    public class ReportableControllerBaseTests
    {
        IQueryable<TestObject> _testsource;
        ReportableControllerBase _controller;

        public ReportableControllerBaseTests()
        {
            Setup();
        }

        #region Report For Options Tests
        [Fact]
        public void GetQueryForReportOptions_ValidFilterRequest()
        {
            var opts = new ReportOptions() { skip = 0, take = 10 };
            opts.filter = new FilterOptions[1];
            opts.filter[0] = new FilterOptions() { field = "myproperty1", @operator = FilterOperator.eq, value = "test1" };

            var result = _controller.GetQueryForReportOptions<TestObject, TestObjectResource>(_testsource, opts).ToList();
            Assert.Single(result);
            Assert.Equal("test1", result.First().Property1);
        }

        [Fact]
        public void GetQueryForReportOptions_ValidOrderRequest()
        {
            var opts = new ReportOptions() { skip = 0, take = 20 };
            opts.sort = new SortOptions[1];
            opts.sort[0] = new SortOptions() { field = "myproperty1", dir = SortDirection.desc };

            var result = _controller.GetQueryForReportOptions<TestObject, TestObjectResource>(_testsource, opts);
            Assert.Equal(2, result.Count());
            Assert.Equal("test2", result.First().Property1);
            Assert.Equal("test1", result.Last().Property1);
        }

        [Fact]
        public void GetQueryForReportOptions_ValidFilterOrderRequest()
        {
            var opts = new ReportOptions() { skip = 0, take = 10 };
            opts.filter = new FilterOptions[1];
            opts.filter[0] = new FilterOptions() { field = "myproperty1", @operator = FilterOperator.neq, value = "test1" };
            opts.sort = new SortOptions[1];
            opts.sort[0] = new SortOptions() { field = "myproperty1", dir = SortDirection.desc };

            var tmpsrc = _testsource.ToList();
            tmpsrc.Add(new TestObject() { Property1 = "test3", Property2 = "test3", Property3 = 3 });
            var result = _controller.GetQueryForReportOptions<TestObject, TestObjectResource>(tmpsrc.AsQueryable(), opts).ToList();
            Assert.Equal(2, result.Count);
            Assert.Equal("test3", result.First().Property1);
            Assert.Equal("test2", result.Last().Property1);
        }

        [Fact]
        public void GetQueryForReportOptions_InalidFilterRequest_BadProperty()
        {
            var opts = new ReportOptions() { skip = 0, take = 10 };
            opts.filter = new FilterOptions[1];
            opts.filter[0] = new FilterOptions() { field = "badproperty", @operator = FilterOperator.eq, value = "test1" };

            var result = _controller.GetQueryForReportOptions<TestObject, TestObjectResource>(_testsource, opts);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetQueryForReportOptions_InvalidOrderRequest_BadProperty()
        {
            var opts = new ReportOptions() { skip = 0, take = 20 };
            opts.sort = new SortOptions[1];
            opts.sort[0] = new SortOptions() { field = "badproperty", dir = SortDirection.desc };

            var result = _controller.GetQueryForReportOptions<TestObject, TestObjectResource>(_testsource, opts);
            Assert.Equal(2, result.Count());
        }
        #endregion

        #region Setup
        private void Setup(int datarowstoadd = 1)
        {
            MockData();
            var mapper = MockMapper();
            _controller = new ReportableControllerBase(mapper,
                                                       new DynamicExpressionService(new NullLogger<DynamicExpressionService>()),
                                                       new NullLogger<ReportableControllerBase>());
        }

        private void MockData()
        {
            var tmpsrc = new List<TestObject>();
            tmpsrc.Add(new TestObject() { Property1 = "test1", Property2 = "test1", Property3 = 1 });
            tmpsrc.Add(new TestObject() { Property1 = "test2", Property2 = "test2", Property3 = 2 });
            _testsource = tmpsrc.AsQueryable();
        }

        private IMapper MockMapper()
        {
            var profile = new MappingProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(profile));
            return new Mapper(configuration);
        }
        #endregion
    }

    
}
