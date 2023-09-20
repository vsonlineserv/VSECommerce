using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class ResetPasswordMerchantDTO
    {
        public string UserName { get; set; }
        public string NewPassword { get; set; }
        public string PasswordResetToken { get; set; }
    }
}
