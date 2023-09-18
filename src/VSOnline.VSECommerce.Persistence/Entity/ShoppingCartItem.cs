using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class ShoppingCartItem
    {
        public ShoppingCartItem()
        {

        }
        public int Id { get; set; }
        public int BranchId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }
        public decimal? ShippingCharges { get; set; }
        public string? SelectedSize { get; set; }
    }
}
