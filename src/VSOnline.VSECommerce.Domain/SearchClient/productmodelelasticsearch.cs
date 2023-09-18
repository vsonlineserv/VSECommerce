using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain
{
    public class productmodelelasticsearch
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public string SubCategoryName { get; set; }
        public string MetaKeywords { get; set; }
        public string CategoryGroupTag { get; set; }
        public string SubCategoryMetaKeywords { get; set; }
        public string BrandName { get; set; }
        public string BrandMetaKeywords { get; set; }
    }
}
