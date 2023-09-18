using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class NewMasterShippingCalculation
    {
        public int Id { get; set; }
        public string? Type { get; set; }
        public string? DisplayName { get; set; }
        public string? DeliveryTime { get; set; }
        public int? Rate { get; set; }
        public int? RangeStart { get; set; }
        public int? RangeEnd { get; set; }
        public string? CreatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public int BranchId { get; set; }
    }
}
