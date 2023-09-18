using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class ProductSpecification
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? SpecificationGroup { get; set; }
        public string? SpecificationAttribute { get; set; }
        public string? SpecificationDetails { get; set; }
        public int DisplayOrder { get; set; }
    }
}
