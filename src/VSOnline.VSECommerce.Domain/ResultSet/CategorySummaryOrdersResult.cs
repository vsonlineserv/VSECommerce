using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class CategorySummaryOrdersResult
    {
        public string? CategoryName { get; set; }
        public int? Quantity { get; set; }
        public decimal? Total { get; set; }
    }
}
