using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class MenuResult
    {
        public int ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public string CategoryImage { get; set; }
        public string PermaLink { get; set; }
        public List<SubMenuResult> SubMenu { get; set; }
    }
    public class SubMenuResult
    {
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }
        public string CategoryGroupTag { get; set; }
        public string CategoryImage { get; set; }
        public string PermaLink { get; set; }
    }
}
