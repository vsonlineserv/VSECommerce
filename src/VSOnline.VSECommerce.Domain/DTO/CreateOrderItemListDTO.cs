using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class CreateOrderItemListDTO
    {
        public List<ShoppingCartResult> shoppingCartDTOList { get; set; }
        public string UserName { get; set; }
        public Enums.PaymentOption PaymentMethod { get; set; }
        public Enums.DeliveryOption DeliveryMethod { get; set; }
        public string? CouponCode { get; set; }
        public string? PayPalOrderId { get; set; }
        public string? RazorPaymentId { get; set; }
        public string? RazorOrderId { get; set; }
        public string? RazorSignature { get; set; }
        public string? RazorGeneratedOrderId { get; set; }
        public decimal? CouponValue { get; set; }
        public int AddressId { get; set; }  
    }
}
