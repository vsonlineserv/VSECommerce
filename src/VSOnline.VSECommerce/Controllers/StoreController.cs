using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Permission;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Controllers
{

    [Route("")]
    [ApiController]
    public class StoreController : VSControllerBase
    {
        private readonly EfContext _efContext;
        private readonly DataContext _context;
        private readonly SellerRepository _sellerRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly ManufacturerRepository _manufacturerRepository;
        private readonly RatingHelper _ratingHelper;
        private readonly UserService _userService;
        private readonly PricingRepository _pricingRepository;
        private readonly IMapper _mapper;
        private readonly SellerBranchRepository _sellerBranchRepository;
        public StoreController(IMapper mapper, EfContext efContext, DataContext context, IOptions<AppSettings> _appSettings, SellerRepository sellerRepository, CategoryRepository categoryRepository, ManufacturerRepository manufacturerRepository, RatingHelper ratingHelper, UserService userService, PricingRepository pricingRepository, SellerBranchRepository sellerBranchRepository) : base(_appSettings)
        {
            _efContext = efContext;
            _context = context;
            _sellerRepository = sellerRepository;
            _categoryRepository = categoryRepository;
            _manufacturerRepository = manufacturerRepository;
            _ratingHelper = ratingHelper;
            _userService = userService;
            _pricingRepository = pricingRepository;
            _mapper = mapper;
            _sellerBranchRepository = sellerBranchRepository;
        }

        [Authorize(Roles = "Administrators , StoreAdmin , SalesSupport, StoreModerator")]
        [HttpGet("GetRetailerInfo")]
        public RetailerInfoResult GetRetailerInfo()
        {
            var currentUser = User.FindFirst("UserId")?.Value;
            var userId = Convert.ToInt64(currentUser);
            var RetailerInfo = _context.Seller.Where(x => x.PrimaryContact == userId).Include(y => y.Branches).FirstOrDefault();
            if (RetailerInfo != null)
            {
                var RetailerInfoDetails = _sellerRepository.GetStoreDetails(RetailerInfo);
                if (RetailerInfoDetails != null)
                {
                    return RetailerInfoDetails;
                }
            }

            var RetailerInfoStaff = _context.SellerStaffMapping.Where(x => x.UserId == userId).FirstOrDefault<SellerStaffMapping>();
            if (RetailerInfoStaff != null)
            {
                var BranchDetailsList = _context.SellerBranch.Where(x => x.BranchId == RetailerInfoStaff.BranchId).ToList<SellerBranch>();
                RetailerInfoStaff.Branches = BranchDetailsList;
                var RetailerInfoStaffDetails = _sellerRepository.GetStoreDetailsForStaff(RetailerInfoStaff);
                if (RetailerInfoStaffDetails != null)
                {
                    return RetailerInfoStaffDetails;
                }
            }
            return null;
        }
        [HttpGet("GetStoreInfo/{id}")]
        public RetailerInfoResult GetStoreInfo(int id)
        {
            var seller = _context.Seller.Where(x => x.StoreId == id).Include(y => y.Branches).FirstOrDefault();
            if (seller != null)
            {
                var RetailerInfoDetails = _sellerRepository.GetStoreDetailsForTemplate(seller);
                if (RetailerInfoDetails != null)
                {
                    return RetailerInfoDetails;
                }
            }
            return null;
        }


        [HttpGet("UpdateLogo")]
        public IActionResult UpdateLogo(int domainId, string fileName)
        {
            try
            {
                var logo = _context.Seller.Where(x => x.StoreRefereneId == domainId).FirstOrDefault();
                if (logo != null)
                {
                    logo.LogoPicture = fileName;
                    logo.UpdatedOnUtc = DateTime.UtcNow;
                    _context.Seller.Update(logo);
                    _context.SaveChanges();
                    return Ok("Logo updated successfully");
                }
                return BadRequest("Error in Updating Logo");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.ToString());
            }
        }
        // for Hyperlocal
        [HttpGet("GetStoreInfoForVbuy/{id}")]
        public RetailerInfoResult GetStoreInfoForVbuy(int id)
        {
            var seller = _context.Seller.Where(x => x.StoreId == id).Include(y => y.Branches).FirstOrDefault();
            if (seller != null)
            {
                var RetailerInfoDetails = _sellerRepository.GetStoreDetails(seller);
                if (RetailerInfoDetails != null)
                {
                    return RetailerInfoDetails;
                }
            }
            return null;
        }

        [HttpGet("GetStoresProductFilter/{storeId}/{productId}")]
        public RetailerProductFilterResult GetStoresProductFilter(int storeId, int productId)
        {
            var retailerProductFilterResult = new RetailerProductFilterResult();
            var storeCategoryQuery = _categoryRepository.GetStoresCategoryQuery(storeId);
            var categoryList = _efContext.Database.SqlQuery<int>(storeCategoryQuery).ToList<int>();

            var selectedSubCategory = _context.ProductStoreMapping.Where(x => x.ProductId == productId).Select(y => y.Category).FirstOrDefault<int>();
            var selectedCategory = _context.Category.Where(x => x.CategoryId == selectedSubCategory).Select(y => y.ParentCategoryId).FirstOrDefault<int?>();

            retailerProductFilterResult.SelectedFilters = new RetailerProductSelectedFilter { SelectedCategory = selectedCategory, SelectedSubCategory = selectedSubCategory };


            retailerProductFilterResult = _categoryRepository.GetRetailerProductFilterResult(retailerProductFilterResult, categoryList);
            retailerProductFilterResult.Brands = _manufacturerRepository.GetBrands(storeId);
            return retailerProductFilterResult;
        }
        [HttpGet("GetBranchRating/{branchId}")]
        public IActionResult GetBranchRating(int branchId)
        {
            try
            {
                var sellerRating = _ratingHelper.GetSellerRating(branchId);
                return Ok(sellerRating);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("UpdateBranchRating/{branchId}/{rating}/{userName}")]
        public IActionResult UpdateBranchRating(int branchId, int rating, string userName)
        {
            try
            {
                var curUserId = 0;
                var currentUser = User.Identity.Name;
                if (!string.IsNullOrEmpty(currentUser))
                {
                    var currentUserId = User.FindFirst("UserId")?.Value;
                    var user = _userService.GetUser(Convert.ToInt16(currentUserId));
                    if (user != null)
                    {
                        curUserId = user.UserId;
                    }
                    if (user != null && (userName.ToLower() == user.Email || userName.ToLower() == user.PhoneNumber1.ToLower()))
                    {
                        //1,5 hardcoded for now.
                        _ratingHelper.InsertSellerRating(branchId, rating, curUserId, 1, 5);

                        var sellerBranchRating = _ratingHelper.GetSellerRating(branchId);
                        return Ok(sellerBranchRating);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [HttpGet("GetStoreProducts/{selectedCategory}/{selectedSubCategory}/{storeId}/{selectedBranchId}")]
        public IActionResult GetStoreProducts(int selectedCategory, int selectedSubCategory, int storeId, int selectedBranchId)
        {
            return GetStoreProducts(selectedCategory, selectedSubCategory, storeId, selectedBranchId, null);
        }

        [HttpGet("GetStoreProducts/{selectedCategory}/{selectedSubCategory}/{storeId}/{selectedBranchId}/{selectedBrand}")]
        public IActionResult GetStoreProducts(int selectedCategory, int selectedSubCategory, int storeId, int selectedBranchId, int? selectedBrand)
        {
            var productResult = _pricingRepository.GetStoreProducts(selectedCategory, selectedSubCategory, storeId, selectedBranchId, selectedBrand);
            return Ok(productResult);
        }

        // get all store for Hyperlocal

        [HttpGet("GetVbuyEnabledStores")]
        [HttpGet("GetVbuyEnabledStores/{lat}/{lng}/{mapRadius}")]
        public IActionResult GetVbuyEnabledStores(decimal lat, decimal lng, int mapRadius)
        {
            try
            {
                var query = _sellerBranchRepository.GetStoresWithinAreaQuery(0, lat, lng, mapRadius);
                var branchIdList = _efContext.Database.SqlQuery<LocationBoundaryResult>(query).Distinct().Select(x => x.BranchId).ToList<int>();

                var RetailerInfo = _context.Seller.Join(_context.SellerBranch,
                    x => x.StoreId,
                    y => y.Store,
                    (x, y) => new { x, y }).Where(x => x.y.FlagvBuy == true && branchIdList.Contains(x.y.BranchId)).Select(z => z.x).ToList();

                if (RetailerInfo.Count > 0)
                {
                    var RetailerInfoDetails = _sellerRepository.GetStoreDetailsForVbuy(RetailerInfo);
                    if (RetailerInfoDetails.Count > 0)
                    {
                        return Ok(RetailerInfoDetails);
                    }
                }
                return Ok(new List<RetailerInfoResult>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("GetProductsForCrm/{BranchId}")]
        public IActionResult GetProductsForCrm(int BranchId)
        {
            try
            {
                if (BranchId > 0)
                {
                    var productDetails = _context.ProductStoreMapping.Join(_context.Category,
                        pr => pr.Category,
                        ct => ct.CategoryId,
                        (pr, ct) => new { pr, ct }).Where(x => x.pr.IsDeleted == false && x.pr.Published == true && (x.pr.FlagSampleProducts == false || x.pr.FlagSampleProducts == null) && x.pr.BranchId == BranchId)
                        .Select(x => new
                        {
                            productID = x.pr.ProductId,
                            name = x.pr.Name,
                            category = _context.Category.Where(t => t.CategoryId == _context.Category.Where(z => z.CategoryId == x.ct.CategoryId).Select(z => z.ParentCategoryId).FirstOrDefault()).Select(x => x.Name).FirstOrDefault(),
                            subCategory = _context.Category.Where(y => y.CategoryId == x.ct.CategoryId).Select(x => x.Name).FirstOrDefault(),
                            categoryId = x.ct.CategoryId,
                            branchId = x.ct.BranchId,
                            branchName = _context.SellerBranch.Where(sb => sb.BranchId == x.pr.BranchId).Select(x => x.BranchName).FirstOrDefault(),
                            pictureName = (!string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == x.pr.ProductId).FirstOrDefault().PictureName) && !_context.ProductImage.Where(b => b.ProductId == x.pr.ProductId).FirstOrDefault().PictureName.Contains("http")) ? _appSettings.ImageUrlBase + _context.ProductImage.Where(b => b.ProductId == x.pr.ProductId).FirstOrDefault().PictureName : !string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == x.pr.ProductId).FirstOrDefault().PictureName) ? _context.ProductImage.Where(b => b.ProductId == x.pr.ProductId).FirstOrDefault().PictureName : "",
                        }).ToList();

                    return Ok(productDetails);
                }
                return Ok();
            }
            catch (Exception EX)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("GetVbuyCategory")]
        public IActionResult GetVbuyCategory()
        {
            try
            {
                var vbuyCategory = _context.Category.Where(x => x.MarketPlaceVbuyCategory == true && x.IsDeleted == false && x.ParentCategoryId == null).Select(x => new
                {
                    categoryName = x.Name,
                    categoryId = x.CategoryId,
                }).ToList();
                return Ok(vbuyCategory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("GetVbuySubCategory/{categoryId}")]
        public IActionResult GetVbuySubCategory(int categoryId)
        {
            try
            {
                var vbuySubCategory = _context.Category.Where(x => x.ParentCategoryId == categoryId).Select(x => new
                {
                    subCategoryName = x.Name,
                    categoryId = x.CategoryId,
                }).ToList();
                return Ok(vbuySubCategory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("MapCategory/{categoryId}/{ids}")]
        public IActionResult MapCategory(int categoryId, string ids)
        {
            try
            {
                List<int> productIds = ids.Split(',').Select(int.Parse).ToList();
                if (productIds.Count > 0)
                {
                    foreach (var id in productIds)
                    {
                        var product = _context.Product.Where(x => x.ProductId == id).FirstOrDefault();
                        if (product != null)
                        {
                            product.Category = categoryId;
                            product.UpdatedOnUtc = DateTime.UtcNow;
                            _context.Product.Update(product);
                            _context.SaveChanges();
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [HttpPost("AddVbuyCategory")]
        public IActionResult AddVbuyCategory(CategoryModelDTO categoryModel)
        {
            BaseUpdateResultSet result = new BaseUpdateResultSet();
            result.Status = Enums.UpdateStatus.Failure;
            try
            {
                var categoryExist = _context.Category.Where(x => x.Name == categoryModel.Name && x.ParentCategoryId == categoryModel.SelectedCategory && x.MarketPlaceVbuyCategory == true && (x.IsDeleted == false || x.IsDeleted == null)).ToList();
                if (categoryExist.Count > 0)
                {
                    result.Status = Enums.UpdateStatus.AlreadyExist;
                }
                else
                {
                    Category category = new Category();
                    category.Name = categoryModel.Name;
                    category.CategoryGroupTag = categoryModel.CategoryGroupTag;
                    category.ParentCategoryId = categoryModel.SelectedCategory;
                    category.FlagShowBuy = categoryModel.FlagShowBuy;
                    category.GroupDisplayOrder = categoryModel.GroupDisplayOrder;
                    category.Published = categoryModel.Published;
                    category.CreatedOnUtc = DateTime.UtcNow;
                    category.UpdatedOnUtc = DateTime.UtcNow;
                    category.PermaLink = categoryModel.Name.ToLower().Replace(" ", "-");
                    category.CreatedBy = "Admin";
                    category.IsDeleted = false;
                    category.MarketPlaceVbuyCategory = true;
                    _context.Category.Add(category);
                    _context.SaveChanges();
                    result.UpdatedId = category.CategoryId;
                    result.Status = Enums.UpdateStatus.Success;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
            return Ok(result);
        }

        [HttpGet("GetVbuyCategoryDetail/{categoryId}")]
        public IActionResult GetVbuyCategoryDetail(int categoryId)
        {
            try
            {
                var category = _context.Category.Where(x => x.CategoryId == categoryId).FirstOrDefault();
                if (category != null)
                {
                    CategoryResult categoryResult = new CategoryResult();
                    var categoeyDetails = _mapper.Map<Category, CategoryResult>(category, categoryResult);
                    if (categoeyDetails.CategoryImage != null)
                    {
                        if (!categoeyDetails.CategoryImage.Contains("http"))
                        {
                            categoeyDetails.CategoryImage = _appSettings.ImageUrlBase + categoeyDetails.CategoryImage;
                        }
                    }
                    return Ok(categoeyDetails);
                }
                return BadRequest("Category not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("UpdateVbuyCategory")]
        public IActionResult UpdateVbuyCategory(CategoryModelDTO categoryModel)
        {
            try
            {
                var categoryDetail = _context.Category.Where(x => x.Name == categoryModel.Name && x.CategoryId != categoryModel.CategoryId).FirstOrDefault();
                if (categoryDetail == null)
                {
                    var category = _context.Category.Where(x => x.CategoryId == categoryModel.CategoryId).FirstOrDefault();
                    if (category != null)
                    {
                        //update
                        category.Name = categoryModel.Name;
                        category.CategoryGroupTag = categoryModel.CategoryGroupTag;
                        category.GroupDisplayOrder = categoryModel.GroupDisplayOrder;
                        category.DisplayOrder = categoryModel.DisplayOrder;
                        category.Published = categoryModel.Published;
                        category.ShowOnHomePage = categoryModel.flagTopCategory;
                        category.FlagShowBuy = categoryModel.FlagShowBuy;
                        category.UpdatedOnUtc = DateTime.Now;
                        category.PermaLink = categoryModel.Name.ToLower().Replace(" ", "-");
                        _context.Category.Update(category);
                        _context.SaveChanges();
                        return Ok();
                    }
                    return BadRequest("Category not found");
                }
                return BadRequest("Already Exists");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpDelete("DeleteVbuyCategoryImage/{categoryId}")]
        public IActionResult DeleteVbuyCategoryImage(int categoryId)
        {
            try
            {
                if (categoryId > 0)
                {
                    var category = _context.Category.Where(x => x.CategoryId == categoryId).FirstOrDefault<Category>();
                    if (category != null)
                    {
                        category.CategoryImage = null;
                        category.UpdatedOnUtc = DateTime.UtcNow;
                        _context.Category.Update(category);
                        _context.SaveChanges();
                        return Ok(Enums.UpdateStatus.Success.ToString());
                    }
                }
                return Ok(Enums.UpdateStatus.Failure.ToString());
            }
            catch (Exception e)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("GetCategoryBasedProductDetailsForVbuy/{categoryId}")]
        public IActionResult GetCategoryBasedProductDetails(int categoryId)
        {
            try
            {
                var mainCategories = _context.Category.Where(x => x.ParentCategoryId == categoryId).Select(a => a.CategoryId).ToList();

                foreach (int eachCategory in mainCategories)
                {
                    var totalCount = _context.Product.Where(a => a.Category == eachCategory && a.IsDeleted == false).Count();
                    if (Convert.ToInt32(totalCount) > 0)
                    {
                        return Ok(true);
                    }
                }
            }
            catch (Exception Ex)
            {

            }
            return Ok(false);
        }

        [HttpGet("GetSubCategoryBasedProductDetailsForVbuy/{categoryId}")]
        public IActionResult GetSubCategoryBasedProductDetails(int categoryId)
        {
            try
            {
                var totalCount = _context.Product.Where(x => x.Category == categoryId && x.IsDeleted == false).Select(a => a.ProductId).Count();
                if (Convert.ToInt32(totalCount) > 0)
                {
                    return Ok(true);
                }
            }
            catch (Exception Ex)
            {

            }
            return Ok(false);
        }

        [HttpDelete("DeleteVbuyCategory/{categoryId}")]
        public IActionResult DeleteVbuyCategory(int categoryId)
        {
            try
            {
                if (categoryId > 0)
                {
                    var mainCategories = _context.Category.Where(x => x.ParentCategoryId == categoryId).Select(a => a.CategoryId).ToList();
                    if (mainCategories.Count > 0)
                    {
                        foreach (int eachCategory in mainCategories)
                        {
                            var subCategoryDetails = _context.Category.Where(a => a.CategoryId == eachCategory).FirstOrDefault();
                            if (subCategoryDetails != null)
                            {
                                subCategoryDetails.IsDeleted = true;
                                subCategoryDetails.ShowOnHomePage = false;
                                subCategoryDetails.UpdatedOnUtc = DateTime.UtcNow;
                                _context.Category.Update(subCategoryDetails);
                                _context.SaveChanges();
                            }
                        }
                    }
                    var categoryDetails = _context.Category.Where(x => x.CategoryId == categoryId).FirstOrDefault();
                    if (categoryDetails != null)
                    {
                        categoryDetails.IsDeleted = true;
                        categoryDetails.ShowOnHomePage = false;
                        categoryDetails.UpdatedOnUtc = DateTime.UtcNow;
                        _context.Category.Update(categoryDetails);
                        _context.SaveChanges();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("GetParentCategoryForVbuy")]
        public IActionResult GetParentCategoryForVbuy()
        {
            try
            {
                var categoryList = _context.Category.Where(a => a.IsDeleted != true && a.ParentCategoryId == null && a.MarketPlaceVbuyCategory == true).ToList();
                List<CategoryResult> catDToList = new List<CategoryResult>();
                var parentCategoryList = _mapper.Map<List<Category>, List<CategoryResult>>(categoryList, catDToList);
                foreach (var eachData in parentCategoryList)
                {
                    if (eachData.CategoryImage != null)
                    {
                        if (!eachData.CategoryImage.Contains("http"))
                        {
                            eachData.CategoryImage = _appSettings.ImageUrlBase + eachData.CategoryImage;
                        }
                    }
                }
                return Ok(parentCategoryList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("GetCategoryMenuForVbuy")]
        public List<CategoryWithChildren> GetCategoryMenuForVbuy()
        {
            var parentCategories = _context.Category.Where(x => x.MarketPlaceVbuyCategory == true && x.ParentCategoryId == null && x.IsDeleted != true && x.Published == true).OrderBy(x => x.DisplayOrder).ToList();

            List<CategoryWithChildren> categoryWithChildrens = new List<CategoryWithChildren>();

            foreach (Category parentCategory in parentCategories)
            {
                CategoryWithChildren menuResult = new CategoryWithChildren();
                menuResult.Id = parentCategory.CategoryId;
                menuResult.Name = parentCategory.Name;
                if (parentCategory.CategoryImage != null)
                {
                    if (!parentCategory.CategoryImage.Contains("http"))
                    {
                        menuResult.CategoryImage = _appSettings.ImageUrlBase + parentCategory.CategoryImage;
                    }
                    else
                    {
                        menuResult.CategoryImage = parentCategory.CategoryImage;
                    }
                }
                menuResult.PermaLink = parentCategory.PermaLink;

                var subCategories = _context.Category.Where(x => x.ParentCategoryId == parentCategory.CategoryId && x.IsDeleted != true && x.Published == true).OrderBy(z => z.GroupDisplayOrder).ThenBy(y => y.CategoryGroupTag).ThenBy(g => g.DisplayOrder);
                menuResult.Children = new List<MainCategory>();
                foreach (Category category in subCategories)
                {
                    MainCategory subMenuResult = new MainCategory();
                    subMenuResult.Id = category.CategoryId;
                    subMenuResult.Name = category.Name;
                    if (category.CategoryImage != null)
                    {
                        if (!category.CategoryImage.Contains("http"))
                        {
                            subMenuResult.CategoryImage = _appSettings.ImageUrlBase + category.CategoryImage;
                        }
                        else
                        {
                            subMenuResult.CategoryImage = category.CategoryImage;
                        }
                    }
                    subMenuResult.PermaLink = category.PermaLink;
                    menuResult.Children.Add(subMenuResult);
                }
                categoryWithChildrens.Add(menuResult);
            }
            return categoryWithChildrens;
        }
    }
}
