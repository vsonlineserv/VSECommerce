using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.Caching;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Domain.Settings;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class LandingController : VSControllerBase
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly ProductRepository _productRepository;
        private readonly UserService _userService;
        private readonly DataContext _context;
        private readonly EfContext _efContext;
        private readonly ProductHelper _productHelper;
        private readonly SellerRepository _sellerRepository;
        private readonly SiteSettingsService _siteSettingsService;
        private readonly IMemoryCache _cache;
        private readonly DefaultCache _cacheManager;
        private readonly SellerBranchRepository _sellerBranchRepository;
        private readonly RatingHelper _ratingHelper;
        private HttpContext _httpContext => new HttpContextAccessor().HttpContext;
        public LandingController(SiteSettingsService siteSettingsService, SellerRepository sellerRepository, ProductHelper productHelper,
            ProductRepository productRepository, DataContext dataContext, UserService userService, CategoryRepository categoryRepository,
            IOptions<AppSettings> _appSettings, DefaultCache cacheManager, IMemoryCache cache, EfContext efContext, SellerBranchRepository sellerBranchRepository, RatingHelper ratingHelper) : base(_appSettings)
        {
            _categoryRepository = categoryRepository;
            _productRepository = productRepository;
            _userService = userService;
            _context = dataContext;
            _productHelper = productHelper;
            _sellerRepository = sellerRepository;
            _siteSettingsService = siteSettingsService;
            _cache = cache;
            _cacheManager = cacheManager;
            _efContext = efContext;
            _sellerBranchRepository = sellerBranchRepository;
            _ratingHelper = ratingHelper;
        }

        [HttpGet("GetMainMenu")]
        [HttpGet("GetMainMenu/{lat}/{lng}/{mapRadius}")]
        public List<MenuResult> GetMainMenu(decimal lat, decimal lng, int mapRadius)
        {
            try
            {
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(0, lat, lng, mapRadius);
                var branchIdList = _efContext.Database.SqlQuery<LocationBoundaryResult>(query).Distinct().Select(x => x.BranchId).ToList<int>();

                var menuResult = _categoryRepository.GetCategoryMenu(branchIdList);
                return menuResult;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("GetFeaturedProducts/{lat}/{lng}/{mapRadius}")]
        public List<ProductModel> GetFeaturedProducts(decimal lat, decimal lng, int mapRadius)
        {
            try
            {
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(0, lat, lng, mapRadius);
                var branchIdList = _efContext.Database.SqlQuery<LocationBoundaryResult>(query).Distinct().Select(x => x.BranchId).ToList<int>();

                var productModelList = _productRepository.GetFeaturedProducts(branchIdList);
                var curUserId = 0;
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    curUserId = Convert.ToInt32(currentUserId);
                }
                var wishList = _context.UserWishlist.Where(x => x.User == curUserId).ToList();

                foreach (var product in productModelList)
                {
                    var wishListProduct = wishList.Find(x => x.Product == product.ProductId);
                    var productImages = _context.ProductImage.Where(x => x.ProductId == product.ProductId).FirstOrDefault();
                    if (wishListProduct != null)
                    {
                        product.FlagWishlist = true;
                    }
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
                    product.ProductRating = _ratingHelper.CalulateProductRating(product.ProductId);
                    product.ProductRatingCount = _context.ProductRating.Where(x=>x.ProductId == product.ProductId).Count();
                }
                return productModelList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("GetTopSellingProductList1/{lat}/{lng}/{mapRadius}")]
        public List<ProductModelWithCategory> GetTopSellingProductList1(decimal lat, decimal lng, int mapRadius)
        {
            try
            {
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(0, lat, lng, mapRadius);
                var branchIdList = _efContext.Database.SqlQuery<LocationBoundaryResult>(query).Distinct().Select(x => x.BranchId).ToList<int>();
               
                var topSellingProducts = _context.OrderProduct
                   .Join(_context.OrderProductItem, x => new {id1 = x.Id}, y => new {id1 = y.OrderId }, (x, y) => new { x, y }).Where(r=>r.x.OrderFromVbuy == true)
                   .Join(_context.ProductStoreMapping, z => new {id1 = z.y.ProductId, id2 =z.y.BranchId }, t => new {id1 = t.ProductId,id2 = t.BranchId }, (z, t) => new { z, t })
                   .Where(w => branchIdList.Contains(w.z.y.BranchId) && w.z.x.OrderFromVbuy == true).Select(s => new ProductModelWithCategory
                   {
                       ProductId = s.t.ProductId,
                       Name = s.t.Name,
                       PermaLink = s.t.PermaLink,
                       Price = _context.Pricing.Where(x => x.Product == s.t.ProductId).Select(x => x.Price).FirstOrDefault(),
                       SpecialPrice = _context.Pricing.Where(x => x.Product == s.t.ProductId).Select(x => x.SpecialPrice).FirstOrDefault(),
                       CategoryId = s.t.Category,
                       ProductRating = _ratingHelper.CalulateProductRating(s.t.ProductId),
                       ProductRatingCount = _context.ProductRating.Where(x => x.ProductId == s.t.ProductId).Count(),
                       BranchId = s.t.BranchId,
                       PictureName = (!string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == s.t.ProductId).FirstOrDefault().PictureName) && !_context.ProductImage.Where(b => b.ProductId == s.t.ProductId).FirstOrDefault().PictureName.Contains("http")) ? _appSettings.ImageUrlBase + _context.ProductImage.Where(b => b.ProductId == s.t.ProductId).FirstOrDefault().PictureName : !string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == s.t.ProductId).FirstOrDefault().PictureName) ? _context.ProductImage.Where(b => b.ProductId == s.t.ProductId).FirstOrDefault().PictureName : "",
                   }).Distinct().ToList();
                return topSellingProducts;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private List<ProductModelWithCategory> GetProductBasedonCategory(SiteSettings categoryKey, string enumCacheString, List<int> branchIdList)
        {
            try
            {
                if (categoryKey != null && !string.IsNullOrEmpty(categoryKey.Value))
                {
                    var cacheProductModelList = _cacheManager.GetProductModelCategory(enumCacheString, categoryKey.Value, branchIdList);

                    var curUserId = 0;
                    var currentUser = User.Identity.Name;
                    var currentUserId = User.FindFirst("UserId")?.Value;
                    if (!string.IsNullOrEmpty(currentUserId))
                    {
                        curUserId = Convert.ToInt32(currentUserId);
                    }
                    var wishList = _context.UserWishlist.Where(x => x.User == Convert.ToInt32(currentUserId)).ToList();

                    foreach (var product in cacheProductModelList)
                    {
                        var wishListProduct = wishList.Find(x => x.Product == product.ProductId);
                        if (wishListProduct != null)
                        {
                            product.FlagWishlist = true;
                        }
                    }
                    return cacheProductModelList;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [HttpGet("GetSearchAreaFilter/{city}")]
        public List<AreaResult> GetSearchAreaFilter(string city)
        {
            if (_httpContext.Items["SearchArea"] == null)
            {
                var query = _sellerRepository.GetAllSearchAreaQuery();
                _httpContext.Items["SearchArea"] = _efContext.Database.SqlQuery<AreaResult>(query).ToList<AreaResult>();
                _httpContext.Items["SearchArea"] = _context.Area.ToList();
            }

            return _httpContext.Items["SearchArea"] as List<AreaResult>;
        }

        [HttpGet("GetPersonalizedProductList1/{lat}/{lng}/{mapRadius}")]
        public List<ProductModelWithCategory> GetPersonalizedProductList1(decimal lat, decimal lng, int mapRadius)
        {
            try
            {
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(0, lat, lng, mapRadius);
                var branchIdList = _efContext.Database.SqlQuery<LocationBoundaryResult>(query).Distinct().Select(x => x.BranchId).ToList<int>();
                List<SiteSettings> siteSettingsList = _siteSettingsService.GetSiteSettings();

                var categoryKey = siteSettingsList.Find(x => x.SiteKey == Enums.SiteSettings.TopSellingProductCategory2.ToString());
                return GetProductBasedonCategory(categoryKey, Enums.Home_TopProductList2_CACHE_KEY, branchIdList);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("GetTopCategoriesList/{lat}/{lng}/{mapRadius}")]
        public List<CategoryHomePageModelResult> GetTopCategoriesList(decimal lat, decimal lng, int mapRadius)
        {
            try
            {
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(0, lat, lng, mapRadius);
                var branchIdList = _efContext.Database.SqlQuery<LocationBoundaryResult>(query).Distinct().Select(x => x.BranchId).ToList<int>();

                var categoryHomePageModel = _context.Category.Where(x => branchIdList.Contains((int)x.BranchId) && x.ShowOnHomePage == true && x.IsDeleted != true).ToList();
                List<CategoryHomePageModelResult> catHomePageModelList = new List<CategoryHomePageModelResult>();
                foreach (var c in categoryHomePageModel)
                {
                    CategoryHomePageModelResult model = new CategoryHomePageModelResult();
                    model.CategoryGroupTag = c.CategoryGroupTag;
                    model.CategoryId = c.CategoryId;
                    model.Name = c.Name;
                    model.CategoryImage = c.CategoryImage;
                    if (c.CategoryImage != null)
                    {
                        if (!c.CategoryImage.Contains("http"))
                        {
                            model.CategoryImage = _appSettings.ImageUrlBase + c.CategoryImage;
                        }
                        else
                        {
                            model.CategoryImage = c.CategoryImage;
                        }
                    }
                    model.PermaLink = c.PermaLink;
                    catHomePageModelList.Add(model);
                }
                return catHomePageModelList;
            }
            catch
            {
                return null;
            }
        }
        [HttpGet("GetApplicationData")]
        public Dictionary<string, string> GetApplicationData()
        {
            try
            {
                //To Configure for each account.
                Dictionary<string, string> appSettings = new Dictionary<string, string>();
                appSettings.Add("imageUrlBaseUploaded", _appSettings.ImageUrlBaseUploaded);

                appSettings.Add("imageUrlBase", _appSettings.ImageUrlBase);
                appSettings.Add("imageUrlBaseStandard", _appSettings.ImageUrlBaseStandard);
                appSettings.Add("imageUrlBaseSmall", _appSettings.ImageUrlBaseSmall);
                appSettings.Add("imageUrlBaseLarge", _appSettings.ImageUrlBaseLarge);
                appSettings.Add("ApplicationHosting", _appSettings.ApplicationHosting);
                appSettings.Add("homeFolder", _appSettings.HomeFolder);
                appSettings.Add("homeCategoryFolder", _appSettings.HomeCategoryFolder);

                return appSettings;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [HttpGet("GetHomeBannerSettings")]
        public List<SiteSettings> GetHomeBannerSettings()
        {
            try
            {
                List<SiteSettings> siteSettingsList = _siteSettingsService.GetSiteSettings();
                var homeBannerSiteSettings = siteSettingsList.Where(x => x.SiteKey.Contains("HomeBanner")).ToList();
                return homeBannerSiteSettings;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //for single db
        [HttpGet("Seller/{BranchId}/GetMainMenu")]
        public List<MenuResult> GetMainMenu(int BranchId)
        {
            try
            {
                var menuResult = _categoryRepository.GetCategoryMenu_(BranchId);
                return menuResult;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        [HttpGet("Seller/{BranchId}/GetFeaturedProducts")]
        public List<ProductModel> GetFeaturedProducts(int BranchId)
        {
            try
            {
                var productModelList = _productRepository.GetFeaturedProducts_(BranchId);
                var curUserId = 0;
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    curUserId = Convert.ToInt32(currentUserId);
                }
                var wishList = _context.UserWishlist.Where(x => x.User == curUserId).ToList();

                foreach (var product in productModelList)
                {
                    var wishListProduct = wishList.Find(x => x.Product == product.ProductId);
                    var productImages = _context.ProductImage.Where(x => x.ProductId == product.ProductId).FirstOrDefault();
                    if (wishListProduct != null)
                    {
                        product.FlagWishlist = true;
                    }
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
                    product.ProductRating = _ratingHelper.CalulateProductRating(product.ProductId);

                }
                return productModelList;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("Seller/{BranchId}/GetTopSellingProductList1")]
        public List<ProductModelWithCategory> GetTopSellingProductList1(int BranchId)
        {
            try
            {
                List<SiteSettings> siteSettingsList = _siteSettingsService.GetSiteSettings();

                var categoryKey = siteSettingsList.Find(x => x.SiteKey == Enums.SiteSettings.TopSellingProductCategory.ToString());
                return GetProductBasedonCategory(categoryKey, Enums.Home_TopProductList1_CACHE_KEY, BranchId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private List<ProductModelWithCategory> GetProductBasedonCategory(SiteSettings categoryKey, string enumCacheString, int BranchId)
        {
            try
            {
                if (categoryKey != null && !string.IsNullOrEmpty(categoryKey.Value))
                {
                    var cacheProductModelList = _cacheManager.GetProductModelCategory(enumCacheString, categoryKey.Value, BranchId);

                    var curUserId = 0;
                    var currentUser = User.Identity.Name;
                    var currentUserId = User.FindFirst("UserId")?.Value;
                    if (!string.IsNullOrEmpty(currentUserId))
                    {
                        curUserId = Convert.ToInt32(currentUserId);
                    }
                    var wishList = _context.UserWishlist.Where(x => x.User == curUserId).ToList();

                    foreach (var product in cacheProductModelList)
                    {
                        var wishListProduct = wishList.Find(x => x.Product == product.ProductId);
                        if (wishListProduct != null)
                        {
                            product.FlagWishlist = true;
                        }
                    }
                    return cacheProductModelList;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("Seller/{BranchId}/GetPersonalizedProductList1")]
        public List<ProductModelWithCategory> GetPersonalizedProductList1(int BranchId)
        {
            try
            {
                List<SiteSettings> siteSettingsList = _siteSettingsService.GetSiteSettings();

                var categoryKey = siteSettingsList.Find(x => x.SiteKey == Enums.SiteSettings.TopSellingProductCategory2.ToString());
                return GetProductBasedonCategory(categoryKey, Enums.Home_TopProductList2_CACHE_KEY, BranchId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("Seller/{BranchId}/GetTopCategoriesList")]
        public List<CategoryHomePageModelResult> GetTopCategoriesList(int BranchId)
        {
            try
            {
                var categoryHomePageModel = _context.Category.Where(x => x.ShowOnHomePage == true && x.BranchId == BranchId && x.IsDeleted != true).ToList();
                List<CategoryHomePageModelResult> catHomePageModelList = new List<CategoryHomePageModelResult>();
                foreach (var c in categoryHomePageModel)
                {
                    CategoryHomePageModelResult model = new CategoryHomePageModelResult();
                    model.CategoryGroupTag = c.CategoryGroupTag;
                    model.CategoryId = c.CategoryId;
                    model.Name = c.Name;
                    if (c.CategoryImage != null)
                    {
                        if (!c.CategoryImage.Contains("http"))
                        {
                            model.CategoryImage = _appSettings.ImageUrlBase + c.CategoryImage;
                        }
                        else
                        {
                            model.CategoryImage = c.CategoryImage;
                        }
                    }
                    model.PermaLink = c.PermaLink;
                    catHomePageModelList.Add(model);
                }
                return catHomePageModelList;
            }
            catch
            {
                return null;
            }
        }
    }
}
