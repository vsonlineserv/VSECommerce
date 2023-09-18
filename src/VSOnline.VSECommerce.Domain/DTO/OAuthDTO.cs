using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class OAuthDTO
    {
        public string grant_type { get; set; }
        public string tokenOrigin { get; set; }
        public string username { get; set; }
        public string password { get; set; }
    }
}
