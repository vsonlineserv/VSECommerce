using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class CategoryWithChildren
    {
        public string? Name { get; set; }
        public int Id { get; set; }
        public string? CategoryImage { get; set; }
        public string PermaLink { get; set; }
        public List<MainCategory> Children { get; set; }
    }
    public class MainCategory
    {
        public string? Name { get; set; }
        public int Id { get; set; }
        public string? CategoryImage { get; set; }
        public int ParentCategoryId { get; set; }   
        public string? PermaLink { get; set; }
    }
}
