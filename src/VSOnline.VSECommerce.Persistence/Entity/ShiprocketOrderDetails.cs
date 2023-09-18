using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class ShiprocketOrderDetails
    {
        public int Id { get; set; }
        public string OrderId { get; set; }
        public string? ShiprocketOrderId { get; set; }
        public string? ShipmentId { get; set; }
        public string? AwbCode { get; set; }

    }
}
