using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class Currency
    {
        public int Id { get; set; }
        public string? CurrencyName { get; set; }
        public string? Code { get; set; }
        public string? Symbol { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public int BranchId { get; set; }
    }
}
