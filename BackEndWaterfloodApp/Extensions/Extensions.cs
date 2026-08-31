using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BackEndWaterFloodApp.Extensions
{
    public static class Extensions
    {
        public static void AddPagination(
            this HttpResponse response,
            int currentPage,
            int itemsPerPage,
            int totalItem,
            int totalPage
        )
        {
            var paginationHeader = new PaginationHeader(
                currentPage,
                itemsPerPage,
                totalItem,
                totalPage
            );
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            response.Headers["Pagination"] = JsonConvert.SerializeObject(
                paginationHeader,
                camelCaseFormatter
            );
            response.Headers["Access-Control-Expose-Headers"] = "Pagination";
        }
    }
}
