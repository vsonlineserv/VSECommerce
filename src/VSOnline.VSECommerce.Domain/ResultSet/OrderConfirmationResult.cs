using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class OrderConfirmationResult
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public OrderDTO OrderDetails { get; set; }
        public BuyerAddressResult ShippingAddress { get; set; }
        public List<OrderItemResult> OrderItemDetails { get; set; }
    }
}
