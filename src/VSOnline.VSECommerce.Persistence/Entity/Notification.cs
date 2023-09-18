using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class Notification
    {
        public int Id { get; set; }
        public string AuthId { get; set; }
        public string MobileToken { get; set; }
        public int BranchId { get; set; }
        public Nullable<System.DateTime> CreatedOnUtc { get; set; }
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }
        public bool FlagNotification { get; set; }
    }
}
