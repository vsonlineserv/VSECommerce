using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class CategoryResult
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public string? MetaTitle { get; set; }
        public Nullable<int> ParentCategoryId { get; set; }
        public string? CategoryGroupTag { get; set; }
        public Nullable<int> GroupDisplayOrder { get; set; }
        public Nullable<bool> Published { get; set; }
        public Nullable<bool> IsDeleted { get; set; }
        public int? DisplayOrder { get; set; }
        public bool? FlagShowBuy { get; set; }
        public bool? ShowOnHomePage { get; set; }
        public Nullable<System.DateTime> CreatedOnUtc { get; set; }
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }
        public string? CategoryImage { get; set; }
        public string? PermaLink { get; set; }
    }
}
