using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain
{
    public class ProductRepository
    {
        private readonly DataContext _context;
        private readonly ProductHelper _productHelper;
        private readonly HelperQuery _helperQuery;
        private readonly SearchClient _searchClient;
        private readonly IMapper _mapper;

        public ProductRepository(SearchClient searchClient, HelperQuery helperQuery, ProductHelper productHelper, DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _productHelper = productHelper;
            _helperQuery = helperQuery;
            _searchClient = searchClient;
        }
        public ProductStoreMapping DeleteProductInProduct(int productId, int BranchId)
        {
            try
            {
                var productDetails = _context.Product.Where(x => x.ProductId == productId && x.FlagSampleProducts == false).FirstOrDefault();
                if (productDetails != null)
                {
                    productDetails.IsDeleted = true;
                    productDetails.UpdatedOnUtc = DateTime.UtcNow;
                    _context.Product.Update(productDetails);
                    _context.SaveChanges();
                }
                var product = _context.ProductStoreMapping.Where(x => x.ProductId == productId && x.BranchId == BranchId).FirstOrDefault();
                if (product != null)
                {
                    product.IsDeleted = true;
                    product.UpdatedOnUtc = DateTime.UtcNow;
                    _context.ProductStoreMapping.Update(product);
                    _context.SaveChanges();
                    return product;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public List<ProductModel> GetFeaturedProducts(List<int> branchIdList)
        {

            var productsDetails = _context.ProductStoreMapping.Join(_context.Pricing,
                Product => new { id1 = Product.ProductId, id2 = Product.BranchId },
                Pricing => new { id1 = Pricing.Product, id2 = Pricing.Branch },
                (Product, Pricing) => new { Product, Pricing }).Where(x => x.Product.ShowOnHomePage == true && x.Product.Published == true
                && x.Product.IsDeleted != true && branchIdList.Contains((int)x.Product.BranchId)).Select(y => new ProductModel
                {
                    ProductId = y.Product.ProductId,
                    Name = y.Product.Name,
                    PictureName = _context.ProductImage.Where(x => x.ProductId == y.Product.ProductId).Select(x => x.PictureName).FirstOrDefault(),
                    Price = y.Pricing.Price,
                    SpecialPrice = y.Pricing.SpecialPrice,
                    FullDescription = y.Product.FullDescription,
                    Category = y.Product.Category,
                    PermaLink = y.Product.PermaLink,
                    StoresCount = y.Product.PricingCollection.ToList().Count(),
                    BranchId = y.Product.BranchId,
                }).ToList();
            return productsDetails;
        }
        public string GetOffersQuery(List<int> branchIdList, int limit, int? userId)
        {
            var query = @"   
                    WITH ProductCTE AS 
                    (
                     Select ParentCategoryId, CategoryId, Category.Name AS SubCategoryName, ProductId, Product.Name as Name, productimage.PictureName,Product.PermaLink, Max(Price) AS Price,
                      Pricing.Branch, Pricing.AdditionalShippingCharge,Pricing.DeliveryTime, SellerBranch.BranchName
                               From ProductStoreMapping as Product
                                            Inner Join Category ON Category.CategoryId = Product.Category
                                            INNER Join Pricing ON Pricing.Product = Product.ProductId and Pricing.Branch = Product.BranchId
                                            Left join SellerBranch on Pricing.Branch = SellerBranch.BranchId
                                            OUTER apply
                                                    (
                                                    select top 1 PictureName from ProductImage where ProductId = Product.ProductId
                                                    )
                                                    productimage
                                             WHERE ( NOT ((1 = [Product].[IsDeleted]) AND ([Product].[IsDeleted] IS NOT NULL))) 
                                                                                                   
                                                    AND (SpecialPriceStartDateTimeUtc <= '{OfferLimitStartDate}' AND  SpecialPriceEndDateTimeUtc >=  '{CurrentDate}'
                                                    AND GETUTCDATE() BETWEEN SpecialPriceStartDateTimeUtc AND SpecialPriceEndDateTimeUtc 
                                                    )	  
                                                    AND ISNULL( ((1-(Pricing.SpecialPrice / NULLIF(Pricing.Price,0) ))*100),0) >5                                                      
		                                                                                  AND (1 = [Product].[Published])
	                                                                                      And  NOT ((1 = pricing.[IsDeleted]) AND (pricing.[IsDeleted] IS NOT NULL))
                    GROUP BY ParentCategoryId, CategoryId,  Category.Name , ProductId, Product.Name,productimage.PictureName, Product.PermaLink,Pricing.Branch,Pricing.AdditionalShippingCharge,SellerBranch.BranchName,Pricing.DeliveryTime
                    )
                    ,ProductWishlist AS
                     (

                    select   ProductId, UserWishlist.Id, CASE WHEN (UserWishlist.Id) IS NULL 
                                    THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END 
                                        AS FlagWishlist
                    From ProductCTE 
	                    INNER JOIN UserWishlist ON ProductCTE.ProductId = UserWishlist.Product
											                    AND UserWishlist.[User] = {userId}

                    )
                    ,
                    BranchCTE AS
                    (
	                    SELECT DISTINCT Pricing.Product Product, Count(Pricing.Branch) StoresCount, Min(SpecialPrice) SpecialPrice 
                        FROM Pricing 
	                    Inner Join ProductCTE ON Pricing.Product = ProductCTE.ProductId
                        
	                    WHERE Pricing.Branch IN ({branchIdListFilter})
                         
	                    Group BY Pricing.Product
                    )
                    Select TOP {topLimit} ParentCategoryId, CategoryId, SubCategoryName, ProductCTE.ProductId, Name,ProductCTE.PictureName,ProductCTE.PermaLink, ProductCTE.Branch as BranchId, Price, ISNULL(StoresCount,0) StoresCount
                    , ISNULL(SpecialPrice,0) SpecialPrice
                    ,FlagWishlist, ProductCTE.AdditionalShippingCharge, ProductCTE.BranchName,ProductCTE.DeliveryTime
                    FROM ProductCTE
                    INNER JOIN BranchCTE ON ProductCTE.ProductId = BranchCTE.Product
                    LEFT JOIN ProductWishlist ON ProductCTE.ProductId = ProductWishlist.ProductId
                     WHERE ProductCTE.Branch IN ({branchIdListFilter})"
                .FormatWith(new
                {
                    branchIdListFilter = branchIdList.Count > 0 ? string.Join(",", branchIdList) : "0",
                    OfferLimitStartDate = DateTime.UtcNow.Date.AddDays(1).ToString("yyyy-MM-dd"),
                    CurrentDate = DateTime.UtcNow.Date.ToString("yyyy-MM-dd"),
                    topLimit = limit > 0 ? limit : 100,
                    userId = userId

                });
            return query;
        }
        public List<BrandFilterDTO> GetBrandFilter(List<int> prodList)
        {
            var brandList = _context.ProductStoreMapping.Where(x => prodList.Contains(x.ProductId)).Include(y => y.ManufacturerDetails).Select(y => y.ManufacturerDetails)
            .Distinct()
            .ToList<Manufacturer>();

            return _productHelper.GetBrandFilterFromBrands(brandList);
        }
        public string GetProductsQuery_WithDescription(SelectedFilter filter, List<SelectedProductFilterList> selectedProductFilter,
        int[] categoryIdList, List<int> branchIdList, int? priceRangeFrom, int? priceRangeTo, int? pageStart, int? pageSize, int? userId, bool Isvbuy)
        {
            string SelectedFilterDetailsCTE = "";
            if (selectedProductFilter != null)
            {
                if (selectedProductFilter.Count() > 0)
                {
                    SelectedFilterDetailsCTE = _helperQuery.GenerateFilterTempTable(selectedProductFilter);
                }
            }
            if (SelectedFilterDetailsCTE.Length <= 0)
            {
                SelectedFilterDetailsCTE = "Select NULL Dummy";
            }
            var query = @"   
                    WITH SelectedFilterDetails 
					AS 
					(
					{SelectedFilterDetailsCTE}
					)                    
                    , ProductCTE AS 
                    (
                     Select ParentCategoryId, CategoryId, Category.Name AS SubCategoryName, Category.FlagShowBuy AS FlagShowBuy,
                    Product.ProductId, Product.Name as Name, Product.FullDescription AS FullDescription, productimage.PictureName, Product.PermaLink
                               From {productTable} as Product
                                            Inner Join Category ON Category.CategoryId = Product.Category
                                            {SelectedFilterDetailsCTEJoin}
                                            OUTER apply
                                                        (
                                                        select top 1 PictureName from ProductImage where ProductId = Product.ProductId
                                                        )
                                                        productimage
                                            
                                             WHERE ( NOT ((1 = [Product].[IsDeleted]) AND ([Product].[IsDeleted] IS NOT NULL))) 
                                                                                          {BrandFilter}  
		                                                                                  AND (1 = [Product].[Published]) AND ([Product].[Category] IN ({categoryIdListArray}))																 
                    GROUP BY ParentCategoryId, CategoryId,  Category.Name , FlagShowBuy, Product.ProductId, Product.Name,productimage.PictureName, Product.FullDescription, Product.PermaLink
                    )
                     ,ProductWishlist AS
                     (

                    select   ProductId, UserWishlist.Id, CASE WHEN (UserWishlist.Id) IS NULL 
                                    THEN CAST(0 AS BIT) ELSE CAST(1 AS BIT) END 
                                        AS FlagWishlist
                    From ProductCTE 
	                    INNER JOIN UserWishlist ON ProductCTE.ProductId = UserWishlist.Product
											                    AND UserWishlist.[User] = {userId}

                    )
                    ,ParentCategoryCTE AS 
					(
					 Select DISTINCT Category.Name AS ParentCategoryName, ProductCTE.ParentCategoryId 
					 FROM Category
					 Inner Join ProductCTE ON Category.CategoryId = ProductCTE.ParentCategoryId
					)
	                ,MinBranchCTE AS
					(
					Select  price,SpecialPrice, Pricing.Branch,SellerBranch.BranchName, Pricing.Product, Pricing.DeliveryTime, 
					Pricing.AdditionalShippingCharge, SellerBranch.EnableBuy, ProductCTE.FlagShowBuy,Count(Branch) StoresCount
					from Pricing 
					Inner Join SellerBranch on SellerBranch.BranchId = Pricing.Branch
                    INNER JOIN ProductCTE ON ProductCTE.ProductId = Pricing.Product
	                WHERE Branch IN ({branchIdListFilter})
                    {SpecialPricingFilter}
                    AND [Pricing].[IsDeleted] IS NULL OR [Pricing].[IsDeleted] =0
                    AND SellerBranch.EnableBuy =1
                    AND ProductCTE.FlagShowBuy =1
					Group BY Pricing.Product,Pricing.Price,Pricing.SpecialPrice,Pricing.Branch,Pricing.DeliveryTime,Pricing.AdditionalShippingCharge
					 ,SellerBranch.BranchName,SellerBranch.EnableBuy,ProductCTE.FlagShowBuy
					)
                , Results AS 
					(
                    Select ProductCTE.ParentCategoryId CategoryId, ParentCategoryName, CategoryId AS SubCategoryId, SubCategoryName, 
                    ProductCTE.ProductId, Name, FullDescription, ProductCTE.PictureName, PermaLink
                    , FlagWishlist
	                ,MinBranchCTE.Branch,  MinBranchCTE.BranchName, MinBranchCTE.Product, MinBranchCTE.DeliveryTime, 
					MinBranchCTE.AdditionalShippingCharge, MinBranchCTE.EnableBuy, MinBranchCTE.FlagShowBuy,MinBranchCTE.Price,MinBranchCTE.SpecialPrice,
                    ISNULL(StoresCount,0) StoresCount
                    FROM ProductCTE
                    INNER JOIN ParentCategoryCTE ON ParentCategoryCTE.ParentCategoryId = ProductCTE.ParentCategoryId                    
                    LEFT JOIN ProductWishlist ON ProductCTE.ProductId = ProductWishlist.ProductId     
                    LEFT JOIN MinBranchCTE ON MinBranchCTE.Product =  ProductCTE.ProductId
                    WHERE Branch IN ({branchIdListFilter})
                     {SpecialPricingFilter}
                    {PriceLowToHighFilterClause}              
                    )
                    {GetRowNumberQuery}
                    "
                .FormatWith(new
                {
                    productTable = Isvbuy == true ? "Product" : "ProductStoreMapping",
                    categoryIdListArray = string.Join(",", categoryIdList),
                    branchIdListFilter = branchIdList.Count > 0 ? string.Join(",", branchIdList) : "0",
                    SpecialPricingFilter = " AND [Price] >= {priceRangeFrom} AND [Price] <= {priceRangeTo}".FormatWith(new { priceRangeFrom, priceRangeTo }),
                    PricingFilter = " AND [SpecialPrice] >= {priceRangeFrom} AND [SpecialPrice] <= {priceRangeTo}".FormatWith(new { priceRangeFrom, priceRangeTo }),
                    BrandFilter = (filter != null && filter.SelectedBrandList != null && filter.SelectedBrandList.Count > 0) ? " AND [Product].[Manufacturer] IN ( " +
                    string.Join(",", filter.SelectedBrandIdList) + " )" : "",
                    GetRowNumberQuery = _productHelper.GetRowNumberQuery(GetSortByFieldBasedOnEnumForProductQuery(filter), pageStart, pageSize),
                    PriceLowToHighFilterClause = (filter != null && filter.SortById > 0 && filter.SortById < 4) ? " And SpecialPrice > 0 " : "",
                    userId = userId,
                    SelectedFilterDetailsCTE,
                    SelectedFilterDetailsCTEJoin = (selectedProductFilter != null && selectedProductFilter.Count() > 0) ? @"Inner Join ProductFilterValue ON ProductFilterValue.ProductId = Product.ProductId
											 Inner Join [CategoryMasterFilter] ON ProductFilterValue.CategoryMasterFilter = [CategoryMasterFilter].Id
											 INNER JOIN SelectedFilterDetails ON Product.ProductId = SelectedFilterDetails.ProductId
											 " : ""
                });
            return query;
        }
        public string GetSortByFieldBasedOnEnumForProductQuery(SelectedFilter filter)
        {
            var sortById = (filter != null && filter.SortById > 0) ? filter.SortById : (int)Enums.SortBy.StoresCount;
            switch (sortById)
            {

                case 2:
                    {
                        return "SpecialPrice ASC";
                    }
                case 3:
                    {
                        return "SpecialPrice DESC";
                    }
                case 4:
                    {
                        return "Name ASC";
                    }
                case 5://NameDESC
                    {
                        return "Name DESC";
                    }
                //stores count.
                default:
                case 1:
                    {
                        return "StoresCount DESC";
                    }
            }
        }
        public ProductDetailModel GetProductDetails(int productId)
        {
            var product = _context.ProductStoreMapping.Where(x => x.IsDeleted != true && x.Published == true && x.ProductId == productId).Include(y => y.ManufacturerDetails)
                .FirstOrDefault<ProductStoreMapping>();

            ProductDetailModel productDetailModel = new ProductDetailModel();
            if (product != null)
            {
                _mapper.Map<ProductStoreMapping, ProductDetailModel>(product, productDetailModel);
                productDetailModel.BrandName = product.ManufacturerDetails != null ? product.ManufacturerDetails.Name : String.Empty;
            }
            return productDetailModel;
        }
        public List<ProductModel> GetSearchProductsFilter(string searchString, bool enableElastic, List<int> branchIdList)
        {

            var products = _context.ProductStoreMapping.Where(x => x.IsDeleted != true && x.Published == true && x.FlagSampleProducts == false && branchIdList.Contains(x.BranchId) && (x.Name.Contains(searchString) || x.ManufacturerDetails.Name.Contains(searchString)))
             .Include(y => y.ManufacturerDetails).ToList<ProductStoreMapping>();
            List<ProductModel> productModelList = new List<ProductModel>();
            var productdetailsList = _mapper.Map<List<ProductStoreMapping>, List<ProductModel>>(products, productModelList);
            return productdetailsList;

        }
        public List<ProductModel> GetSearchProductsFilter(int BranchId, string searchString, bool enableElastic)
        {

            var products = _context.ProductStoreMapping.Where(x => x.IsDeleted != true && x.Published == true && x.BranchId == BranchId && (x.Name.Contains(searchString) || x.ManufacturerDetails.Name.Contains(searchString)))
                .Include(y => y.ManufacturerDetails).ToList<ProductStoreMapping>();
            List<ProductModel> productModelList = new List<ProductModel>();
            var productdetailsList = _mapper.Map<List<ProductStoreMapping>, List<ProductModel>>(products, productModelList);
            return productdetailsList;

        }
        public string GetSearchCatalogueQueryWithBuy(SelectedFilter filter, string productFilter, List<int> branchIdList, int? priceRangeFrom, int? priceRangeTo, int? pageStart, int? pageSize)
        {
            var query = @"WITH ProductCTE AS 
                    (
                     Select ParentCategoryId, CategoryId, Category.Name AS SubCategoryName, ProductId, Product.Name as Name, productimage.PictureName, Product.PermaLink, Max(Price) AS Price, AdditionalShippingCharge, SellerBranch.BranchId, SellerBranch.BranchName
                               From ProductStoreMapping as Product
                                            Inner Join Category ON Category.CategoryId = Product.Category
                                            LEFT Join Pricing ON Pricing.Product = Product.ProductId
                                            LEFT JOIN Manufacturer ON Product.Manufacturer = Manufacturer.ManufacturerId
                                            LEFT JOIN SellerBranch ON Pricing.Branch = SellerBranch.BranchId
                                             OUTER apply
                                                        (
                                                        select top 1 PictureName from ProductImage where ProductId = Product.ProductId
                                                        )
                                                        productimage
                                             WHERE ( NOT ((1 = [Product].[IsDeleted]) AND ([Product].[IsDeleted] IS NOT NULL))) 
                                                                                         
                                                                                          {SpecialPricingFilter}     
                                                                                          {BrandFilter}                                                          
		                                                                                  AND (1 = [Product].[Published]) AND
                                                                            ([Product].Name LIKE '{productNameFilter}'
                                                                                OR Manufacturer.Name LIKE '{productNameFilter}') AND [Product].FlagSampleProducts = 0														 
                    GROUP BY ParentCategoryId, CategoryId,  Category.Name , ProductId, Product.Name,productimage.PictureName,Product.PermaLink, Pricing.AdditionalShippingCharge, SellerBranch.BranchId, SellerBranch.BranchName
                    )
                    ,
                    BranchCTE AS
                    (
	                    SELECT DISTINCT Branch, Pricing.Product Product, Count(Branch) StoresCount, Min(SpecialPrice) SpecialPrice 
                        FROM Pricing 
	                    Inner Join ProductCTE ON Pricing.Product = ProductCTE.ProductId
                        
	                    WHERE Branch IN ({branchIdListFilter})
                        {SpecialPricingFilter}
                        AND [Pricing].[IsDeleted] IS NULL OR [Pricing].[IsDeleted] =0
	                    Group BY Pricing.Product,Pricing.Branch
                    )
                , Results AS 
					(
                    Select ParentCategoryId, CategoryId AS SubCategoryId, SubCategoryName, ProductId, Name,ProductCTE.PictureName,ProductCTE.PermaLink, Price, ISNULL(StoresCount,0) StoresCount
                    , ISNULL(SpecialPrice,0) SpecialPrice, AdditionalShippingCharge, ProductCTE.BranchId, ProductCTE.BranchName
                    FROM ProductCTE
                    LEFT JOIN BranchCTE ON ProductCTE.ProductId = BranchCTE.Product and ProductCTE.BranchId = BranchCTE.Branch
                    WHERE Branch IN ({branchIdListFilter})
                    {PriceLowToHighFilterClause} 
                    )                    
                    {GetRowNumberQuery}
                    "
                .FormatWith(new
                {
                    productNameFilter = "%" + productFilter + "%",
                    branchIdListFilter = branchIdList.Count > 0 ? string.Join(",", branchIdList) : "0",
                    PricingFilter = (priceRangeFrom != null && priceRangeTo != null) ? " AND [Pricing].[Price] >= {priceRangeFrom} AND [Pricing].[Price] <= {priceRangeTo}".FormatWith(new { priceRangeFrom, priceRangeTo }) : "",
                    SpecialPricingFilter = (priceRangeFrom != null && priceRangeTo != null) ? " AND [Pricing].[SpecialPrice] >= {priceRangeFrom} AND [Pricing].[SpecialPrice] <= {priceRangeTo}".FormatWith(new { priceRangeFrom, priceRangeTo }) : "",
                    BrandFilter = (filter != null && filter.SelectedBrandList != null && filter.SelectedBrandList.Count > 0) ? " AND [Product].[Manufacturer] IN ( " +
                     string.Join(",", filter.SelectedBrandIdList) + " )" : "",
                    GetRowNumberQuery = _productHelper.GetRowNumberQuery(GetSortByFieldBasedOnEnumForProductQuery(filter), pageStart, pageSize),
                    PriceLowToHighFilterClause = (filter != null && filter.SortById > 0 && filter.SortById < 4) ? " And SpecialPrice > 0 " : ""
                });
            return query;
        }

        public string GetUserWishlistProductsQuery(List<int> branchIdList,
            int? pageStart, int? pageSize, int? userId)
        {
            var query = @"   
                    WITH ProductCTE AS 
                    (
                     Select ParentCategoryId, CategoryId, Category.Name AS SubCategoryName, ProductId, Product.Name as Name ,productimage.PictureName, Product.PermaLink, Max(Price) AS Price
                               From ProductStoreMapping as Product
                                            Inner Join Category ON Category.CategoryId = Product.Category
                                            INNER JOIN UserWishlist ON Product.ProductId = UserWishlist.Product
											                    AND UserWishlist.[User] = {userId}
                                            LEFT Join Pricing ON Pricing.Product = Product.ProductId
                                             OUTER apply
                                            (
                                            select top 1 PictureName from ProductImage where ProductId = Product.ProductId
                                            )
                                            productimage
                                             WHERE ( NOT ((1 = [Product].[IsDeleted]) AND ([Product].[IsDeleted] IS NOT NULL)))                                                                                                                                                   
		                                                                                  AND (1 = [Product].[Published])															 
                                                                                    
                    GROUP BY ParentCategoryId, CategoryId,  Category.Name , ProductId, Product.Name,productimage.PictureName,Product.PermaLink
                    )                     
                    ,ParentCategoryCTE AS 
					(
					 Select DISTINCT Category.Name AS ParentCategoryName, ProductCTE.ParentCategoryId 
					 FROM Category
					 Inner Join ProductCTE ON Category.CategoryId = ProductCTE.ParentCategoryId
					)
                    ,
                    BranchCTE AS
                    (
	                    SELECT DISTINCT Pricing.Product Product, Count(Branch) StoresCount, Min(SpecialPrice) SpecialPrice 
                        FROM Pricing 
	                    Inner Join ProductCTE ON Pricing.Product = ProductCTE.ProductId
                         
	                    WHERE Branch IN ({branchIdListFilter})
                        AND [Pricing].[IsDeleted] IS NULL OR [Pricing].[IsDeleted] =0
	                    Group BY Pricing.Product
                    )
                , Results AS 
					(
                    Select ProductCTE.ParentCategoryId, ParentCategoryName, CategoryId AS SubCategoryId, SubCategoryName, ProductCTE.ProductId, Name,ProductCTE.PictureName,ProductCTE.PermaLink, Price, ISNULL(StoresCount,0) StoresCount
                    , ISNULL(SpecialPrice,0) SpecialPrice
                   , CAST(1 AS BIT) FlagWishlist
                    FROM ProductCTE
                    INNER JOIN ParentCategoryCTE ON ParentCategoryCTE.ParentCategoryId = ProductCTE.ParentCategoryId
                    LEFT JOIN BranchCTE ON ProductCTE.ProductId = BranchCTE.Product
                    )
                    {GetRowNumberQuery}
                    "
                .FormatWith(new
                {

                    branchIdListFilter = branchIdList.Count > 0 ? string.Join(",", branchIdList) : "0",
                    GetRowNumberQuery = _productHelper.GetRowNumberQuery("ProductId", pageStart, pageSize),
                    userId = userId
                });
            return query;
        }

        //product variant
        public NewProductResult CreateNewProductWithVariant(int BranchId, ProductDTOVariant newProductDTO, string userName)
        {
            NewProductResult result = new NewProductResult();
            try
            {
                var product = _context.Product.Where(x => x.Name == newProductDTO.Name && x.Category == newProductDTO.Category && x.IsDeleted == false).FirstOrDefault();
                if (product == null)
                {
                    Product newProduct = new Product();
                    _mapper.Map<ProductDTOVariant, Product>(newProductDTO, newProduct);
                    newProduct.CreatedOnUtc = DateTime.UtcNow;
                    newProduct.UpdatedOnUtc = DateTime.UtcNow;
                    newProduct.FlagSampleProducts = false;
                    newProduct.CreatedBy = userName;
                    newProduct.PermaLink = newProductDTO.Name.ToLower().Replace(" ", "-");
                    _context.Product.Add(newProduct);
                    _context.SaveChanges();
                    result.ProductId = newProduct.ProductId;
                    result.NewProduct = newProduct;

                    // after insert in productstore mapping table
                    ProductStoreMapping productStoreMapping = new ProductStoreMapping();
                    productStoreMapping.ProductId = newProduct.ProductId;
                    productStoreMapping.Name = newProductDTO.Name;
                    productStoreMapping.FullDescription = newProductDTO.FullDescription;
                    productStoreMapping.Manufacturer = newProductDTO.Manufacturer;
                    productStoreMapping.Category = newProductDTO.Category;
                    productStoreMapping.Published = true;
                    productStoreMapping.IsDeleted = false;
                    productStoreMapping.PermaLink = newProductDTO.Name.ToLower().Replace(" ", "-");
                    productStoreMapping.CreatedOnUtc = DateTime.UtcNow;
                    productStoreMapping.StoreId = newProductDTO.StoreId;
                    productStoreMapping.BranchId = BranchId;
                    productStoreMapping.FlagSampleProducts = false;
                    _context.ProductStoreMapping.Add(productStoreMapping);
                    _context.SaveChanges();

                    result.Status = Enums.UpdateStatus.Success;
                    return result;
                }
                else
                {
                    var productStoreDetails = _context.ProductStoreMapping.Where(x => x.ProductId == product.ProductId && x.BranchId == BranchId && x.IsDeleted == false).FirstOrDefault();
                    if (productStoreDetails == null)
                    {
                        ProductStoreMapping productStoreMapping = new ProductStoreMapping();
                        productStoreMapping.ProductId = product.ProductId;
                        productStoreMapping.Name = product.Name;
                        productStoreMapping.FullDescription = product.FullDescription;
                        productStoreMapping.Manufacturer = product.Manufacturer;
                        productStoreMapping.Category = product.Category;
                        productStoreMapping.Published = true;
                        productStoreMapping.IsDeleted = false;
                        productStoreMapping.PermaLink = newProductDTO.Name.ToLower().Replace(" ", "-");
                        productStoreMapping.CreatedOnUtc = DateTime.UtcNow;
                        productStoreMapping.StoreId = newProductDTO.StoreId;
                        productStoreMapping.BranchId = BranchId;
                        productStoreMapping.FlagSampleProducts = false;
                        _context.ProductStoreMapping.Add(productStoreMapping);
                        _context.SaveChanges();
                        result.Status = Enums.UpdateStatus.Success;
                        result.ProductId = productStoreMapping.ProductId;
                        return result;
                    }
                    else
                    {
                        result.Status = Enums.UpdateStatus.AlreadyExist;
                    }
                    return result;
                }
            }
            catch
            {
                result.Status = Enums.UpdateStatus.Error;
                return result;
            }
        }
        public NewProductResult UpdateProductForVariant(int BranchId, ProductDTOVariant updateProductDTO, string currentUser)
        {
            NewProductResult result = new NewProductResult();
            try
            {
                var product = _context.Product.Where(x => x.ProductId == updateProductDTO.ProductId && x.CreatedBy == currentUser).Include(y => y.ManufacturerDetails).Include(z => z.CategoryDetails).FirstOrDefault();
                if (product != null)
                {
                    product.Name = updateProductDTO.Name;
                    product.ShortDescription = updateProductDTO.ShortDescription;
                    product.FullDescription = updateProductDTO.FullDescription;
                    product.Weight = updateProductDTO.Weight;
                    product.Length = updateProductDTO.Length;
                    product.Height = updateProductDTO.Height;
                    product.Width = updateProductDTO.Width;
                    product.Color = updateProductDTO.Color;
                    product.ManufacturerPartNumber = updateProductDTO.ManufacturerPartNumber;
                    product.Manufacturer = updateProductDTO.Manufacturer;
                    product.Category = updateProductDTO.Category;
                    product.Size1 = updateProductDTO.Size1;
                    product.Size2 = updateProductDTO.Size2;
                    product.Size3 = updateProductDTO.Size3;
                    product.Size4 = updateProductDTO.Size4;
                    product.Size5 = updateProductDTO.Size5;
                    product.Size6 = updateProductDTO.Size6;
                    product.PermaLink = updateProductDTO.Name.ToLower().Replace(" ", "-");
                    product.UpdatedOnUtc = DateTime.UtcNow;
                    product.FlagSharedInfo = updateProductDTO.FlagSharedInfo;
                    product.FlagSampleProducts = false;
                    _context.Product.Update(product);
                    _context.SaveChanges();
                    // then change in product store mapping also
                    var productStoreMappingDetail = _context.ProductStoreMapping.Where(x => x.ProductId == updateProductDTO.ProductId && x.BranchId == BranchId).FirstOrDefault();
                    if (productStoreMappingDetail != null)
                    {
                        productStoreMappingDetail.Name = updateProductDTO.Name;
                        productStoreMappingDetail.FullDescription = updateProductDTO.FullDescription;
                        productStoreMappingDetail.Manufacturer = updateProductDTO.Manufacturer;
                        productStoreMappingDetail.Category = updateProductDTO.Category;
                        productStoreMappingDetail.PermaLink = updateProductDTO.Name.ToLower().Replace(" ", "-");
                        productStoreMappingDetail.UpdatedOnUtc = DateTime.UtcNow;
                        productStoreMappingDetail.FlagSampleProducts = false;
                        _context.ProductStoreMapping.Update(productStoreMappingDetail);
                        _context.SaveChanges();
                    }
                    result.ProductId = product.ProductId;
                    result.NewProduct = product;
                    return result;
                }
                else
                {
                    var productStoreMappingDetail = _context.ProductStoreMapping.Where(x => x.ProductId == updateProductDTO.ProductId && x.BranchId == BranchId).FirstOrDefault();
                    if (productStoreMappingDetail != null)
                    {
                        productStoreMappingDetail.Name = updateProductDTO.Name;
                        productStoreMappingDetail.FullDescription = updateProductDTO.FullDescription;
                        productStoreMappingDetail.Manufacturer = updateProductDTO.Manufacturer;
                        productStoreMappingDetail.Category = updateProductDTO.Category;
                        productStoreMappingDetail.PermaLink = updateProductDTO.Name.ToLower().Replace(" ", "-");
                        productStoreMappingDetail.UpdatedOnUtc = DateTime.UtcNow;
                        productStoreMappingDetail.FlagSampleProducts = false;
                        _context.ProductStoreMapping.Update(productStoreMappingDetail);
                        _context.SaveChanges();
                        result.ProductId = productStoreMappingDetail.ProductId;
                        return result;
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        public ProductDetailModelWithVariant GetProductDetailsForVariant(int productId, int BranchId)
        {
            ProductDetailModelWithVariant productDetailModel = new ProductDetailModelWithVariant();

            var product = _context.Product.Where(x => x.IsDeleted != true && x.Published == true && x.ProductId == productId).Include(y => y.ManufacturerDetails).FirstOrDefault();

            if (product != null)
            {
                _mapper.Map<Product, ProductDetailModelWithVariant>(product, productDetailModel);
                productDetailModel.BrandName = product.ManufacturerDetails != null ? product.ManufacturerDetails.Name : String.Empty;
            }
            if (BranchId > 0)
            {
                var productMapping = _context.ProductStoreMapping.Where(x => x.IsDeleted != true && x.Published == true && x.ProductId == productId && x.BranchId == BranchId).Include(y => y.ManufacturerDetails).FirstOrDefault();
                if (productMapping != null)
                {
                    _mapper.Map<ProductStoreMapping, ProductDetailModelWithVariant>(productMapping, productDetailModel);
                    productDetailModel.BrandName = productMapping.ManufacturerDetails != null ? productMapping.ManufacturerDetails.Name : String.Empty;
                }
            }


            return productDetailModel;
        }



        // For single db Hyperlocal
        public List<ProductModel> GetFeaturedProducts_(int BranchId)
        {
            var products = _context.ProductStoreMapping.Join(_context.Pricing,
            Product => new { id1 = Product.ProductId, id2 = Product.BranchId },
            Pricing => new { id1 = Pricing.Product, id2 = Pricing.Branch },
            (Product, Pricing) => new { Product, Pricing }).Where(x => x.Product.ShowOnHomePage == true && x.Product.Published == true
            && x.Product.IsDeleted != true && x.Product.BranchId == BranchId).Select(y => new ProductModel
            {
                ProductId = y.Product.ProductId,
                Name = y.Product.Name,
                PictureName = _context.ProductImage.Where(x => x.ProductId == y.Product.ProductId).Select(x => x.PictureName).FirstOrDefault(),
                Price = y.Pricing.Price,
                SpecialPrice = y.Pricing.SpecialPrice,
                FullDescription = y.Product.FullDescription,
                Category = y.Product.Category,
                PermaLink = y.Product.PermaLink,
                StoresCount = y.Product.PricingCollection.ToList().Count(),
                BranchId = y.Product.BranchId,
            }).ToList();
            return products;
        }

        public string GetSearchCataloguePricingFilterQuery(string productFilter, int[] BranchId)
        {
            var query = @"
                     Select DISTINCT ProductId
                               From ProductStoreMapping as Product
                                            Inner Join Category ON Category.CategoryId = Product.Category                                             
                                            LEFT JOIN Manufacturer ON Product.Manufacturer = Manufacturer.ManufacturerId
                                             WHERE ( NOT ((1 = [Product].[IsDeleted]) AND ([Product].[IsDeleted] IS NOT NULL)))                                                                                                                                             
		                                                                                  AND (1 = [Product].[Published]) AND
                                                                                          [Product].[FlagSampleProducts] = 0 AND
                                                                            ([Product].Name LIKE '{productNameFilter}'
                                                                                OR Manufacturer.Name LIKE '{productNameFilter}')  {branchId}"
                .FormatWith(new
                {
                    branchId = (BranchId.Length > 0) ? "AND [Product].BranchId  In ( " + string.Join(",", BranchId) + " )" : "",
                    productNameFilter = "%" + productFilter + "%"
                });
            return query;
        }

        //for Hyperlocal
        public List<ProductModelWithCategory> SearchCatalogue(string productFilter, List<int> branchIdList)
        {
            var products = _context.Product.Where(x => x.IsDeleted != true && x.Published == true &&
                         (x.Name.Contains(productFilter) || x.MetaKeywords.Contains(productFilter) || x.MetaTitle.Contains(productFilter)
               || x.ManufacturerDetails.Name.Contains(productFilter))).Include(y => y.ManufacturerDetails).Include(z => z.PricingCollection).Include(cat => cat.CategoryDetails).ToList<Product>();

            products.ForEach(x => x.PricingCollection = x.PricingCollection.Where(y => branchIdList.Contains(y.Branch) && y.IsDeleted != true).ToList());

            List<ProductModelWithCategory> productModelList = new List<ProductModelWithCategory>();
            _mapper.Map<List<Product>, List<ProductModelWithCategory>>(products, productModelList);
            return productModelList;
        }

        //filter
        public string GetSearchFilterForVbuy(SelectedFilter filter, string productFilter, List<int> branchIdList, int? priceRangeFrom, int? priceRangeTo, int? pageStart, int? pageSize)
        {
            var query = @"WITH ProductCTE AS 
                    (
                     Select ParentCategoryId, CategoryId, Category.Name AS SubCategoryName, ProductId, Product.Name as Name, productimage.PictureName, Product.PermaLink, Max(Price) AS Price, AdditionalShippingCharge, SellerBranch.BranchId, SellerBranch.BranchName
                               From ProductStoreMapping as Product
                                            Inner Join Category ON Category.CategoryId = Product.Category
                                            LEFT Join Pricing ON Pricing.Product = Product.ProductId
                                            LEFT JOIN Manufacturer ON Product.Manufacturer = Manufacturer.ManufacturerId
                                            LEFT JOIN SellerBranch ON Pricing.Branch = SellerBranch.BranchId
                                             OUTER apply
                                                        (
                                                        select top 1 PictureName from ProductImage where ProductId = Product.ProductId
                                                        )
                                                        productimage
                                             WHERE ( NOT ((1 = [Product].[IsDeleted]) AND ([Product].[IsDeleted] IS NOT NULL))) 
                                                                                         
                                                                                          {PricingFilter}     
                                                                                          {BrandFilter}                                                          
		                                                                                  AND (1 = [Product].[Published]) AND
                                                                            ([Product].Name LIKE '{productNameFilter}'
                                                                                OR Manufacturer.Name LIKE '{productNameFilter}') AND [Product].FlagSampleProducts = 0														 
                    GROUP BY ParentCategoryId, CategoryId,  Category.Name , ProductId, Product.Name,productimage.PictureName,Product.PermaLink, Pricing.AdditionalShippingCharge, SellerBranch.BranchId, SellerBranch.BranchName
                    )
                    ,
                    BranchCTE AS
                    (
	                    SELECT DISTINCT Branch, Pricing.Product Product, Count(Branch) StoresCount, Min(SpecialPrice) SpecialPrice 
                        FROM Pricing 
	                    Inner Join ProductCTE ON Pricing.Product = ProductCTE.ProductId
                        
	                    WHERE Branch IN ({branchIdListFilter})
                        {SpecialPricingFilter}
                        AND [Pricing].[IsDeleted] IS NULL OR [Pricing].[IsDeleted] =0
	                    Group BY Pricing.Product,Pricing.Branch
                    )
                , Results AS 
					(
                    Select ParentCategoryId, CategoryId AS SubCategoryId, SubCategoryName, ProductId, Name,ProductCTE.PictureName,ProductCTE.PermaLink, Price, ISNULL(StoresCount,0) StoresCount
                    , ISNULL(SpecialPrice,0) SpecialPrice, AdditionalShippingCharge, ProductCTE.BranchId, ProductCTE.BranchName
                    FROM ProductCTE
                    LEFT JOIN BranchCTE ON ProductCTE.ProductId = BranchCTE.Product
                    WHERE Branch IN ({branchIdListFilter})
                    {PriceLowToHighFilterClause} 
                    )                    
                    {GetRowNumberQuery}
                    "
                .FormatWith(new
                {
                    productNameFilter = "%" + productFilter + "%",
                    branchIdListFilter = branchIdList.Count > 0 ? string.Join(",", branchIdList) : "0",
                    SpecialPricingFilter = (priceRangeFrom != null && priceRangeTo != null) ? " AND [Pricing].[Price] >= {priceRangeFrom} AND [Pricing].[Price] <= {priceRangeTo}".FormatWith(new { priceRangeFrom, priceRangeTo }) : "",
                    PricingFilter = (priceRangeFrom != null && priceRangeTo != null) ? " AND [Pricing].[SpecialPrice] >= {priceRangeFrom} AND [Pricing].[SpecialPrice] <= {priceRangeTo}".FormatWith(new { priceRangeFrom, priceRangeTo }) : "",
                    BrandFilter = (filter != null && filter.SelectedBrandList != null && filter.SelectedBrandList.Count > 0) ? " AND [Product].[Manufacturer] IN ( " +
                     string.Join(",", filter.SelectedBrandIdList) + " )" : "",
                    GetRowNumberQuery = _productHelper.GetRowNumberQuery(GetSortByFieldBasedOnEnumForProductQuery(filter), pageStart, pageSize),
                    PriceLowToHighFilterClause = (filter != null && filter.SortById > 0 && filter.SortById < 4) ? " And SpecialPrice > 0 " : ""
                });
            return query;
        }
    }
}
