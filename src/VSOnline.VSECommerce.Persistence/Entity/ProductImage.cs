using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? PictureName { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
