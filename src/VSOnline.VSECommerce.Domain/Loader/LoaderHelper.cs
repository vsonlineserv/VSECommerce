using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.Loader
{
    public class LoaderHelper
    {
        public static string GetColumn(DataRow Row, int Ordinal)
        {
            return Row.Table.Columns[Ordinal].ColumnName;
        }
    }
}
