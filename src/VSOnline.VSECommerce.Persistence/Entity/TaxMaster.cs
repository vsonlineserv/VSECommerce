using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class TaxMaster
    {
        public int Id { get; set; }
        public string? TaxType { get; set; }
        public string? PrimaryOption { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public int BranchId { get; set; }
    }
}
