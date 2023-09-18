using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class StatusSummaryOrdersResult
    {
        public string? OrderStatus { get; set; }
        public string? PaymentStatus { get; set; }
        public string? PaymentMethodString { get; set; }
        public int OrderStatusId { get; set; }
        public int PaymentStatusId { get; set; }
        public int PaymentMethod { get; set; }
        public int? OrderCount { get; set; }
    }
}
