using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class Pricing
    {
        public int PricingId { get; set; }
        public int Product { get; set; }
        public int Store { get; set; }
        public int Branch { get; set; }
        public bool CallForPrice { get; set; }
        public Nullable<decimal> Price { get; set; }
        public decimal OldPrice { get; set; }
        public decimal ProductCost { get; set; }
        public Nullable<decimal> SpecialPrice { get; set; }
        public string? SpecialPriceDescription { get; set; }

        public Nullable<System.DateTime> SpecialPriceStartDateTimeUtc { get; set; }
        public Nullable<System.DateTime> SpecialPriceEndDateTimeUtc { get; set; }
        public decimal? AdditionalShippingCharge { get; set; }
        public int? DeliveryTime { get; set; }
        //  public Nullable<bool> IsShipEnabled { get; set; }
        public Nullable<bool> IsFreeShipping { get; set; }
        public Nullable<decimal> AdditionalTax { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public string? CreatedUser { get; set; }
        public string? UpdatedUser { get; set; }

        public System.DateTime? CreatedOnUtc { get; set; }
        public System.DateTime? UpdatedOnUtc { get; set; }


        [ForeignKey("Product")]
        public virtual Product ProductDetails { get; set; }
        [ForeignKey("Branch")]
        public virtual SellerBranch BranchDetails { get; set; }

        public int? ProductVariantId { get; set; }
        [ForeignKey("Product")]
        public virtual ProductStoreMapping ProductStoreMapping { get; set; }
    }
}
