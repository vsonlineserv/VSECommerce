using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class SellerContactResult
    {
        public int Id { get; set; }
        public string? ContactName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public int? ProductId { get; set; }
        public int? StoreId { get; set; }
        public string? Subject { get; set; }
        public string? Reply { get; set; }
        public DateTime UpdatedOnIST { get; set; }
        public DateTime? ReplyDateIST { get; set; }
        public string? ProductName { get; set; }
        public string? BranchName { get; set; }
    }
    public class SellerContactInfo
    {
        public string? ContactName { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public int ProductId { get; set; }
        public int StoreId { get; set; }
    }
}
