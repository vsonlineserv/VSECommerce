using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class UserWishlist
    {
        public int Id { get; set; }
        public int User { get; set; }
        public int Product { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public int? BranchId { get; set; }
    }
}
