using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class CategoryModelDTO
    {
        public int CategoryId { get; set; }
        public string? Name { get; set; }
        public string? CategoryGroupTag { get; set; }
        public Nullable<int> GroupDisplayOrder { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool flagTopCategory { get; set; }   
        public bool? FlagShowBuy { get; set; }
        public Nullable<int> SelectedCategory { get; set; }
    }
}
