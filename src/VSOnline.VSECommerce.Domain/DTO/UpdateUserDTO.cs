using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class UpdateUserDTO
    {
        public string FirstName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? LastName { get; set; }
        public string? PictureName { get; set; }
        public IFormFile? ImageFile { get; set; }
    }
}
