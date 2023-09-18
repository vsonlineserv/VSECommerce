using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class ProductSummaryOrdersResult
    {
        public string? ProductName { get; set; }
        public int? quantity { get; set; }
        public decimal? Total { get; set; }
    }
}
