using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.ResultSet;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class ShoppingCartItemListDTO
    {
        public List<ShoppingCartResult> shoppingCartDTOList { get; set; }
        public string UserName { get; set; }
        public string? couponCode { get; set; }
    }
}
