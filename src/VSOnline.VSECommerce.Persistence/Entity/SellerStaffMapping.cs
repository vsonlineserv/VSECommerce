using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class SellerStaffMapping
    {
        public SellerStaffMapping()
        {
            this.Branches = new HashSet<SellerBranch>();
        }
        [Key]
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int BranchId { get; set; }
        public int UserId { get; set; }
        public int StoreRefereneId { get; set; }    
        public Nullable<System.DateTime> CreatedOnUtc { get; set; }
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }
        public virtual ICollection<SellerBranch> Branches { get; set; }
    }
}
