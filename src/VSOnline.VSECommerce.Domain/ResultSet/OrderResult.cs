using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class OrderResult
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime OrderDateUtc { get; set; }
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPriceInclTax { get; set; }
        public decimal? PriceInclTax { get; set; }
        public int CustomerId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Country { get; set; }        
        public string? Address1 { get; set; }
        public string? City { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public int? ShippingStatusId { get; set; }
        public int? PaymentStatusId { get; set; }
        public string? PaymentStatus { get; set; }
        public int? OrderItemStatusId { get; set; }
        public string? OrderItemStatus { get; set; }
        public decimal? OrderTotal { get; set; }
        public string? PhoneNumber { get; set; }
        public int? PaymentMethod { get; set; }
        public string? PaymentMethodString { get; set; }
        public int? OrderCount { get; set; }
        public int? OrderStatusId { get; set; }
        public string? OrderStatus { get; set; }
        public string? PictureName { get; set; }
        public string? SelectedSize { get; set; }
        public string? Address2 { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Email { get; set; }
        public bool? FlagConfirmStatus { get; set; }
        public decimal? OrderShippingTotal { get; set; }
        public decimal? OrderDiscount { get; set; }
        public decimal? OrderTaxTotal { get; set; }
        public int? BranchOrderId { get; set; }
        public string? BranchOrderIdWithPrefix { get; set; }

    }
}
