using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class PermissionDTO
    {
        public int UserId { get; set; }
        public int StoreId { get; set; }
        public int BranchId { get; set; }
        public List<int> PermissionIds { get; set; }
    }
}
