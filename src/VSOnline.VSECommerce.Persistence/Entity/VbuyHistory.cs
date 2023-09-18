using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class VbuyHistory
    {
        [Key]
        public int Slno { get; set; }
        public string? TableName { get; set; }
        public string? ColumnName { get; set; }
        public string? OldValue { get; set; }
        public string? newValue { get; set; }
        public int ModifyValueID { get; set; }
        public Nullable<System.DateTime> modifiedDate { get; set; }
        public string? UserId { get; set; }

    }
}
