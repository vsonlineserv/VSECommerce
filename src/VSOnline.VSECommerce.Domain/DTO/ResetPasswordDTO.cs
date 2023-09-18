using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class ResetPasswordDTO
    {
        public string UniqueId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
