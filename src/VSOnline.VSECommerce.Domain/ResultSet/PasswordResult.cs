using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class PasswordResult
    {
        public byte[]? Password { get; set; }
        public string? PasswordSalt { get; set; }
    }
}
