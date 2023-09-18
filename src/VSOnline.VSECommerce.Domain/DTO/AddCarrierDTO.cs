using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class AddCarrierDTO
    {
        public string? CarrierName { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
