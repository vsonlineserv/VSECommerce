using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class Discount
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int? DiscountTypeId { get; set; }
        public bool UsePercentage { get; set; }
        public decimal? DiscountPercentage { get; set; }
        public decimal? DiscountAmount { get; set; }
        public System.DateTime? StartDateUtc { get; set; }
        public System.DateTime? EndDateUtc { get; set; }
        public bool? RequiresCouponCode { get; set; }
        public string? CouponCode { get; set; }
        public decimal? MinOrderValue { get; set; }
        public decimal? MaxDiscountAmount { get; set; } 
        public System.DateTime CreatedDateUtc { get; set; }
        public Nullable<System.DateTime> UpdatedDateUtc { get; set; }
        public bool? IsDeleted { get; set; }
        public int? BranchId { get; set; }
    }
}
    