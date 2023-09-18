using System;
namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class OrderTrackingResult
    {
        public int Orders { get; set; }
        public int Id { get; set; }
        public DateTime OrderDateUtc { get; set; }
        public decimal? OrderTotal { get; set; }
        public int Quantity { get; set; }
        public int NumberOfProducts { get; set; }
        public int OrderStatusId { get; set; }
        public string OrderStatus { get; set; }

        public string OrderItemStatus { get; set; }
        public int? OrderItemStatusId { get; set; }

        public int PaymentMethod { get; set; }
        public string PaymentMethodString { get; set; }

        public string PaymentStatus { get; set; }
        public int? PaymentStatusId { get; set; }

        public string Name { get; set; }
        public string BranchName { get; set; }
        public decimal? PriceInclTax { get; set; }
        public decimal? ShippingCharges { get; set; }
        public string SelectedSize { get; set; }
        public int? BranchOrderId { get; set; }

    }
}

