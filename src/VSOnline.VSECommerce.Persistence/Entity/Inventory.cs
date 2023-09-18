using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class NewInventory
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SellerBranchId { get; set; }
        public string? SKU { get; set; }
        public string? BarCode { get; set; }
        public int AvailableQuantity { get; set; }
        public bool FlagTrackQuantity { get; set; }
        public bool FlagAllowSellOutOfStock { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public int BranchId { get; set; }
    }
}
