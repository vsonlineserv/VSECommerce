using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class CategoryHomePageModelResult
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public string? CategoryGroupTag { get; set; }
        public string? CategoryImage { get; set; }
        public string? PermaLink { get; set; }
    }
}
