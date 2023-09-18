using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class ProductKeyFeaturesResult
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Parameter { get; set; }
        public string? KeyFeature { get; set; }
    }
}
