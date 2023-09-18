using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class SubscriptionProvider
    {
        public int Id { get; set; }
        public string? Provider { get; set; }
        public string? SecretKey { get; set; }
        public string? SecretId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public bool FlagEnable { get; set; }
        public int ProviderName { get; set; }
        public string? Details1 { get; set; }
        public string? Details2 { get; set; }
        public int BranchId { get; set; }
    }
}
