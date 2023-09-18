using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class ProductRating
    {
        [Key]
        public int id { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; } 
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }
        public int User { get; set; }
    }
}
