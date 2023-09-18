using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class UserModelDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        [Required]
        public string? PhoneNumber1 { get; set; }
        [Required]
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? AuthToken { get; set; }
        public int StoreId { get; set; }
        public int BranchId { get; set; }
        public int? StoreRefereneId { get; set; }

        public List<int>? permissionIds { get; set; }
    }
}
