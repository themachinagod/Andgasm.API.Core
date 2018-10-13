using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Andgasm.API.Core.Tests
{
    public class TestObject
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }
        public int Property3 { get; set; }
    }

    public class TestObjectResource
    {
        public string MyProperty1 { get; set; }
        public string MyProperty2 { get; set; }
        public int MyProperty3 { get; set; }
    }

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            MapTestObjectToResource();
        }

        private void MapTestObjectToResource()
        {
            CreateMap<TestObject, TestObjectResource>()
                .ForMember(dest => dest.MyProperty1,
                           src => src.MapFrom(y => y.Property1))
                .ForMember(dest => dest.MyProperty2,
                           src => src.MapFrom(y => y.Property2))
                .ForMember(dest => dest.MyProperty3,
                           src => src.MapFrom(y => y.Property3))
                .ReverseMap();
        }
    }
}
