using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class RetailerUserDTO
    {
        [Required]
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [Required]
        public string StoreName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber1 { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }
        public string? Pincode { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? StoreId { get; set; }
        public int? BranchId { get; set; }
        public int StoreRefereneId { get; set; }
        public string StoreCategoryExcelFile { get; set; }
    }
}
