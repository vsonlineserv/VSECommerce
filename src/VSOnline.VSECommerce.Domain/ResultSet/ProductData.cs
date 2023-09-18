using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class ProductData
    {
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public bool? ShowOnHomePage { get; set; }
        public string CategoryImage { get; set; }
        public string SubCategoryImage { get; set; }
        public string ProductName { get; set; }
        public string FullDescription { get; set; }
        public string PermaLink { get; set; }
        public string ProductImages { get; set; }
        public bool ProductShowOnHomePage { get; set; }
        public decimal? Price { get; set; }
        public decimal? SpecialPrice { get; set; }

    }
}
