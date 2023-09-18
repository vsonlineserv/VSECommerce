using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class RetailerInfoResult
    {
        public string? StoreName { get; set; }
        public int StoreId { get; set; }
        public int PrimaryContact { get; set; } 
        public string? Description { get; set; }
        public string? LogoPicture { get; set; }
        public List<BranchResults> Branches { get; set; }
    }
    public class BranchResults
    {
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public bool? EnableBuy { get; set; }
        public decimal? BranchRating { get; set; }
        public int? RatingCount { get; set; }   
        public string? StoreType { get; set; }   
    }
}
