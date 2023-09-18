using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class DiscountFilterDTO
    {
        public int? days { get; set; }
        public int? month { get; set; }
        public string? startTime { get; set; }
        public string? endTime { get; set; }
        public string? searchString { get; set; }
        public bool activeCoupons { get; set; }
        public int StoreId { get; set; }
        public int BranchId { get; set; }
    }
}
