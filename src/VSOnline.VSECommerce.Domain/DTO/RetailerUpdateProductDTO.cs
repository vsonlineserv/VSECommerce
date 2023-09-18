using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class RetailerUpdateProductDTO
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public decimal SpecialPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string SpecialPriceDescription { get; set; }
        public string NewSpecialPriceDescription { get; set; }
        public decimal NewSpecialPrice { get; set; }
        public DateTime PriceStartTime { get; set; }
        public DateTime PriceEndTime { get; set; }
        public DateTime NewPriceStartTime { get; set; }
        public DateTime NewPriceEndTime { get; set; }
        public decimal AdditionalTax { get; set; }
        public decimal NewAdditionalTax { get; set; }
        public decimal NewShippingCharge { get; set; }
        public int NewDeliveryTime { get; set; }
        public decimal ShippingCharge { get; set; }
        public bool IsFreeShipping { get; set; }
        public int DeliveryTime { get; set; }
        public int StoreId { get; set; }
        public List<int> BranchIdList { get; set; }
    }
}
