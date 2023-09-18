using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class SiteSettings
    {
        [Key]
        public string SiteKey { get; set; }
        public string Value { get; set; }
    }
}
