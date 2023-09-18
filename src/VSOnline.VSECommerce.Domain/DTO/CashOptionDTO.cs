using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class CashOptionDTO
    {
        public bool CardOnDeliveryEnbled { get; set; }
        public bool CashOnDeliveryEnabled { get; set; }
        public bool PayUEnabled { get; set; }
    }
}
