using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class SellerBranch
    {
        public SellerBranch()
        {
            this.PricingCollection = new HashSet<Pricing>();
        }
        [Key]
        public int BranchId { get; set; }
        public int Store { get; set; }
        public string? BranchName { get; set; }
        public string? Email { get; set; }
        public int Country { get; set; }
        public string? State { get; set; }
        public string? City { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FaxNumber { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        [ForeignKey("Store")]
        public virtual Seller SellerMap { get; set; }

        [ForeignKey("BranchId")]
        public virtual ICollection<SellerStaffMapping> SellerSupport { get; set; }
        public bool? EnableBuy { get; set; }
        public bool? FlagPartner { get; set; }
        public virtual ICollection<Pricing> PricingCollection { get; set; }
        public string? StoreType { get; set; }
        public string? OrderIdPrefix { get; set; }
        public bool? FlagvBuy { get; set; }

    }
}
