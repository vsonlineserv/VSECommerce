using System;
namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class BillingAddressResult
    {
        public string User { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int PostalCode { get; set; }
        public string PhoneNumber { get; set; }
    }
}

