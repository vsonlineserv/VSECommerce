using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class ShippingCalculationDTO
    {
        public string? Type { get; set; }
        public string? DisplayName { get; set; }
        public string? DeliveryTime { get; set; }
        public int Rate { get; set; }
        public int RangeStart { get; set; }
        public int RangeEnd { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
    }
}
