using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class TokenResult
    {
        public string AccessToken { get; set; }
        public DateTime ValidDateUTC { get; set; }
    }
}
