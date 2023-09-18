using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class Permissions
    {
        public int Id { get; set; }
        public string Permission { get; set; }
        public string? Name { get; set; }   
    }
}
