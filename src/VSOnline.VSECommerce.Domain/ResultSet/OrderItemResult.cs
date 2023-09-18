using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class OrderItemResult
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public int CustomerId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Name { get; set; }
        public string PictureName { get; set; }
        public decimal SpecialPrice { get; set; }
        public decimal Price { get; set; }
        public decimal ShippingCharges { get; set; }
        public int BranchId { get; set; }
        public string Branch { get; set; }
        public string SelectedSize { get; set; }
    }
}
