using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class InventoryDTO
    {
        public int ProductId { get; set; }
        public int BranchId { get; set; }
        public int AvailableQuantity { get; set; }
        public int purchasedQuantity { get; set; }
        public string? SKU { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public string? PictureName { get; set; }
        public bool IsTrackQuantity { get; set; }
        public bool IsOutOfStock { get; set; }
    }
}
