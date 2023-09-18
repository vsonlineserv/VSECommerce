using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class BaseProductFilterResult
    {
        public decimal? Min { get; set; }
        public decimal? Max { get; set; }
        public List<BrandFilterDTO> Brand { get; set; }
    }
    public class BrandFilterDTO
    {
        public int Id;
        public string? BrandName;
    }
    public class ProductFilter
    {
        public string? FilterParameter { get; set; }
        public string? FilterValueText { get; set; }
    }
}
