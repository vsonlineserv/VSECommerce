using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.ResultSet;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class ProductSearchFilterDTO
    {
        public string? productFilter { get; set; }
        public SelectedFilter? filter { get; set; }
        public decimal? lat { get; set; }
        public decimal? lng { get; set; }
        public int? mapRadius { get; set; }
        public int? priceRangeFrom { get; set; }
        public int? PriceRangeTo { get; set; }
        public int? pageStart { get; set; }
        public int? pageSize { get; set; }
    }
}
