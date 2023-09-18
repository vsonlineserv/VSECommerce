using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class DiscountCouponDetailsDTO
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool? UsePercentage { get; set; }
        public int? DiscountPercentage { get; set; }
        public int? DiscountAmount { get; set; }
        public string? StartDateUtc { get; set; }
        public string? EndDateUtc { get; set; }
        public string? CouponCode { get; set; }
        public bool RequiresCouponCode { get; set; }
        public int? MaxDiscountAmount { get; set; }
        public int MinOrderValue { get; set; }
        public int StoreId { get; set; }
        public int BranchId { get; set; }
    }
}
