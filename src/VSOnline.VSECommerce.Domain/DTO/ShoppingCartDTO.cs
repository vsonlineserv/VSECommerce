using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class ShoppingCartDTO
    {
        public int BranchId { get; set; }
        public int CustomerId { get; set; }
        public string UserName { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? AdditionalShippingCharge { get; set; }
        public string? SelectedSize { get; set; }
    }
}
