using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class SelectedProductFilterDTO
    {
        public SelectedProductFilterList SelectedProductFilterList { get; set; }

    }
    public class SelectedProductFilterList
    {
        public string FilterParameter { get; set; }
        public string FilterValueText { get; set; }
    }
}

