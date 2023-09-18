using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ShiprocketDTO
{
    public class CustomerAddressDTO
    {
        public int order_id { get; set; }
        public string shipping_customer_name { get; set; }
        public int shipping_phone { get; set; }
        public string shipping_address { get; set; }
        public string? shipping_address_2 { get; set; }
        public string shipping_city { get; set; }
        public string shipping_state { get; set; }
        public string shipping_country { get; set; }
        public int shipping_pincode { get; set; }
        public string shipping_email { get; set; }

    }
}
