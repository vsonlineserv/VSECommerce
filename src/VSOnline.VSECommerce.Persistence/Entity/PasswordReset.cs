using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class PasswordReset
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordResetToken { get; set; }
        public System.DateTime PasswordResetExpiration { get; set; }
        public bool FlagCompleted { get; set; }
        public int? StoreId { get; set; }
        public bool? IsVbuyUser { get; set; }   
    }
}
