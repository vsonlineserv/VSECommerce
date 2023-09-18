using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class FilterDTO
    {
        public int BranchId { get; set; }
        public int Status { get; set; }
        public int Days { get; set; }
        public string? SearchString { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
    }
}
