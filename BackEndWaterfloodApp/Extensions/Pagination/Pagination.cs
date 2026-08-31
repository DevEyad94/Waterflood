using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEndWaterFloodApp.Extensions.Pagination
{
    public class Pagination
    {
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int _pageSize = 10;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = (value > maxPageSize) ? maxPageSize : value; }
        }

        // Adding sorting properties with default values
        public string SortColumn { get; set; } = "RefID"; // Default sort by RefID
        public string SortDirection { get; set; } = "asc";
    }
}
