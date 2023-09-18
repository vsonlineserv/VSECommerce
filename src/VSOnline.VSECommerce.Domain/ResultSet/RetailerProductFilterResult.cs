using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class RetailerProductFilterResult
    {
        public List<CategoryFilterDTO> CategoryFilter { get; set; }
        public List<SubCategoryFilterDTO> SubCategoryFilter { get; set; }
        public List<KeyValuePair<int, string>> Brands { get; set; }
        public RetailerProductSelectedFilter SelectedFilters { get; set; }
    }
    public class RetailerProductSelectedFilter
    {
        public int? SelectedCategory { get; set; }
        public int? SelectedSubCategory { get; set; }
        public int? SelectedBrand { get; set; }
    }
    public class CategoryFilterDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
    }

    public class SubCategoryFilterDTO
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public Nullable<int> ParentCategoryId { get; set; }
    }
    public class SelectedFilter
    {
        public List<BrandListFilter> SelectedBrandList { get; set; }
        public int[] SelectedBrandIdList { get; set; }
        public int SortById { get; set; }
    }
    public class BrandListFilter
    {
        public int Id;
    }
}
