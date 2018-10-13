using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Andgasm.API.Core
{
    public enum FilterOperator
    {
        neq,
        eq,
        lt,
        lte,
        gt,
        gte,
        contains,
        startswith,
        endswith
    }

    public enum SortDirection
    {
        asc,
        desc
    }

    public class ReportOptions
    {
        public int skip { get; set; }
        public int take { get; set; }
        public FilterOptions[] filter { get; set; }
        public SortOptions[] sort { get; set; }
    }

    public class FilterOptions
    {
        public string field { get; set; }
        public FilterOperator @operator { get; set; }
        public object value { get; set; }
    }

    public class SortOptions
    {
        public string field { get; set; }
        public SortDirection dir { get; set; }
    }
}
