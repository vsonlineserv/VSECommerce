using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class SellerRating
    {
        [Key]
        public int id { get; set; }
        public int BranchId { get; set; }
        public int Rating { get; set; }
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }
    }
}
