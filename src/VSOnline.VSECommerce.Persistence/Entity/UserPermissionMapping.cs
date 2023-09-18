using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class UserPermissionMapping
    {
        public int Id { get; set; }
        public int PermissionId { get; set; }
        public int UserId { get; set; }
        public int StoreId { get; set; }
        public int BranchId { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    }
}
