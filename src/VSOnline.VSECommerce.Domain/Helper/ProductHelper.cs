using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Helper
{
    public class ProductHelper
    {
        private IMapper _mapper;
        public ProductHelper(IMapper mapper)
        {
            _mapper = mapper;
        }

        public string GetAllCategory(string currentUser)
        {

            var query = @"
				WITH branchCte AS
                    (select  storeId from Seller where CreatedUser= '{currentUser}'),
                    SubCategorieCte as
                    (
                    select category from SellerCategory inner join branchCte on SellerCategory.seller=branchCte.StoreId  where SellerCategory.seller=branchCte.storeId 
                    union   select Category.CategoryId from Category,SellerCategory where (SellerCategory.Category is null)
                    ),
                    CategorieCte AS
                    (
                    select distinct ParentCategoryId  from Category,SubCategorieCte where (CategoryId=category )
                    ),
                    ResultCte AS
                    (
                    select categoryId ,Name from Category,CategorieCte c where (categoryId=c.ParentCategoryId and Category.ParentCategoryId is null)
                    )
                    select *from ResultCte".FormatWith(new { currentUser });
            return query;

        }
        public string GetAllSubCategory(string currentUser)
        {

            var query = @"WITH branchCte AS
                    (
                    select  storeId from Seller where CreatedUser= '{currentUser}'
                    ),
                    SubCategorieCte as
                    (
                    select category from SellerCategory inner join branchCte on SellerCategory.seller=branchCte.StoreId  where SellerCategory.seller=branchCte.storeId 
                    union select Category.CategoryId from Category,SellerCategory where (SellerCategory.Category is null and Category.ParentCategoryId is not null )
                    )
                    select distinct CategoryId,Name,ParentCategoryId from Category,SubCategorieCte where CategoryId=category
			".FormatWith(new { currentUser });
            return query;

        }
        public List<ProductModel> GetProductModelFromProductList(List<Product> products)
        {
            List<ProductModel> productModelList = new List<ProductModel>();
            var productdetailsList = _mapper.Map<List<Product>, List<ProductModel>>(products, productModelList);
            return productdetailsList;
        }
        public List<ProductModelWithCategory> GetProductModelWithCategoryFromProductList(List<Product> products)
        {
            List<ProductModelWithCategory> productModelList = new List<ProductModelWithCategory>();
            _mapper.Map<IEnumerable<Product>, IEnumerable<ProductModelWithCategory>>(products, productModelList);

            return productModelList;
        }
        public List<BrandFilterDTO> GetBrandFilterFromBrands(List<Manufacturer> brandlist)
        {
            List<BrandFilterDTO> brandfilterList = new List<BrandFilterDTO>();
            _mapper.Map<IEnumerable<Manufacturer>, IEnumerable<BrandFilterDTO>>(brandlist, brandfilterList);
            return brandfilterList;
        }
        public string GetRowNumberQuery(string orderByField, int? pageStart, int? pageSize)
        {
            var rownumStart = pageStart ?? 0;
            var rownumEnd = (rownumStart + pageSize) > 0 ? (rownumStart + pageSize) : 500;

            return @" SELECT  *
					FROM    ( SELECT    ROW_NUMBER() OVER ( ORDER BY {OrderByField} ) AS RowNum, COUNT(*) OVER() TotalCount, *
							  FROM      Results
							) AS RowConstrainedResult
					{rowQuery}".FormatWith(new { OrderByField = orderByField, rowQuery = (pageStart != null && pageSize != null) ? "WHERE   RowNum >= " + rownumStart + " AND RowNum < " + rownumEnd + " ORDER BY RowNum" : "" });
        }
        public string GetRowNumberQueryNew(string orderByField, int? pageStart, int? pageSize)
        {
            var rownumStart = pageStart ?? 0;
            var rownumEnd = (rownumStart + pageSize) > 0 ? (rownumStart + pageSize) : 500;

            return @" SELECT  *
					FROM    ( SELECT  COUNT(*) OVER() TotalCount, *
							  FROM      Results
							) AS RowConstrainedResult
					".FormatWith(new { OrderByField = orderByField, rowQuery = (pageStart != null && pageSize != null) ? "WHERE   RowNum >= " + rownumStart + " AND RowNum < " + rownumEnd + " ORDER BY RowNum" : "" });
        }
    }
}
