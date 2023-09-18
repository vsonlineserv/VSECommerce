using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class LocationBoundaryResult
    {
        public int BranchId { get; set; }
        public int Store { get; set; }
        public double Distance { get; set; }
    }
}
