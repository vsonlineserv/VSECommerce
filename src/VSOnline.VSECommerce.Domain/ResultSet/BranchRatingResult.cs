using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class BranchRatingResult
    {
        public int BranchId { get; set; }
        public int Rating { get; set; }
        public int RatingCount { get; set; }
    }
}
