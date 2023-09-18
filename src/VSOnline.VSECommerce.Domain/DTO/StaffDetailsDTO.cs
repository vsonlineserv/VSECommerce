using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class StaffDetailsDTO
    {
        public string? Email { get; set; }
        public string? ApplicationName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string licenseDetailsId { get; set; }
        public int StoreId { get; set; }
        public int BranchId { get; set; }
        public int StoreReferenceId { get; set; }
        public string AuthIdentifier { get; set; }
        public List<int>? PermissionIds { get; set; }
    }
}
