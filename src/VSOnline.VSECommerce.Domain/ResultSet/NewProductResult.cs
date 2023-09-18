using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class NewProductResult
    {
        public Enums.UpdateStatus Status { get; set; }

        public Product? NewProduct { get; set; }
        public int ProductId { get; set; } //added for product variant implementation
        public string StatusString
        {
            get
            {
                return Status.ToString();
            }
        }
    }

    public class BaseUpdateResultSet
    {
        public Enums.UpdateStatus Status { get; set; }
        public int UpdatedId { get; set; }
        public string StatusString
        {
            get
            {
                return Status.ToString();
            }
        }
    }
}
