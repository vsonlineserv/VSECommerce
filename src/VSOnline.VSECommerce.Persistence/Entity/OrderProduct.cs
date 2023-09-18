using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class OrderProduct
    {
        public OrderProduct()
        {

            this.OrderProductItem = new HashSet<OrderProductItem>();
        }
        [Key]
        public int Id { get; set; }
        public Guid OrderGuid { get; set; }
        public int CustomerId { get; set; }
        public int BillingAddressId { get; set; }
        public int ShippingAddressId { get; set; }
        public int OrderStatusId { get; set; }
        public int ShippingStatusId { get; set; }
        public int PaymentStatusId { get; set; }
        public int PaymentMethod { get; set; }
        public string? VatNumber { get; set; }
        public decimal OrderSubtotalInclTax { get; set; }
        public decimal OrderShippingCharges { get; set; }
        public decimal? PaymentMethodAdditionalFee { get; set; }
        public decimal? OrderDiscount { get; set; }
        public decimal? OrderTotal { get; set; }
        public decimal? RefundedAmount { get; set; }
        public string? CustomerIp { get; set; }
        public string? TransactionId { get; set; }
        public string? TransactionResult { get; set; }
        public string? TransactionResultDetails { get; set; }
        public DateTime OrderDateUtc { get; set; }
        public int DeliveryMethod { get; set; }
        public bool Deleted { get; set; }
        public DateTime UpdatedOnUtc { get; set; }
        public int PurchaseOrderStatus { get; set; }
        public virtual ICollection<OrderProductItem> OrderProductItem { get; set; }
        public bool? OrderCancel { get; set; }
        public int BranchId { get; set; }
        public decimal? OrderTaxTotal { get; set; }
        public bool? FlagConfirmStatus { get; set; }
        public decimal? OrderShippingTotal { get; set; }    
        public string? OrderOrigin { get; set; }
        public int? BranchOrderId { get; set; }
        public bool? OrderFromVbuy { get; set; }
        public bool? OrderFromVshopper { get; set; }
    }

}
