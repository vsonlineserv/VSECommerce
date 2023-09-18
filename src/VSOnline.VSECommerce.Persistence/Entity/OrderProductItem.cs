using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class OrderProductItem
    {
        [Key]
        public int Id { get; set; }
        public Guid OrderItemGuid { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPriceInclTax { get; set; }
        public decimal PriceInclTax { get; set; }
        public decimal? ShippingCharges { get; set; }
        public bool OrderCancel { get; set; }
        public int? OrderItemStatus { get; set; }
        public string? SelectedSize { get; set; }

        [ForeignKey("OrderId")]
        public virtual OrderProduct OrderProductMap { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product ProductMap { get; set; }
        [ForeignKey("BranchId")]
        public virtual SellerBranch SellerBranchMap { get; set; }
    }
}
