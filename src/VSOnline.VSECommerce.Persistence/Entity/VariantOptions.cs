using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class VariantOptions
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Options { get; set; }
        public bool FlagDeleted { get; set; }
        public Nullable<System.DateTime> CreatedOnUtc { get; set; }
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }
    }
}
