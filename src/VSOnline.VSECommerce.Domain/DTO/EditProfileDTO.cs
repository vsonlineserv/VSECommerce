using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class EditProfileDTO
    {
        public int UserId { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber1 { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
