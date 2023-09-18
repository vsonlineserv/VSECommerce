using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class ProductSpecificationResult
    {
        public int ProductId { get; set; }
        public string? SpecificationGroup { get; set; }
        public string? SpecificationAttribute { get; set; }
        public string? SpecificationDetails { get; set; }
    }
}
