using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data.SqlClient;
using System.Security.Claims;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.Order;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class ProductsController : VSControllerBase
    {
        private readonly EfContext _efContext;
        private readonly DataContext _context;
        private readonly UserService _userService;
        private readonly SellerBranchRepository _sellerBranchRepository;
        private readonly ProductRepository _productRepository;
        private readonly ProductFilterRepository _productFilterRepository;
        private readonly ProductFeaturesHelper _productFeaturesHelper;
        private readonly IMapper _mapper;
        private readonly RatingHelper _ratingHelper;
        private readonly CartRepository _cartRepository;
        private readonly SellerContactHelper _sellerContactHelper;
        private readonly MailHelper _mailHelper;
        private readonly MessageHelper _messageHelper;
        public ProductsController(SellerContactHelper sellerContactHelper, CartRepository cartRepository, RatingHelper ratingHelper, IMapper mapper, ProductFeaturesHelper productFeaturesHelper, ProductFilterRepository productFilterRepository, ProductRepository productRepository, SellerBranchRepository sellerBranchRepository, UserService userService, DataContext dataContext, EfContext efContext, IOptions<AppSettings> _appSettings, MailHelper mailHelper, MessageHelper messageHelper) : base(_appSettings)
        {
            _efContext = efContext;
            _context = dataContext;
            _userService = userService;
            _sellerBranchRepository = sellerBranchRepository;
            _productRepository = productRepository;
            _productFilterRepository = productFilterRepository;
            _productFeaturesHelper = productFeaturesHelper;
            _mapper = mapper;
            _ratingHelper = ratingHelper;
            _cartRepository = cartRepository;
            _sellerContactHelper = sellerContactHelper;
            _mailHelper = mailHelper;
            _messageHelper = messageHelper;
        }

        [HttpGet("Seller/{StoreId}/GetTopOffers/{limit}")]
        public List<ProductModelWithCategory> GetTopOffers(int StoreId, int limit)
        {
            return GetTopOffers(StoreId, null, null, null, limit);
        }

        [HttpGet("GetTopOffers/{lat}/{lng}/{radius}/{limit}")]
        [HttpGet("Seller/{StoreId}/GetTopOffers/{lat}/{lng}/{radius}/{limit}")]
        public List<ProductModelWithCategory> GetTopOffers(int StoreId, decimal? lat, decimal? lng, int? radius, int limit)
        {
            try
            {
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(StoreId, lat, lng, radius);
                var branchIdListLocation = _efContext.Database.SqlQuery<LocationBoundaryResult>(query)
                    .Distinct()
                    .Select(x => x.BranchId)
                    .ToList<int>();
                var partnerStoreList = _context.SellerBranch.Where(x => x.FlagPartner == true).Select(x => x.BranchId).ToList<int>();

                foreach (var partnerStore in partnerStoreList)
                {
                    branchIdListLocation.Add(partnerStore);
                }

                var curUserId = 0;

                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    curUserId = Convert.ToInt32(currentUserId);
                }

                var offersquery = _productRepository.GetOffersQuery(branchIdListLocation, limit, curUserId);
                var offerList = _efContext.Database.SqlQuery<ProductModelWithCategory>(offersquery).ToList();
                foreach (var eachData in offerList)
                {
                    if (eachData.PictureName != null)
                    {
                        if (!eachData.PictureName.Contains("http"))
                        {
                            eachData.PictureName = _appSettings.ImageUrlBase + eachData.PictureName;
                        }
                    }
                    eachData.ProductRating = _ratingHelper.CalulateProductRating(eachData.ProductId);
                    eachData.ProductRatingCount = _context.ProductRating.Where(x => x.ProductId == eachData.ProductId).Count();
                }
                return offerList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("GetMinMaxForProductCategory/{id}")]
        [HttpGet("Seller/{BranchId}/GetMinMaxForProductCategory/{id}")]
        public BaseProductFilterResult GetMinMaxForProductCategory(int BranchId, int id)
        {
            try
            {
                var catIdList = _context.Category.Where(x => x.ParentCategoryId == id || x.CategoryId == id).Select(x => x.CategoryId).ToList<int>();

                var prodList = new List<int>();
                prodList = _context.Product.Where(x => catIdList.Contains(x.Category)).Select(x => x.ProductId).ToList<int>();
                if (BranchId > 0)
                {
                    prodList = _context.ProductStoreMapping.Where(x => catIdList.Contains(x.Category)).Select(x => x.ProductId).ToList<int>();
                }

                BaseProductFilterResult filter = new BaseProductFilterResult();

                filter.Max = _context.Pricing.Where(x => prodList.Contains(x.Product)).Max(x => x.Price);
                filter.Min = _context.Pricing.Where(x => prodList.Contains(x.Product)).Min(x => x.SpecialPrice);
                filter.Brand = _productRepository.GetBrandFilter(prodList);

                return filter;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("GetMinMaxForProductSearch/{lat}/{lng}/{mapRadius}/{search}")]
        [HttpGet("Seller/{BranchId}/GetMinMaxForProductSearch/{search}")]
        public BaseProductFilterResult GetMinMaxForProductSearch(int BranchId, string search, decimal? lat, decimal? lng, int? mapRadius)
        {
            try
            {
                var branchIds = new List<int>();

                if (lat > 0 && lng > 0 && mapRadius > 0)
                {
                    var storeQuery = _sellerBranchRepository.GetStoresWithinAreaQuery(0, lat, lng, mapRadius);
                    branchIds = _efContext.Database.SqlQuery<LocationBoundaryResult>(storeQuery).Distinct().Select(x => x.BranchId).ToList<int>();
                }
                if (BranchId > 0)
                {
                    branchIds.Add(BranchId);
                }
                var query = _productRepository.GetSearchCataloguePricingFilterQuery(search, branchIds.ToArray());
                var prodList = _efContext.Database.SqlQuery<int>(query).ToList();

                BaseProductFilterResult filter = new BaseProductFilterResult();
                filter.Max = _context.Pricing.Where(x => prodList.Contains(x.Product)).Max(x => x.Price);
                filter.Min = _context.Pricing.Where(x => prodList.Contains(x.Product)).Min(x => x.SpecialPrice);
                filter.Brand = _productRepository.GetBrandFilter(prodList);

                return filter;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("GetFiltersForProductCategory/{id}")]
        [HttpGet("Seller/{BranchId}/GetFiltersForProductCategory/{id}")]
        public List<ProductFilter> GetFiltersForProductCategory(int BranchId, int id)
        {
            try
            {
                var catIdList = _context.Category.Where(x => x.ParentCategoryId == id || x.CategoryId == id)
                .Select(x => x.CategoryId)
                .ToList<int>();
                var query = _productFilterRepository.GetProductFilterQuery(catIdList.ToArray());
                var filterList = _efContext.Database.SqlQuery<ProductFilter>(query).ToList<ProductFilter>();

                return filterList;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [HttpPost("GetProductsWithDescription")]
        [HttpPost("Seller/{StoreId}/GetProductsWithDescription")]
        public List<ProductModelWithDescription> GetProductsWithDescription(int StoreId, ProductParameterFilterDTO productParameterFilterSet)
        {
            try
            {
                var catIdList = _context.Category.Where(x => x.ParentCategoryId == productParameterFilterSet.id || x.CategoryId == productParameterFilterSet.id)
                    .Select(x => x.CategoryId)
                    .ToList<int>();

                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(StoreId, productParameterFilterSet.lat, productParameterFilterSet.lng, productParameterFilterSet.mapRadius);
                var branchIdListLocation = _efContext.Database.SqlQuery<LocationBoundaryResult>(query)
                    .Distinct()
                    .Select(x => x.BranchId)
                    .ToList<int>();
                var partnerStoreList = _context.SellerBranch.Where(x => x.FlagPartner == true).Select(x => x.BranchId).ToList<int>();

                foreach (var partnerStore in partnerStoreList)
                {
                    branchIdListLocation.Add(partnerStore);
                }
                var curUserId = 0;
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    curUserId = Convert.ToInt32(currentUserId);
                }
                bool Isvbuy = false;
                var orderOrigin = Request.Headers.Origin;
                if (!string.IsNullOrEmpty(orderOrigin))
                {
                    string[] hostList = _appSettings.VbuyHostList.Split(",");

                    foreach (string host in hostList)
                    {
                        if (host.Equals(orderOrigin))
                        {
                            Isvbuy = true;
                        }
                    }
                }
                if (catIdList.Count > 0)
                {
                    var productquery = _productRepository.GetProductsQuery_WithDescription(productParameterFilterSet.filter, productParameterFilterSet.selectedProductFilter,
                        catIdList.ToArray(), branchIdListLocation, productParameterFilterSet.priceRangeFrom, productParameterFilterSet.PriceRangeTo,
                        productParameterFilterSet.pageStart,
                        productParameterFilterSet.pageSize, curUserId, Isvbuy);
                    var productDetails = _efContext.Database.SqlQuery<ProductModelWithDescription>(productquery).ToList();
                    foreach (var eachDetail in productDetails)
                    {
                        if (eachDetail.PictureName != null)
                        {
                            if (!eachDetail.PictureName.Contains("http"))
                            {
                                eachDetail.PictureName = _appSettings.ImageUrlBase + eachDetail.PictureName;
                            }
                        }
                        eachDetail.ProductRating = _ratingHelper.CalulateProductRating(eachDetail.ProductId);
                        eachDetail.ProductRatingCount = _context.ProductRating.Where(x => x.ProductId == eachDetail.ProductId).Count();
                    }
                    return productDetails;
                }
                return new List<ProductModelWithDescription>();
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        [HttpGet("GetProductKeyFeatures/{id}")]
        [HttpGet("Seller/{BranchId}/GetProductKeyFeatures/{id}")]
        public List<ProductKeyFeaturesResult> GetProductKeyFeatures(int BranchId, int id)
        {
            try
            {
                var keyFeatureDetails = _context.ProductKeyFeatures.Where(x => x.ProductId == id).OrderBy(x => x.DisplayOrder).ToList();
                var productKeyFeaturesList = _mapper.Map<List<ProductKeyFeatures>, List<ProductKeyFeaturesResult>>(keyFeatureDetails);
                return productKeyFeaturesList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("GetProductSpecification/{id}")]
        [HttpGet("Seller/{BranchId}/GetProductSpecification/{id}")]
        public List<ProductSpecificationResult> GetProductSpecification(int BranchId, int id)
        {
            try
            {
                var productSpecification = _context.ProductSpecification.Where(x => x.ProductId == id).ToList();
                var productSpecificationList = _mapper.Map<List<ProductSpecification>, List<ProductSpecificationResult>>(productSpecification);
                return productSpecificationList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("Seller/{StoreId}/GetProductDetails/{id}/{flagLocation}")]
        public ProductDetailModel GetProductDetails(int StoreId, int id, bool flagLocation)
        {
            return GetProductDetails(StoreId, id, flagLocation, null, null, null);
        }

        [HttpGet("Seller/{StoreId}/GetProductDetails/{id}/{flagLocation}/{lat}/{lng}/{mapRadius}")]
        public ProductDetailModel GetProductDetails(int StoreId, int id, bool flagLocation, decimal? lat, decimal? lng, int? mapRadius)
        {
            try
            {
                if (!flagLocation)
                {
                    return GetProductDetails(id, flagLocation, null);
                }
                var branchIdListLocation = GetBranchIdListBasedOnLocation(StoreId, lat, lng, mapRadius);
                var productDetailModel = GetProductDetails(id, flagLocation, branchIdListLocation);
                productDetailModel.RelatedProductList = GetRelatedProducts(id, StoreId, branchIdListLocation);
                foreach (var product in productDetailModel.RelatedProductList)
                {
                    var productImages = _context.ProductImage.Where(x => x.ProductId == product.ProductId).FirstOrDefault();
                    if (productImages != null)
                    {
                        if (!productImages.PictureName.Contains("http"))
                        {
                            product.PictureName = _appSettings.ImageUrlBase + productImages.PictureName;
                        }
                        else
                        {
                            product.PictureName = productImages.PictureName;
                        }
                    }
                    var pricing = _context.Pricing.Where(x => x.Product == product.ProductId).FirstOrDefault();
                    if (StoreId > 0)
                    {
                        pricing = _context.Pricing.Where(x => x.Product == product.ProductId && x.Branch == StoreId).FirstOrDefault();
                    }
                    if (pricing != null)
                    {
                        var branchDetails = _context.SellerBranch.Where(x => x.BranchId == pricing.Branch).FirstOrDefault();
                        product.AdditionalShippingCharge = pricing.AdditionalShippingCharge;
                        product.DeliveryTime = pricing.DeliveryTime;
                        if (branchDetails != null)
                        {
                            product.BranchName = branchDetails.BranchName;
                        }
                    }
                }
                return productDetailModel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private ProductDetailModel GetProductDetails(int id, bool flagLocation, List<int> branchIdList)
        {
            try
            {
                var productDetailModel = _productRepository.GetProductDetails(id);
                var inventoryQuantityExceed = _cartRepository.CheckQuantityExists(id);
                var availableQuantity = _cartRepository.getAvailableQuantity(id);

                productDetailModel.StorePricingModel = new List<StorePricingModel>();
                productDetailModel.ProductImages = new List<string>();
                //Images
                if (productDetailModel != null && productDetailModel.ProductId > 0)
                {
                    var productImages = _context.ProductImage.Where(x => x.ProductId == id).ToList();
                    foreach (var images in productImages)
                    {
                        if (images.PictureName != null)
                        {
                            if (!images.PictureName.Contains("http"))
                            {
                                productDetailModel.ProductImages.Add(_appSettings.ImageUrlBase + images.PictureName);
                            }
                            else
                            {
                                productDetailModel.ProductImages.Add(images.PictureName);
                            }
                        }
                    }
                }
                //include the lat and longitude and also location. 
                if (productDetailModel != null && productDetailModel.ProductId > 0)
                {
                    var pricingDetails = _context.Pricing.Where(x => x.Product == id && x.IsDeleted != true && (flagLocation ? branchIdList.Contains(x.Branch) : x.Branch != null))
                        .Include(y => y.BranchDetails).Include(z => z.BranchDetails.SellerMap)
                        .OrderBy(z => z.SpecialPrice).OrderByDescending(y => y.UpdatedOnUtc).ToList();

                    foreach (var pricing in pricingDetails)
                    {
                        var storePricingModel = new StorePricingModel()
                        {
                            BranchId = pricing.Branch,
                            BranchName = pricing.BranchDetails.BranchName,
                            StoreId = pricing.BranchDetails.SellerMap.StoreId,
                            StoreName = pricing.BranchDetails.SellerMap.StoreName,
                            BranchAddress1 = pricing.BranchDetails.Address1,
                            BranchAddress2 = pricing.BranchDetails.Address2,
                            BranchCity = pricing.BranchDetails.City,
                            Latitude = pricing.BranchDetails.Latitude,
                            Longitude = pricing.BranchDetails.Longitude,
                            EnableBuy = pricing.BranchDetails.EnableBuy,
                            Price = pricing.Price,
                            SpecialPrice = pricing.SpecialPrice,
                            BranchRating = GetSellerRating(pricing.Branch),
                            AdditionalShippingCharge = pricing.AdditionalShippingCharge,
                            AdditionalTax = pricing.AdditionalTax,
                            DeliveryTime = pricing.DeliveryTime,
                            SpecialPriceDescription = pricing.SpecialPriceDescription,
                            FlagQuantityExceeded = inventoryQuantityExceed,
                            AvailableQuantity = availableQuantity
                        };
                        productDetailModel.StorePricingModel.Add(storePricingModel);
                    }
                    //Category details
                    if (productDetailModel.Category > 0)
                    {
                        var category = _context.Category.Where(x => x.CategoryId == productDetailModel.Category).FirstOrDefault();
                        productDetailModel.CategoryName = category.Name;
                        productDetailModel.ParentCategoryId = category.ParentCategoryId;
                        productDetailModel.CategoryGroupTag = category.CategoryGroupTag;

                        var parentCategory = _context.Category.Where(x => x.CategoryId == category.ParentCategoryId).FirstOrDefault();
                        productDetailModel.ParentCategoryName = parentCategory.Name;
                    }
                }
                productDetailModel.AndroidInformation1 = @"Availability: Our Service is available only in Chennai, and certain cities of Tamilnadu. If stock is not available in the preferred store, VBuy will get you the same product from another store with same price. 
                * Some registered sellers choose not to avail our map service and will also be shown outside the selected map area.";
                return productDetailModel;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private List<int> GetBranchIdListBasedOnLocation(int StoreId, decimal? lat, decimal? lng, int? radius)
        {
            try
            {
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(StoreId, lat, lng, radius);
                var branchIdListLocation = _efContext.Database.SqlQuery<LocationBoundaryResult>(query)
                    .Distinct()
                    .Select(x => x.BranchId)
                    .ToList<int>();

                var partnerStoreList = _context.SellerBranch.Where(x => x.FlagPartner == true).Select(x => x.BranchId).ToList<int>();

                foreach (var partnerStore in partnerStoreList)
                {
                    branchIdListLocation.Add(partnerStore);
                }
                return branchIdListLocation;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private decimal? GetSellerRating(int branchId)
        {
            try
            {
                var branchRatingResultSet = _ratingHelper.GetSellerRating(branchId);
                if (branchRatingResultSet.Count > 0)
                {
                    return CalculateStarRating(branchRatingResultSet);
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private decimal CalculateStarRating(List<BranchRatingResult> ratingList)
        {
            try
            {
                decimal rating = 0;
                decimal totalCount = 0;
                for (var i = 0; i < ratingList.Count; i++)
                {
                    rating = rating + (ratingList[i].Rating * ratingList[i].RatingCount);
                    totalCount = totalCount + ratingList[i].RatingCount;
                }
                var starRatingValue = rating / totalCount;
                return Math.Round(starRatingValue, 1);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        [HttpGet("GetRelatedProducts/{id}")]
        public List<ProductModelWithCategory> GetRelatedProducts(int id, int StoreId, List<int> branchIdList)
        {
            try
            {
                var curProduct = _context.ProductStoreMapping.Join(_context.Pricing, Product => new { id1 = Product.ProductId, id2 = Product.BranchId }, Pricing => new { id1 = Pricing.Product, id2 = Pricing.Branch }, (Product, Pricing) => new { Product, Pricing }).
                Where(x => x.Product.ProductId == id && x.Product.IsDeleted != true && x.Product.Published == true).FirstOrDefault();
                if (StoreId > 0)
                {
                    curProduct = _context.ProductStoreMapping.Join(_context.Pricing, Product => new { id1 = Product.ProductId, id2 = Product.BranchId }, Pricing => new { id1 = Pricing.Product, id2 = Pricing.Branch }, (Product, Pricing) => new { Product, Pricing }).
                    Where(x => x.Product.ProductId == id && x.Product.BranchId == StoreId && x.Product.IsDeleted != true && x.Product.Published == true).FirstOrDefault();
                }

                int Min = curProduct.Product.ProductId - 5;
                int Max = curProduct.Product.ProductId + 5;
                Random randNum = new Random();
                int[] randomProductIds = Enumerable.Repeat(0, 8).Select(i => randNum.Next(Min, Max)).ToArray();
                var productPrice = curProduct.Pricing;

                var category = _context.Category.Where(x => x.CategoryId == curProduct.Product.Category).FirstOrDefault();
                if (StoreId > 0)
                {
                    category = _context.Category.Where(x => x.CategoryId == curProduct.Product.Category && x.BranchId == StoreId).FirstOrDefault();
                }

                var catidList = _context.Category.Where(x => x.CategoryGroupTag == category.CategoryGroupTag).ToList().Select(x => x.CategoryId);

                var adjustedProductPrice = productPrice != null ? productPrice.SpecialPrice : 1000;

                var relatedPricing2ProductIds = _context.Pricing.Where(x => x.IsDeleted != true && x.SpecialPrice > (adjustedProductPrice - 1000) && x.SpecialPrice < (adjustedProductPrice + 1000)).Select(x => x.Product);


                var relatedProduct1List = _context.ProductStoreMapping.Join(_context.Pricing,
                        x => new { id1 = x.ProductId, id2 = x.BranchId },
                        y => new { id1 = y.Product, id2 = y.Branch },
                        (x, y) => new { x, y }).Where(x => x.x.Category == curProduct.Product.Category && (randomProductIds.Contains(x.x.ProductId)) && x.x.IsDeleted != true && x.x.ProductId != curProduct.Product.ProductId && x.x.FlagSampleProducts == false && branchIdList.Contains(x.x.BranchId))
                    .Select(z => new ProductModelWithCategory
                    {
                        ProductId = z.x.ProductId,
                        Name = z.x.Name,
                        Price = z.y.Price,
                        SpecialPrice = z.y.SpecialPrice,
                        PermaLink = z.x.PermaLink,
                    }).Distinct().ToList();
                if (StoreId > 0)
                {
                    relatedProduct1List = _context.ProductStoreMapping.Join(_context.Pricing,
                         x => new { id1 = x.ProductId, id2 = x.BranchId },
                         y => new { id1 = y.Product, id2 = y.Branch },
                         (x, y) => new { x, y }).Where(x => x.x.Category == curProduct.Product.Category && (randomProductIds.Contains(x.x.ProductId)) && x.x.IsDeleted != true && x.x.ProductId != curProduct.Product.ProductId && x.x.BranchId == StoreId && x.x.FlagSampleProducts == false)
                     .Select(z => new ProductModelWithCategory
                     {
                         ProductId = z.x.ProductId,
                         Name = z.x.Name,
                         Price = z.y.Price,
                         SpecialPrice = z.y.SpecialPrice,
                         PermaLink = z.x.PermaLink,
                     }).Distinct().ToList();
                }

                var relatedProduct2List = _context.ProductStoreMapping.Join(_context.Pricing,
                            x => new { id1 = x.ProductId, id2 = x.BranchId },
                            y => new { id1 = y.Product, id2 = y.Branch },
                            (x, y) => new { x, y }).Where(x => relatedPricing2ProductIds.Contains(x.x.ProductId) && catidList.Contains(x.x.Category) && x.x.IsDeleted != true && x.x.ProductId != curProduct.Product.ProductId && x.x.FlagSampleProducts == false && branchIdList.Contains(x.x.BranchId))
                        .Select(z => new ProductModelWithCategory
                        {
                            ProductId = z.x.ProductId,
                            Name = z.x.Name,
                            Price = z.y.Price,
                            SpecialPrice = z.y.SpecialPrice,
                            PermaLink = z.x.PermaLink,
                        }).Distinct().Take(10).ToList();
                if (StoreId > 0)
                {
                    relatedProduct2List = _context.ProductStoreMapping.Join(_context.Pricing,
                            x => new { id1 = x.ProductId, id2 = x.BranchId },
                            y => new { id1 = y.Product, id2 = y.Branch },
                            (x, y) => new { x, y }).Where(x => relatedPricing2ProductIds.Contains(x.x.ProductId) && catidList.Contains(x.x.Category) && x.x.IsDeleted != true && x.x.ProductId != curProduct.Product.ProductId && x.x.BranchId == StoreId && x.x.FlagSampleProducts == false)
                        .Select(z => new ProductModelWithCategory
                        {
                            ProductId = z.x.ProductId,
                            Name = z.x.Name,
                            Price = z.y.Price,
                            SpecialPrice = z.y.SpecialPrice,
                            PermaLink = z.x.PermaLink,
                        }).Distinct().Take(10).ToList();
                }
                var relatedProduct3List = _context.ProductStoreMapping.Join(_context.Pricing,
                            x => new { id1 = x.ProductId, id2 = x.BranchId },
                            y => new { id1 = y.Product, id2 = y.Branch },
                            (x, y) => new { x, y }).Where(x => x.x.Manufacturer == curProduct.Product.Manufacturer && (x.x.ShowOnHomePage == true) && x.x.IsDeleted != true && x.x.ProductId != curProduct.Product.ProductId && x.x.FlagSampleProducts == false && branchIdList.Contains(x.x.BranchId))
                        .Select(z => new ProductModelWithCategory
                        {
                            ProductId = z.x.ProductId,
                            Name = z.x.Name,
                            Price = z.y.Price,
                            SpecialPrice = z.y.SpecialPrice,
                            PermaLink = z.x.PermaLink,
                        }).Distinct().ToList();
                if (StoreId > 0)
                {
                    relatedProduct3List = _context.ProductStoreMapping.Join(_context.Pricing,
                            x => new { id1 = x.ProductId, id2 = x.BranchId },
                            y => new { id1 = y.Product, id2 = y.Branch },
                            (x, y) => new { x, y }).Where(x => x.x.Manufacturer == curProduct.Product.Manufacturer && (x.x.ShowOnHomePage == true) && x.x.IsDeleted != true && x.x.ProductId != curProduct.Product.ProductId && x.x.BranchId == StoreId && x.x.FlagSampleProducts == false)
                        .Select(z => new ProductModelWithCategory
                        {
                            ProductId = z.x.ProductId,
                            Name = z.x.Name,
                            Price = z.y.Price,
                            SpecialPrice = z.y.SpecialPrice,
                            PermaLink = z.x.PermaLink,
                        }).Distinct().ToList();
                }

                List<ProductModelWithCategory> productlistFinal = relatedProduct1List.Concat(relatedProduct2List).Concat(relatedProduct3List).Distinct().Take(10).ToList();

                var distinctList = productlistFinal.DistinctBy(x => x.ProductId).ToList();

                return distinctList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("GetProductStoreLocations/{id}/{lat}/{lng}/{mapRadius}")]
        [HttpGet("Seller/{StoreId}/GetProductStoreLocations/{id}/{lat}/{lng}/{mapRadius}")]
        public List<RetailerLocationMapResult> GetProductStoreLocations(int StoreId, int id, decimal lat, decimal lng, int mapRadius)
        {
            try
            {
                var branchIdListLocation = GetBranchIdListBasedOnLocation(StoreId, lat, lng, mapRadius);

                var branchIdList = _context.Pricing.Where(x => x.Product == id && branchIdListLocation.Contains(x.Branch))
                    .Select(x => x.Branch)
                    .Distinct()
                    .ToList<int>();

                return _sellerBranchRepository.GetStoreLocations(branchIdList);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [HttpGet("GetSearchProductsFilter/{lat}/{lng}/{mapRadius}/{searchString}")]
        public List<ProductModel> GetSearchProductsFilter(decimal lat, decimal lng, int mapRadius, string? searchString)
        {
            try
            {
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(0, lat, lng, mapRadius);
                var branchIdList = _efContext.Database.SqlQuery<LocationBoundaryResult>(query).Distinct().Select(x => x.BranchId).ToList<int>();

                if (!string.IsNullOrEmpty(searchString))
                {
                    if (_appSettings.enableElasticSearch != null && _appSettings.enableElasticSearch.ToString().ToLower() == "true")
                    {
                        return _productRepository.GetSearchProductsFilter(searchString, true, branchIdList);
                    }
                    return _productRepository.GetSearchProductsFilter(searchString, false, branchIdList);
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        // single db
        [HttpGet("Seller/{BranchId}/GetSearchProductsFilter/{searchString}")]
        public List<ProductModel> GetSearchProductsFilter(int BranchId, string searchString)
        {
            try
            {
                if (_appSettings.enableElasticSearch != null && _appSettings.enableElasticSearch.ToString().ToLower() == "true")
                {
                    return _productRepository.GetSearchProductsFilter(BranchId, searchString, true);
                }
                return _productRepository.GetSearchProductsFilter(BranchId, searchString, false);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpPost("SearchCatalogueWithBuy")]
        [HttpPost("Seller/{StoreId}/SearchCatalogueWithBuy")]
        public List<SearchProductModelWithBuy> SearchCatalogueWithBuy(int StoreId, ProductSearchFilterDTO productSearchFilterSet)
        {
            try
            {
                var branchIdListLocation = GetBranchIdListBasedOnLocation(StoreId, productSearchFilterSet.lat, productSearchFilterSet.lng, productSearchFilterSet.mapRadius);
                var searchProductsquery = _productRepository.GetSearchCatalogueQueryWithBuy(productSearchFilterSet.filter,
                    productSearchFilterSet.productFilter, branchIdListLocation, productSearchFilterSet.priceRangeFrom,
                    productSearchFilterSet.PriceRangeTo, productSearchFilterSet.pageStart, productSearchFilterSet.pageSize);
                var productList = _efContext.Database.SqlQuery<SearchProductModelWithBuy>(searchProductsquery).ToList();
                foreach (var eachData in productList)
                {
                    if (eachData.PictureName != null)
                    {
                        if (!eachData.PictureName.Contains("http"))
                        {
                            eachData.PictureName = _appSettings.ImageUrlBase + eachData.PictureName;
                        }
                    }
                    eachData.ProductRating = _ratingHelper.CalulateProductRating(eachData.ProductId);
                    eachData.ProductRatingCount = _context.ProductRating.Where(x => x.ProductId == eachData.ProductId).Count();
                }
                return productList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("Seller/{BranchId}/GetAllOffers")]
        public List<ProductModelWithCategory> GetAllOffers(int BranchId)
        {
            return GetAllOffers(BranchId, null, null, null);
        }
        [HttpGet("GetAllOffers/{lat}/{lng}/{radius}")]
        [HttpGet("Seller/{BranchId}/GetAllOffers/{lat}/{lng}/{radius}")]
        public List<ProductModelWithCategory> GetAllOffers(int BranchId, decimal? lat, decimal? lng, int? radius)
        {
            try
            {
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(BranchId, lat, lng, radius);
                var branchIdListLocation = _efContext.Database.SqlQuery<LocationBoundaryResult>(query)
                    .Distinct()
                    .Select(x => x.BranchId)
                    .ToList<int>();
                var curUserId = 0;
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    curUserId = Convert.ToInt32(currentUserId);
                }
                //hardcoded as 300 for now. 
                var offersquery = _productRepository.GetOffersQuery(branchIdListLocation, 300, curUserId);
                var offerList = _efContext.Database.SqlQuery<ProductModelWithCategory>(offersquery).ToList();
                foreach (var eachData in offerList)
                {
                    if (eachData.PictureName != null)
                    {
                        if (!eachData.PictureName.Contains("http"))
                        {
                            eachData.PictureName = _appSettings.ImageUrlBase + eachData.PictureName;
                        }
                    }
                    eachData.ProductRating = _ratingHelper.CalulateProductRating(eachData.ProductId);
                    eachData.ProductRatingCount = _context.ProductRating.Where(x => x.ProductId == eachData.ProductId).Count();
                }
                return offerList;
            }
            catch (Exception EX)
            {
                return null;
            }
        }

        [HttpGet("GetProductComparison/{ids}")]
        [HttpGet("Seller/{BranchId}/GetProductComparison/{ids}")]
        public List<ProductComparisonResult> GetProductComparison(string ids)
        {
            try
            {
                List<int> productIds = ids.Split(',').Select(int.Parse).ToList();
                if (productIds.Count <= 4)
                {
                    var query = _productFeaturesHelper.GetProductComparisonQuery(productIds);
                    var comparisonResult = _efContext.Database.SqlQuery<ProductComparisonResult>(query).ToList();
                    return comparisonResult;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        [HttpGet("GetProductDetailedComparison/{ids}")]
        [HttpGet("Seller/{BranchId}/GetProductDetailedComparison/{ids}")]
        public List<ProductDetailedComparisonResult> GetProductDetailedComparison(string ids)
        {
            try
            {
                List<int> productIds = ids.Split(',').Select(int.Parse).ToList();
                if (productIds.Count <= 4)
                {
                    var query = _productFeaturesHelper.GetProductDetailedComparisonQuery(productIds);
                    var comparisonResult = _efContext.Database.SqlQuery<ProductDetailedComparisonResult>(query).ToList();
                    return comparisonResult;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        //get product with variants
        [HttpGet("Seller/{StoreId}/GetProductDetailsWithVariant/{id}/{flagLocation}")]
        public ProductDetailModelWithVariant GetProductDetailsWithVariant(int StoreId, int id, bool flagLocation)
        {
            return GetProductDetailsWithVariant(StoreId, id, flagLocation, null, null, null);
        }
        [HttpGet("GetProductDetailsWithVariant/{id}/{flagLocation}/{lat}/{lng}/{mapRadius}")]
        [HttpGet("Seller/{StoreId}/GetProductDetailsWithVariant/{id}/{flagLocation}/{lat}/{lng}/{mapRadius}")]
        public ProductDetailModelWithVariant GetProductDetailsWithVariant(int StoreId, int id, bool flagLocation, decimal? lat, decimal? lng, int? mapRadius)
        {

            if (!flagLocation)
            {
                return GetProductDetailsWithVariants(StoreId, id, flagLocation, null);
            }
            var branchIdListLocation = GetBranchIdListBasedOnLocation(StoreId, lat, lng, mapRadius);
            var productDetailModel = GetProductDetailsWithVariants(StoreId, id, flagLocation, branchIdListLocation);
            if (productDetailModel != null)
            {
                productDetailModel.RelatedProductList = GetRelatedProducts(id, StoreId, branchIdListLocation);
                foreach (var product in productDetailModel.RelatedProductList)
                {
                    var productImages = _context.ProductImage.Where(x => x.ProductId == product.ProductId).FirstOrDefault();
                    if (productImages != null)
                    {
                        if (!productImages.PictureName.Contains("http"))
                        {
                            product.PictureName = _appSettings.ImageUrlBase + productImages.PictureName;
                        }
                        else
                        {
                            product.PictureName = productImages.PictureName;
                        }
                    }
                    var pricing = _context.Pricing.Where(x => x.Product == product.ProductId).FirstOrDefault();

                    if (pricing != null)
                    {
                        var branchDetails = _context.SellerBranch.Where(x => x.BranchId == pricing.Branch).FirstOrDefault();
                        product.AdditionalShippingCharge = pricing.AdditionalShippingCharge;
                        product.DeliveryTime = pricing.DeliveryTime;

                        if (branchDetails != null)
                        {
                            product.BranchName = branchDetails.BranchName;
                        }
                    }
                    product.ProductRating = _ratingHelper.CalulateProductRating(product.ProductId);
                    product.ProductRatingCount = _context.ProductRating.Where(x => x.ProductId == product.ProductId).Count();
                }
            }
            return productDetailModel;
        }
        private ProductDetailModelWithVariant GetProductDetailsWithVariants(int BranchId, int id, bool flagLocation, List<int> branchIdList)
        {
            var productDetailModel = _productRepository.GetProductDetailsForVariant(id, BranchId);
            if (productDetailModel != null)
            {
                var inventoryQuantityExceed = _cartRepository.CheckQuantityExists(id);
                var availableQuantity = _cartRepository.getAvailableQuantity(id);

                //rating
                productDetailModel.ProductRating = _ratingHelper.CalulateProductRating(id);
                productDetailModel.ProductRatingCount = _context.ProductRating.Where(x => x.ProductId == id).Count();
                productDetailModel.StorePricingModel = new List<StorePricingModel>();
                productDetailModel.StorePricingModelForVbuy = new List<StorePricingModel>();

                productDetailModel.ProductImages = new List<string>();
                //Images

                if (productDetailModel != null && productDetailModel.ProductId > 0)
                {
                    var productImages = _context.ProductImage.Where(x => x.ProductId == id).ToList();
                    foreach (var images in productImages)
                    {
                        if (images.PictureName != null)
                        {
                            if (!images.PictureName.Contains("http"))
                            {
                                productDetailModel.ProductImages.Add(_appSettings.ImageUrlBase + images.PictureName);
                            }
                            else
                            {
                                productDetailModel.ProductImages.Add(images.PictureName);
                            }
                        }
                    }
                }
                var VariantOptions = _context.VariantOptions.Where(x => x.ProductId == id && x.FlagDeleted != true).ToList();
                productDetailModel.VariantOptions = new List<string>();
                foreach (var eachOption in VariantOptions)
                {
                    productDetailModel.VariantOptions.Add(eachOption.Options);
                }
                var productVariantsOptions1 = _context.ProductVariants.Where(x => x.ProductId == id && x.FlagDeleted != true).Select(x => x.Option1).ToList();
                var option1 = productVariantsOptions1.Distinct().ToList();
                var productVariantsOptions2 = _context.ProductVariants.Where(x => x.ProductId == id && x.FlagDeleted != true).Select(x => x.Option2).ToList();
                var option2 = productVariantsOptions2.Distinct().ToList();

                var productVariantsOptions3 = _context.ProductVariants.Where(x => x.ProductId == id && x.FlagDeleted != true).Select(x => x.Option3).ToList();
                var option3 = productVariantsOptions3.Distinct().ToList();

                productDetailModel.option1 = option1;
                productDetailModel.option2 = option2;
                productDetailModel.option3 = option3;

                var productVariants = _context.ProductVariants.Where(x => x.ProductId == id && x.FlagDeleted != true).ToList();
                productDetailModel.ProductVariants = new List<VariantsDetail>();
                foreach (var eachVariants in productVariants)
                {
                    if (eachVariants != null)
                    {
                        VariantsDetail variants = new VariantsDetail();
                        variants.ProductVariantId = eachVariants.Id;
                        variants.Option1 = eachVariants.Option1;
                        variants.Option2 = eachVariants.Option2;
                        variants.Option3 = eachVariants.Option3;
                        productDetailModel.ProductVariants.Add(variants);
                    }
                }

                //include the lat and longitude and also location. 
                if (productDetailModel != null && productDetailModel.ProductId > 0)
                {
                    var pricingDetails = _context.Pricing.Where(x => x.Product == id && x.IsDeleted != true && (flagLocation ? branchIdList.Contains(x.Branch) : x.Branch != null)).
                        Include(y => y.BranchDetails).Include(z => z.BranchDetails.SellerMap).OrderBy(z => z.SpecialPrice).OrderByDescending(y => y.UpdatedOnUtc).ToList();
                    if (BranchId > 0)
                    {
                        pricingDetails = _context.Pricing.Where(x => x.Product == id && x.IsDeleted != true && x.Branch == BranchId && (flagLocation ? branchIdList.Contains(x.Branch) : x.Branch != null)).
                        Include(y => y.BranchDetails).Include(z => z.BranchDetails.SellerMap).OrderBy(z => z.SpecialPrice).OrderByDescending(y => y.UpdatedOnUtc).ToList();
                    }
                    foreach (var pricing in pricingDetails)
                    {
                        var storePricingModel = new StorePricingModel()
                        {
                            BranchId = pricing.Branch,
                            BranchName = pricing.BranchDetails.BranchName,
                            StoreId = pricing.BranchDetails.SellerMap.StoreId,
                            StoreName = pricing.BranchDetails.SellerMap.StoreName,
                            BranchAddress1 = pricing.BranchDetails.Address1,
                            BranchAddress2 = pricing.BranchDetails.Address2,
                            BranchCity = pricing.BranchDetails.City,
                            Latitude = pricing.BranchDetails.Latitude,
                            Longitude = pricing.BranchDetails.Longitude,
                            EnableBuy = pricing.BranchDetails.EnableBuy,
                            ProductVariantId = pricing.ProductVariantId,
                            Price = pricing.Price,
                            SpecialPrice = pricing.SpecialPrice,
                            BranchRating = GetSellerRating(pricing.Branch),
                            AdditionalShippingCharge = pricing.AdditionalShippingCharge,
                            AdditionalTax = pricing.AdditionalTax,
                            DeliveryTime = pricing.DeliveryTime,
                            SpecialPriceDescription = pricing.SpecialPriceDescription,
                            FlagQuantityExceeded = inventoryQuantityExceed,
                            AvailableQuantity = availableQuantity
                        };
                        productDetailModel.StorePricingModel.Add(storePricingModel);
                    }

                    var distinctList = pricingDetails.DistinctBy(x => x.Branch).ToList();
                    foreach (var pricing in distinctList)
                    {
                        var storePricingModelForVbuy = new StorePricingModel()
                        {
                            BranchId = pricing.Branch,
                            BranchName = pricing.BranchDetails.BranchName,
                            StoreId = pricing.BranchDetails.SellerMap.StoreId,
                            StoreName = pricing.BranchDetails.SellerMap.StoreName,
                            BranchAddress1 = pricing.BranchDetails.Address1,
                            BranchAddress2 = pricing.BranchDetails.Address2,
                            BranchCity = pricing.BranchDetails.City,
                            Latitude = pricing.BranchDetails.Latitude,
                            Longitude = pricing.BranchDetails.Longitude,
                            EnableBuy = pricing.BranchDetails.EnableBuy,
                            Price = pricing.Price,
                            SpecialPrice = pricing.SpecialPrice,
                            BranchRating = GetSellerRating(pricing.Branch),
                            AdditionalShippingCharge = pricing.AdditionalShippingCharge,
                            AdditionalTax = pricing.AdditionalTax,
                            DeliveryTime = pricing.DeliveryTime,
                            SpecialPriceDescription = pricing.SpecialPriceDescription,
                            FlagQuantityExceeded = inventoryQuantityExceed,
                            AvailableQuantity = availableQuantity
                        };
                        productDetailModel.StorePricingModelForVbuy.Add(storePricingModelForVbuy);
                    }
                    //Category details
                    if (productDetailModel.Category > 0)
                    {
                        var category = _context.Category.Where(x => x.CategoryId == productDetailModel.Category).FirstOrDefault();
                        productDetailModel.CategoryName = category.Name;
                        productDetailModel.SubCategoryPermaLink = category.PermaLink;
                        productDetailModel.ParentCategoryId = category.ParentCategoryId;
                        productDetailModel.CategoryGroupTag = category.CategoryGroupTag;

                        var parentCategory = _context.Category.Where(x => x.CategoryId == category.ParentCategoryId).FirstOrDefault();
                        productDetailModel.ParentCategoryName = parentCategory.Name;
                        productDetailModel.ParentCategoryPermaLink = parentCategory.PermaLink;
                    }
                }
                productDetailModel.AndroidInformation1 = @"Availability: Our Service is available only in Chennai, and certain cities of Tamilnadu. If stock is not available in the preferred store, VBuy will get you the same product from another store with same price. 
                * Some registered sellers choose not to avail our map service and will also be shown outside the selected map area.";
            }
            return productDetailModel;
        }

        //Authorize
        [HttpGet("GetProductRating/{productId}")]
        [HttpGet("Seller/{BranchId}/GetProductRating/{productId}")]
        public IActionResult GetProductRating(int BranchId, int productId)
        {
            try
            {
                var productRating = _ratingHelper.GetProductRating(productId);
                var productRatingList = _mapper.Map<List<ProductRatingResult>>(productRating);
                return Ok(productRatingList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("UpdateProductRating/{productId}/{rating}/{userName}")]
        [HttpGet("Seller/{BranchId}/UpdateProductRating/{productId}/{rating}/{userName}")]
        public IActionResult UpdateProductRating(int BranchId, int productId, int rating, string userName)
        {
            try
            {
                if (BranchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var curUserId = 0;
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;

                if (!string.IsNullOrEmpty(currentUser))
                {
                    var user = _userService.GetUser(Convert.ToInt32(currentUserId));
                    if (user != null)
                    {
                        curUserId = user.UserId;
                    }
                    if (user != null && (userName.ToLower() == user.Email || userName.ToLower() == user.PhoneNumber1.ToLower()))
                    {
                        _ratingHelper.InsertProductRatingQuery(productId, rating, curUserId, 1, 5);

                        var productRating = _ratingHelper.GetProductRating(productId);
                        return Ok(productRating);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [Authorize(Roles = "Administrators , StoreAdmin , StoreModerator, Registered, SalesSupport, Support, Marketing")]
        [HttpGet("Seller/{StoreId}/GetUserWishlistProducts/{pageStart}/{pageSize}")]
        public IActionResult GetUserWishlistProducts(int StoreId, int? pageStart, int? pageSize)
        {
            var storeIds = User.FindAll("StoreId").ToList();
            if (!storeIds.Where(a => a.Value == StoreId.ToString()).Any())
            {
                return Unauthorized();
            }
            return GetUserWishlistProducts(StoreId, null, null, null, pageStart, pageSize);
        }

        [Authorize(Roles = "Administrators , StoreAdmin , StoreModerator, Registered, SalesSupport, Support, Marketing")]
        [HttpGet("GetUserWishlistProducts/{lat}/{lng}/{mapRadius}/{pageStart}/{pageSize}")]
        [HttpGet("Seller/{StoreId}/GetUserWishlistProducts/{lat}/{lng}/{mapRadius}/{pageStart}/{pageSize}")]
        public IActionResult GetUserWishlistProducts(int StoreId, decimal? lat, decimal? lng, int? mapRadius, int? pageStart, int? pageSize)
        {
            try
            {
                if (StoreId > 0)
                {
                    var storeIds = User.FindAll("StoreId").ToList();
                    if (!storeIds.Where(a => a.Value == StoreId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(StoreId, lat, lng, mapRadius);
                var branchIdListLocation = _efContext.Database.SqlQuery<LocationBoundaryResult>(query)
                    .Distinct()
                    .Select(x => x.BranchId)
                    .ToList<int>();
                var partnerStoreList = _context.SellerBranch.Where(x => x.FlagPartner == true).Select(x => x.BranchId).ToList<int>();

                foreach (var partnerStore in partnerStoreList)
                {
                    branchIdListLocation.Add(partnerStore);
                }
                var curUserId = 0;
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    curUserId = Convert.ToInt32(currentUserId);
                }
                var productquery = _productRepository.GetUserWishlistProductsQuery(branchIdListLocation, pageStart, pageSize, curUserId);
                var productWishlist = _efContext.Database.SqlQuery<ProductModelWithCategory>(productquery).ToList();
                foreach (var eachData in productWishlist)
                {
                    if (eachData.PictureName != null)
                    {
                        if (!eachData.PictureName.Contains("http"))
                        {
                            eachData.PictureName = _appSettings.ImageUrlBase + eachData.PictureName;
                        }
                    }
                }
                return Ok(productWishlist);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("ContactSeller")]
        [HttpPost("Seller/{BranchId}/ContactSeller")]
        public IActionResult ContactSeller(int BranchId, ProductContactResultDTO productContactResultSet)
        {
            try
            {
                if (BranchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                //get brandh details.
                var sellerBranch = _context.SellerBranch.Where(x => x.BranchId == productContactResultSet.Branchid).FirstOrDefault();
                var productDetails = _context.ProductStoreMapping.Where(x => x.ProductId == productContactResultSet.ProductId).FirstOrDefault();

                if (AllowAdd(productContactResultSet.ProductId, productContactResultSet.Branchid, productContactResultSet.Name, productContactResultSet.Email,
                    productContactResultSet.Mobile, productContactResultSet.Subject))
                {
                    _sellerContactHelper.InsertSellerContactQuery(productContactResultSet.ProductId, productContactResultSet.Branchid,
                       productContactResultSet.Name, productContactResultSet.Email, productContactResultSet.Mobile, productContactResultSet.Subject);


                    try
                    {
                        _mailHelper.SendProductRequestMail(sellerBranch.BranchName, sellerBranch.Email, productContactResultSet.Name,
                            productContactResultSet.Email, productContactResultSet.Mobile, productDetails.Name, productContactResultSet.Subject);
                        _messageHelper.SendProductRequestMessage(sellerBranch.PhoneNumber, productDetails.Name, productContactResultSet.Subject, productContactResultSet.Email,
                            productContactResultSet.Mobile);
                    }
                    catch (Exception ex)
                    {

                    }
                    return Ok("success");
                }
                //You can only sent one message to the store for same product in 2 hours.
                return Ok("failed");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        private bool AllowAdd(int? productId, int? branchid, string name, string email, string mobile, string subject)
        {
            try
            {
                var verifyInboxquery = _sellerContactHelper.VerifyDuplicateInbox(productId, branchid, name, email, mobile);
                int count = _efContext.Database.SqlQuery<int>(verifyInboxquery).FirstOrDefault();
                if (count > 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return true;
            }
        }
    }
}
