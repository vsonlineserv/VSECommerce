using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class ShiprocketOrderDTO
    {
        public string order_id { get; set; }
        public string order_date { get; set; }
        public string pickup_location { get; set; }
        public string?  channel_id { get; set; } 
        public string billing_customer_name { get; set; }
        public string? billing_last_name { get; set; }
        public string billing_address { get; set; }
        public string billing_phone { get; set; }   
        public string billing_city { get; set; }
        public int billing_pincode { get; set; }
        public string billing_state { get; set; }
        public string billing_country { get; set; }
        public string billing_email { get; set; }
        public bool shipping_is_billing { get; set; }
        public string? shipping_customer_name { get; set; }
        public string? shipping_address { get; set; }
        public string? shipping_city { get; set; }
        public int shipping_pincode { get; set; }
        public string? shipping_country { get; set; }
        public string? shipping_state { get; set; }
        public string? shipping_email { get; set; }
        public string? shipping_phone { get; set; }
        public List<order_items> order_items { get; set; }
        public string payment_method { get; set; }
        public int sub_total { get; set; }
        public float length { get; set; }
        public float breadth { get; set; }
        public float height { get; set; }
        public float weight { get; set; }   
    }

    public class order_items
    {
        public string name { get; set; }
        public string sku { get; set; }
        public int units { get; set; }
        public string selling_price { get; set; }   
    }
}
