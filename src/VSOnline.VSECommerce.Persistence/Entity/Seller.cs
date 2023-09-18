using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class Seller
    {
        public Seller()
        {
            this.Branches = new HashSet<SellerBranch>();
        }
        [Key]
        public int StoreId { get; set; }
        public string? StoreName { get; set; }
        public string? LogoPicture { get; set; }
        public string? Description { get; set; }
        public int PrimaryContact { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }
        public int CreatedUser { get; set; }
        public int? StoreRefereneId { get; set; }    
        public virtual ICollection<SellerBranch> Branches { get; set; }
        public bool? FlagvBuy { get; set; } 
    }
}
    