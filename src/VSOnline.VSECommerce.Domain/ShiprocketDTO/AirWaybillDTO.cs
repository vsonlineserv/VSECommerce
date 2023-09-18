using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ShiprocketDTO
{
    public class AirWaybillDTO
    {
        public int shipment_id { get; set; }
        public int? courier_id { get; set; }
        public string? status { get; set; }
    }
}
