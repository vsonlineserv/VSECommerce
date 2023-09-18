using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class BuyerAddressResult
    {
        public int AddressId { get; set; }
        public string UserName { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string? Country { get; set; }
    }
}
