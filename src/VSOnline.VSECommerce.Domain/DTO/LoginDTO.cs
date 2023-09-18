using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class LoginDTO
    {
        public string? username { get; set; }
        public string? password { get; set; }
        public string? grant_type { get; set; }
    }
}
