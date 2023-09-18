using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class ManufacturerResult
    {
        public int ManufacturerId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? MetaKeywords { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaTitle { get; set; }
        public bool? LimitedToStores { get; set; }
        public bool? Deleted { get; set; }
        public int? DisplayOrder { get; set; }
        public int? StoreId { get; set; }
        public int? BranchId { get; set; }
        public string? ManufacturerImage { get; set; }  
    }
}
