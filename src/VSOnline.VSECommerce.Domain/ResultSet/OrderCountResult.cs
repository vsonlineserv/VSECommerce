using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class OrderCountResult
    {
        public int Count { get; set; }
        public int Status { get; set; }
        public string? Name { get; set; }
    }
}
