using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class ShiprocketApiUser
    {
        public int Id { get; set; } 
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ShiprocketToken { get; set; }
        public int StoreId { get; set; }
        public int BranchId { get; set; }
        public Nullable<System.DateTime> CreatedOnUtc { get; set; }
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }
        public Nullable<System.DateTime> ExpireTime { get; set; }

    }
}
