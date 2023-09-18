using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class ProductContact
    {
        public int Id { get; set; }
        public string? ContactName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public int ProductId { get; set; }
        public System.DateTime UpdatedOnUtc { get; set; }
        public string? Subject { get; set; }
        public string? Reply { get; set; }
        public System.DateTime? ReplyDate { get; set; }
        public int BranchId { get; set; }  

    }
}
