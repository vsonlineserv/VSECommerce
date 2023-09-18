using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class ProductKeyFeatures
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Parameter { get; set; }
        public string? KeyFeature { get; set; }
        public int DisplayOrder { get; set; }
    }
}
