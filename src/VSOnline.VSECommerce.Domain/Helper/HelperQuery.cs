using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Helper
{
    public class HelperQuery
    {
        public string GenerateFilterTempTable(List<SelectedProductFilterList> filters)
        {
            StringBuilder queryBuilder = new StringBuilder();
            var uniqueParamters = filters.Select(o => o.FilterParameter).Distinct().ToList();
            var count = 1;
            foreach (var parameter in uniqueParamters)
            {
                var CMF = "CMF" + count;
                var filterValueTexts = filters.Where(p => p.FilterParameter == parameter).Select(v => v.FilterValueText).Distinct().ToList();
                queryBuilder.Append(@"select ProductId from ProductFilterValue 
                    Inner Join [CategoryMasterFilter] AS {CMF} ON ProductFilterValue.CategoryMasterFilter = [{CMF}].Id
                      AND [{CMF}].FilterParameter = '{parameter}' AND FilterValueText in(".FormatWith(new { parameter, CMF }));
                foreach (var filterValueText in filterValueTexts)
                {
                    queryBuilder.Append(@" '{filterValueText}',".FormatWith(new { filterValueText }));
                }

                queryBuilder.Remove(queryBuilder.Length - 1, 1);
                queryBuilder.Append(" ) ");
                queryBuilder.Append(" INTERSECT ");
                count = count + 1;
            }
            queryBuilder.Remove(queryBuilder.Length - 11, 11);
            return queryBuilder.ToString();

        }
    }
}
