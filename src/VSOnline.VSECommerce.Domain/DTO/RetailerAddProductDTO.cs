using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class RetailerAddProductDTO
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public decimal NewPrice { get; set; }
        public decimal NewSpecialPrice { get; set; }
        public string? NewSpecialPriceDescription { get; set; }
        public DateTime NewPriceStartTime { get; set; }
        public DateTime NewPriceEndTime { get; set; }
        public decimal NewAdditionalTax { get; set; }
        public int StoreId { get; set; }
        public List<int>? BranchIdList { get; set; }
    }
}
