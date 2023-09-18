using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class UserResult
    {
        public int UserId { get; set; } 
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber1 { get; set; }
        public string PhoneNumber2 { get; set; }
        public string Password { get; set; }
        public int PasswordFormatId { get; set; }
        public string PasswordSalt { get; set; }
        public bool IsMerchant { get; set; }
        public bool Active { get; set; }
        public bool Deleted { get; set; }
        public string LastIpAddress { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }
        public Nullable<System.DateTime> LastLoginDateUtc { get; set; }
        public Nullable<int> BillingAddress_Id { get; set; }
        public Nullable<int> ShippingAddress_Id { get; set; }
        public string? UserProfileImage { get; set; }

    }
}
