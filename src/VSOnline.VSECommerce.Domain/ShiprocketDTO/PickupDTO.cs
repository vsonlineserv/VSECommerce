using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ShiprocketDTO
{
    public class PickupDTO
    {
        public int shipment_id { get; set; }
        public string? status { get; set; }
        public List<DateTime>? pickup_date { get; set; }
    }
}
