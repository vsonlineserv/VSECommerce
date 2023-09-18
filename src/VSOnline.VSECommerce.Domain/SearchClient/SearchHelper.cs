using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;

namespace VSOnline.VSECommerce.Domain
{
    public class SearchHelper
    {
        public List<ProductModel> GetProductModelFromSearchProductModelResult(ISearchResponse<productmodelelasticsearch> result)
        {
            List<ProductModel> resultProductModelList = new List<ProductModel>();

            if (result.Hits.Any())
            {
                foreach (var hit in result.Hits)
                {
                    ProductModel productModel = new ProductModel();
                    productModel.ProductId = hit.Source.Id;
                    productModel.Name = hit.Source.Name;

                    resultProductModelList.Add(productModel);

                    if (!hit.Highlight.Any()) continue;
                    foreach (var highlight in hit.Highlight)
                    {
                        //TODO:
                    }


                }
            }
            return resultProductModelList;
        }
    }
}
