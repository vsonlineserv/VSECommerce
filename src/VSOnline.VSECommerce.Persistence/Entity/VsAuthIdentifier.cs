using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class VsAuthIdentifier
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? AuthId { get; set; }
        public bool? Deleted { get; set; }
        public int? StoreRefereneId { get; set; }   

    }
}
