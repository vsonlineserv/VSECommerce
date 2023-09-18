using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Utilities
{
    public static class Extender
    {
        public static bool HasItems(this IEnumerable source)
        {
            return source != null && source.GetEnumerator().MoveNext();
        }
    }
}
