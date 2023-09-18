using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;
using PagedList;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using VSOnline.VSECommerce.Permission;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class CatalogController : VSControllerBase
    {
        private readonly ProductRepository _productRepository;
        private readonly PricingRepository _pricingRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly ProductHelper _productHelper;
        private readonly DataContext _context;
        private readonly EfContext _efContext;
        private readonly ManufacturerRepository _manufacturerRepository;
        private readonly IMapper _mapper;

        public CatalogController(IMapper mapper, ManufacturerRepository manufacturerRepository, EfContext efContext, ProductHelper productHelper, ProductRepository productRepository, PricingRepository pricingRepository, IOptions<AppSettings> _appSettings, DataContext context, CategoryRepository categoryRepository) : base(_appSettings)
        {
            _productRepository = productRepository;
            _pricingRepository = pricingRepository;
            _context = context;
            _categoryRepository = categoryRepository;
            _productHelper = productHelper;
            _efContext = efContext;
            _manufacturerRepository = manufacturerRepository;
            _mapper = mapper;
        }

        //product variant
        [Authorize(Policy = PolicyTypes.Product_Write)]
        [HttpPost("Seller/{BranchId}/CreateProductWithVariant")]
        public IActionResult CreateProductWithVariant(int BranchId, ProductDTOVariant newProductDTOVariant)
        {
            var userName = User.Identity.Name;
            var currentUser = User.FindFirst("UserId")?.Value;
            NewProductResult result = new NewProductResult();
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var newProductResultSet = _productRepository.CreateNewProductWithVariant(BranchId, newProductDTOVariant, userName);
                if (newProductResultSet != null && newProductResultSet.Status == Enums.UpdateStatus.Success)
                {
                    if (newProductDTOVariant.Options != null)
                    {
                        if (newProductDTOVariant.Options.Count > 0 && newProductResultSet.ProductId > 0)
                        {
                            foreach (var eachOption in newProductDTOVariant.Options)
                            {
                                if (eachOption != null)
                                {
                                    VariantOptions option = new VariantOptions();
                                    option.ProductId = newProductResultSet.ProductId;
                                    option.Options = eachOption;
                                    option.FlagDeleted = false;
                                    option.CreatedOnUtc = DateTime.UtcNow;
                                    _context.VariantOptions.Add(option);
                                    _context.SaveChanges();
                                }
                            }
                        }
                    }
                    if (newProductDTOVariant.Variants != null)
                    {
                        if (newProductDTOVariant.Variants.Count > 0 && newProductResultSet.ProductId > 0)
                        {
                            foreach (var eachVariant in newProductDTOVariant.Variants)
                            {
                                int i = 1;
                                if (eachVariant != null)
                                {
                                    ProductVariants variants = new ProductVariants();

                                    foreach (var eachCombination in eachVariant.Combination)
                                    {
                                        if (i == 1)
                                        {
                                            variants.Option1 = eachCombination;
                                        }
                                        else if (i == 2)
                                        {
                                            variants.Option2 = eachCombination;
                                        }
                                        else if (i == 3)
                                        {
                                            variants.Option3 = eachCombination;
                                        }
                                        i++;
                                    }
                                    if(!string.IsNullOrEmpty(variants.Option1) || !string.IsNullOrEmpty(variants.Option2) || !string.IsNullOrEmpty(variants.Option3))
                                    {
                                        variants.ProductId = newProductResultSet.ProductId;
                                        variants.FlagDeleted = false;
                                        variants.CreatedOnUtc = DateTime.UtcNow;
                                        _context.ProductVariants.Add(variants);
                                        _context.SaveChanges();
                                        var eachVariantId = variants.Id;
                                        //call pricing 
                                        RetailerAddProductDTOVariant retailerProduct = new RetailerAddProductDTOVariant();
                                        retailerProduct.ProductId = newProductResultSet.ProductId;
                                        retailerProduct.ProductVariantId = eachVariantId;
                                        retailerProduct.NewPrice = eachVariant.NewPriceVariant > 0 ? eachVariant.NewPriceVariant : newProductDTOVariant.NewPrice;
                                        retailerProduct.NewSpecialPrice = (decimal)(eachVariant.NewSpecialPriceVariant > 0 ? eachVariant.NewSpecialPriceVariant : newProductDTOVariant.NewSpecialPrice);
                                        retailerProduct.NewSpecialPriceDescription = newProductDTOVariant.NewSpecialPriceDescription;
                                        retailerProduct.NewAdditionalTax = newProductDTOVariant.NewAdditionalTax;
                                        retailerProduct.NewPriceStartTime = newProductDTOVariant.NewPriceStartTime;
                                        retailerProduct.NewPriceEndTime = newProductDTOVariant.NewPriceEndTime;
                                        retailerProduct.StoreId = newProductDTOVariant.StoreId;
                                        IncludeProductsPricingVariant(retailerProduct);
                                    }
                                }
                            }
                        }
                        else
                        {
                            RetailerAddProductDTO retailerProduct = new RetailerAddProductDTO();
                            retailerProduct.ProductId = newProductResultSet.ProductId;
                            retailerProduct.NewPrice = newProductDTOVariant.NewPrice;
                            retailerProduct.NewSpecialPrice = (decimal)newProductDTOVariant.NewSpecialPrice;
                            retailerProduct.NewSpecialPriceDescription = newProductDTOVariant.NewSpecialPriceDescription;
                            retailerProduct.NewAdditionalTax = newProductDTOVariant.NewAdditionalTax;
                            retailerProduct.NewPriceStartTime = newProductDTOVariant.NewPriceStartTime;
                            retailerProduct.NewPriceEndTime = newProductDTOVariant.NewPriceEndTime;
                            retailerProduct.StoreId = newProductDTOVariant.StoreId;
                            IncludeProducts(retailerProduct);
                        }
                    }
                    
                }

                newProductResultSet.NewProduct = null; //make it as null for product variant due to repeat of productid in pricing table
                result = newProductResultSet;
            }
            catch (Exception ex)
            {
                result.Status = Enums.UpdateStatus.Error;
            }
            return Ok(result);
        }

        [HttpPost("IncludeProductsPricingVariant")]
        public string IncludeProductsPricingVariant(RetailerAddProductDTOVariant retailerProduct)
        {
            var currentUser = User.FindFirst("UserId")?.Value;
            Enums.UpdateStatus status = Enums.UpdateStatus.Failure;
            if (retailerProduct.BranchIdList == null || retailerProduct.BranchIdList.Count <= 0)
            {
                retailerProduct.BranchIdList = GetBranchIdListFromStore(retailerProduct.StoreId);
            }
            try
            {
                foreach (var branch in retailerProduct.BranchIdList)
                {
                    status = _pricingRepository.IncludeProductForVariant(retailerProduct, branch, currentUser);
                }
                _context.SaveChanges();
            }
            catch
            {
                status = Enums.UpdateStatus.Error;
            }
            return status.ToString();
        }

        [Authorize(Policy = PolicyTypes.Product_Write)]
        [HttpPost("Seller/{BranchId}/UpdateProductWithVariant")]
        public IActionResult UpdateProductWithVariant(int BranchId, ProductDTOVariant newProductDTOVariant)
        {
            Enums.UpdateStatus status = Enums.UpdateStatus.Failure;
            var currentUser = User.Identity.Name;
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var updateProduct = _productRepository.UpdateProductForVariant(BranchId, newProductDTOVariant,currentUser);

                if (updateProduct != null)
                {
                    var categoryDetails = _context.Category.Where(x => x.CategoryId == newProductDTOVariant.Category).FirstOrDefault();
                    if (categoryDetails != null)
                    {
                        categoryDetails.FlagSampleCategory = false;
                        _context.Category.Update(categoryDetails);
                        _context.SaveChanges();
                        var parentCategoryDetails = _context.Category.Where(x => x.ParentCategoryId == categoryDetails.ParentCategoryId).FirstOrDefault();
                        if (parentCategoryDetails != null)
                        {
                            parentCategoryDetails.FlagSampleCategory = false;
                            _context.Category.Update(parentCategoryDetails);
                            _context.SaveChanges();
                        }
                    }
                    if (newProductDTOVariant.Options.Count > 0 && updateProduct.ProductId > 0)
                    {
                        var variantOption = _context.VariantOptions.Where(x => x.ProductId == updateProduct.ProductId).ToList();
                        foreach (var eachOption in variantOption)
                        {
                            if (eachOption != null)
                            {
                                eachOption.FlagDeleted = true;
                                eachOption.UpdatedOnUtc = DateTime.UtcNow;
                                _context.VariantOptions.Update(eachOption);
                                _context.SaveChanges();
                            }
                        }
                        if (newProductDTOVariant.Options.Count > 0 && updateProduct.ProductId > 0)
                        {
                            foreach (var eachOption in newProductDTOVariant.Options)
                            {
                                if (eachOption != null)
                                {
                                    VariantOptions option = new VariantOptions();
                                    option.ProductId = updateProduct.ProductId;
                                    option.Options = eachOption;
                                    option.FlagDeleted = false;
                                    option.CreatedOnUtc = DateTime.UtcNow;
                                    _context.VariantOptions.Add(option);
                                    _context.SaveChanges();
                                }
                            }
                        }
                    }
                    if (newProductDTOVariant.Variants.Count > 0 && updateProduct.ProductId > 0)
                    {
                        var productVariant = _context.ProductVariants.Where(x => x.ProductId == updateProduct.ProductId).ToList();
                        foreach (var eachVariant in productVariant)
                        {
                            if (eachVariant != null)
                            {
                                eachVariant.FlagDeleted = true;
                                eachVariant.UpdatedOnUtc = DateTime.UtcNow;
                                _context.ProductVariants.Update(eachVariant);
                                _context.SaveChanges();
                            }
                        }
                        var pricingDetail = _context.Pricing.Where(x => x.Product == updateProduct.ProductId && x.ProductVariantId != null).ToList();
                        foreach (var eachPrice in pricingDetail)
                        {
                            if (eachPrice != null)
                            {
                                eachPrice.IsDeleted = true;
                                eachPrice.UpdatedOnUtc = DateTime.UtcNow;
                                _context.Pricing.Update(eachPrice);
                                _context.SaveChanges();
                            }
                        }
                        foreach (var eachVariant in newProductDTOVariant.Variants)
                        {
                            int i = 1;
                            if (eachVariant != null)
                            {
                                ProductVariants variants = new ProductVariants();

                                foreach (var eachCombination in eachVariant.Combination)
                                {
                                    if (i == 1)
                                    {
                                        variants.Option1 = eachCombination;
                                    }
                                    else if (i == 2)
                                    {
                                        variants.Option2 = eachCombination;
                                    }
                                    else if (i == 3)
                                    {
                                        variants.Option3 = eachCombination;
                                    }
                                    i++;
                                }
                                variants.ProductId = updateProduct.ProductId;
                                variants.FlagDeleted = false;
                                variants.CreatedOnUtc = DateTime.UtcNow;
                                _context.ProductVariants.Add(variants);
                                _context.SaveChanges();
                                var eachVariantId = variants.Id;
                                //call pricing 
                                RetailerAddProductDTOVariant retailerProduct = new RetailerAddProductDTOVariant();
                                retailerProduct.ProductId = updateProduct.ProductId;
                                retailerProduct.ProductVariantId = eachVariantId;
                                retailerProduct.NewPrice = eachVariant.NewPriceVariant > 0 ? eachVariant.NewPriceVariant : newProductDTOVariant.NewPrice;
                                retailerProduct.NewSpecialPrice = (decimal)(eachVariant.NewSpecialPriceVariant > 0 ? eachVariant.NewSpecialPriceVariant : newProductDTOVariant.NewSpecialPrice);
                                retailerProduct.NewSpecialPriceDescription = newProductDTOVariant.NewSpecialPriceDescription;
                                retailerProduct.NewAdditionalTax = newProductDTOVariant.NewAdditionalTax;
                                retailerProduct.NewPriceStartTime = newProductDTOVariant.NewPriceStartTime;
                                retailerProduct.NewPriceEndTime = newProductDTOVariant.NewPriceEndTime;
                                retailerProduct.StoreId = newProductDTOVariant.StoreId;
                                IncludeProductsPricingVariant(retailerProduct);
                            }
                        }
                    }
                    else
                    {
                        RetailerUpdateProductDTO retailerProduct = new RetailerUpdateProductDTO();
                        retailerProduct.ProductId = updateProduct.ProductId;
                        retailerProduct.NewPrice = newProductDTOVariant.NewPrice;
                        retailerProduct.NewSpecialPrice = (decimal)newProductDTOVariant.NewSpecialPrice;
                        retailerProduct.NewSpecialPriceDescription = newProductDTOVariant.NewSpecialPriceDescription;
                        retailerProduct.NewAdditionalTax = newProductDTOVariant.NewAdditionalTax;
                        retailerProduct.NewPriceStartTime = newProductDTOVariant.NewPriceStartTime;
                        retailerProduct.NewPriceEndTime = newProductDTOVariant.NewPriceEndTime;
                        retailerProduct.StoreId = newProductDTOVariant.StoreId;
                        retailerProduct.NewDeliveryTime = newProductDTOVariant.NewDeliveryTime;
                        UpdateProductPrice(retailerProduct);
                    }
                }
                status = Enums.UpdateStatus.Success;
            }
            catch (Exception ex)
            {
                status = Enums.UpdateStatus.Error;
            }
            return Ok(status.ToString());
        }

        [Authorize(Policy = PolicyTypes.Product_Read)]
        [HttpGet("Seller/{BranchId}/GetProductVariant/{id}")]
        public IActionResult GetProductVariantById(int BranchId, int id)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
           
                var productDetails = _context.ProductStoreMapping.Where(x => x.ProductId == id && x.BranchId == BranchId).Include(x => x.CategoryDetails).First<ProductStoreMapping>();
                var product = _mapper.Map<ProductStoreMapping, ProductResult>(productDetails);
                if (product != null && product.ProductId > 0)
                {
                    product.ProductImages = new List<ProductImageDetails>();
                    var productImages = _context.ProductImage.Where(x => x.ProductId == id).ToList();
                    foreach (var images in productImages)
                    {
                        ProductImageDetails imageDetails = new ProductImageDetails();
                        imageDetails.Id = images.Id;
                        if (images.PictureName != null)
                        {
                            if (!images.PictureName.Contains("http"))
                            {
                                imageDetails.PictureName = _appSettings.ImageUrlBase + images.PictureName;
                            }
                            else
                            {
                                imageDetails.PictureName = images.PictureName;
                            }
                        }
                        product.ProductImages.Add(imageDetails);
                    }
                    product.VariantsPricing = new List<VariantsPricing>();
                    product.VariantOptions = _context.VariantOptions.Where(x => x.ProductId == id && x.FlagDeleted != true).Select(x => x.Options).ToList();
                    var VariantsPricing = _context.Pricing.Where(x => x.Product == id && x.Branch == BranchId && (x.IsDeleted == false || x.IsDeleted == null)).ToList();
                    foreach (var eachProductPrice in VariantsPricing)
                    {

                        VariantsPricing pricing = new VariantsPricing();
                        pricing.PricingId = eachProductPrice.PricingId;
                        pricing.ProductVariantId = eachProductPrice.ProductVariantId;
                        pricing.Price = eachProductPrice.Price;
                        pricing.SpecialPrice = eachProductPrice.SpecialPrice;
                        pricing.SpecialPriceDescription = eachProductPrice.SpecialPriceDescription;
                        pricing.Store = eachProductPrice.Store;
                        var productVariant = _context.ProductVariants.Where(x => x.Id == eachProductPrice.ProductVariantId && x.FlagDeleted != true)
                            .Select(x => new options { option1 = x.Option1, option2 = x.Option2, option3 = x.Option3 }).FirstOrDefault();
                        pricing.ProductVariant = productVariant;
                        product.VariantsPricing.Add(pricing);
                    }

                    var productVariantsOptions1 = _context.ProductVariants.Where(x => x.ProductId == id && x.FlagDeleted != true).Select(x => x.Option1).ToList();
                    var option1 = productVariantsOptions1.Distinct().ToList();
                    var productVariantsOptions2 = _context.ProductVariants.Where(x => x.ProductId == id && x.FlagDeleted != true).Select(x => x.Option2).ToList();
                    var option2 = productVariantsOptions2.Distinct().ToList();

                    var productVariantsOptions3 = _context.ProductVariants.Where(x => x.ProductId == id && x.FlagDeleted != true).Select(x => x.Option3).ToList();
                    var option3 = productVariantsOptions3.Distinct().ToList();

                    product.option1 = option1;
                    product.option2 = option2;
                    product.option3 = option3;
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Write)]
        [HttpPost("IncludeProducts")]
        public string IncludeProducts(RetailerAddProductDTO retailerProduct)
        {
            var currentUser = User.FindFirst("UserId")?.Value;
            Enums.UpdateStatus status = Enums.UpdateStatus.Failure;
            if (retailerProduct.BranchIdList == null || retailerProduct.BranchIdList.Count <= 0)
            {
                retailerProduct.BranchIdList = GetBranchIdListFromStore(retailerProduct.StoreId);
            }
            try
            {
                foreach (var branch in retailerProduct.BranchIdList)
                {
                    status = _pricingRepository.IncludeProduct(retailerProduct, branch, currentUser);
                }
                _context.SaveChanges();
            }
            catch
            {
                status = Enums.UpdateStatus.Error;
            }
            return status.ToString();
        }

        [Authorize(Policy = PolicyTypes.Product_Write)]
        [HttpPut("UpdateProductPrice")]
        public bool UpdateProductPrice(RetailerUpdateProductDTO retailerProduct)
        {
            try
            {
                var currentUser = User.FindFirst("UserId")?.Value;
                if (retailerProduct.BranchIdList == null || retailerProduct.BranchIdList.Count <= 0)
                {
                    retailerProduct.BranchIdList = GetBranchIdListFromStore(retailerProduct.StoreId);
                }
                foreach (var branch in retailerProduct.BranchIdList)
                {
                    var pricing = _context.Pricing.Where(x => x.Store == retailerProduct.StoreId && x.Branch == branch && x.Product == retailerProduct.ProductId).FirstOrDefault<Pricing>();
                    _pricingRepository.UpdatePricing(retailerProduct, pricing, currentUser, branch);
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        [Authorize(Policy = PolicyTypes.Product_Read)]
        [HttpGet("Seller/{BranchId}/GetMyFilteredProductsPaging/{selectedCategory}/{selectedSubCategory}/{StoreId}/{pageNo}/{PageSize}")]
        public IActionResult GetMyFilteredProductsPaging(int BranchId, int selectedCategory, int selectedSubCategory, int storeId, int pageNo, int PageSize)
        {
            var branchIds = User.FindAll("BranchId").ToList();
            if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
            {
                return Unauthorized();
            }
            return GetMyFilteredProductsPaging(BranchId, selectedCategory, selectedSubCategory, storeId, null, pageNo, PageSize);
        }

        [Authorize(Policy = PolicyTypes.Product_Read)]
        [HttpGet("Seller/{BranchId}/GetMyFilteredProductsPaging/{selectedCategory}/{selectedSubCategory}/{StoreId}/{selectedBrand}/{pageNo}/{PageSize}")]
        public IActionResult GetMyFilteredProductsPaging(int BranchId, int selectedCategory, int selectedSubCategory, int StoreId, int? selectedBrand, int pageNo, int PageSize)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var FilteredProductsPagingData = _pricingRepository.GetMyFilteredProductsPaging(BranchId, selectedCategory, selectedSubCategory, StoreId, selectedBrand, pageNo, PageSize);
                return Ok(FilteredProductsPagingData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        private List<int> GetBranchIdListFromStore(int storeId)
        {
            var branchIdList = new List<int>();
            var seller = _context.Seller.Where(x => x.StoreId == storeId).Include(y => y.Branches).FirstOrDefault();
            foreach (SellerBranch branch in seller.Branches)
            {
                branchIdList.Add(branch.BranchId);
            }
            return branchIdList;
        }

        [Authorize(Policy = PolicyTypes.Product_Write)]
        [HttpPut("Seller/{BranchId}/UpdatePublishStatus/{publishIds}/{publishStatus}")]
        public IActionResult UpdatePublishStatus(int BranchId, int publishIds, bool publishStatus)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var productDetail = _context.ProductStoreMapping.Where(x => x.ProductId == publishIds && x.BranchId == BranchId).FirstOrDefault();
                if (productDetail != null)
                {
                    productDetail.Published = publishStatus;
                    productDetail.UpdatedOnUtc = DateTime.UtcNow;
                    _context.ProductStoreMapping.Update(productDetail);
                    _context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Write)]
        [HttpPut("Seller/{BranchId}/UpdateFeatureStatus/{publishIds}/{featureStatus}")]
        public IActionResult UpdateFeatureStatus(int BranchId, int publishIds, bool featureStatus)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var productDetail = _context.ProductStoreMapping.Where(x => x.ProductId == publishIds && x.BranchId == BranchId).FirstOrDefault();
                if (productDetail != null)
                {
                    productDetail.ShowOnHomePage = featureStatus;
                    productDetail.UpdatedOnUtc = DateTime.UtcNow;
                    _context.ProductStoreMapping.Update(productDetail);
                    _context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Read)]
        [HttpGet("Seller/{BranchId}/GetRetailerProductFilter")]
        public IActionResult GetRetailerProductFilter(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var retailerProductFilterResult = new RetailerProductFilterResult();
                var currentUser = User.FindFirst("UserId")?.Value;
                var categoryQuery = _productHelper.GetAllCategory(currentUser);
                var parentCategories = _efContext.Database.SqlQuery<CategoryFilterDTO>(categoryQuery).ToList();
                if (parentCategories.Count == 0)
                {
                    retailerProductFilterResult = _categoryRepository.GetRetailerProductFilterResult(retailerProductFilterResult);
                }
                else
                {
                    var subCategoryQuery = _productHelper.GetAllSubCategory(currentUser);
                    var subCategoryFilter = _efContext.Database.SqlQuery<SubCategoryFilterDTO>(subCategoryQuery).ToList();
                    retailerProductFilterResult.CategoryFilter = parentCategories.Select(cf => new CategoryFilterDTO { CategoryId = cf.CategoryId, Name = cf.Name }).ToList<CategoryFilterDTO>();
                    retailerProductFilterResult.SubCategoryFilter = subCategoryFilter;
                }
                retailerProductFilterResult.Brands = _manufacturerRepository.GetBrands(BranchId);
                return Ok(retailerProductFilterResult);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Delete)]
        [HttpDelete("Seller/{BranchId}/Products/{id}")]
        public IActionResult DeleteProductById(int BranchId, int id)
        {
            Enums.UpdateStatus status = Enums.UpdateStatus.Failure;
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (id > 0)
                {
                    var deleteProduct = _productRepository.DeleteProductInProduct(id, BranchId);
                    if (deleteProduct != null)
                    {
                        var deleteProductinPricing = _pricingRepository.DeleteProductInPricing(id, BranchId);
                        status = Enums.UpdateStatus.Success;
                    }
                }
                else
                {
                    status = Enums.UpdateStatus.Failure;
                }
            }
            catch
            {
                status = Enums.UpdateStatus.Error;
            }
            return Ok(status.ToString());
        }

        [Authorize(Policy = PolicyTypes.Product_Delete)]
        [HttpDelete("Seller/{BranchId}/DeleteProductImage/{productId}/{id}")]
        public IActionResult DeleteProductImage(int BranchId, int productId, int id)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var deleteProductImage = 0;
                var productImage = _context.ProductImage.Where(x => x.Id == id && x.ProductId == productId).FirstOrDefault();
                if (productImage != null)
                {
                    _context.Remove(productImage);
                    deleteProductImage = _context.SaveChanges();
                }
                if (Convert.ToInt32(deleteProductImage) > 0)
                {
                    return Ok(Enums.UpdateStatus.Success.ToString());
                }
                else
                {
                    return Ok(Enums.UpdateStatus.Failure.ToString());
                }
            }
            catch (Exception ex)
            {
                return Ok(Enums.UpdateStatus.Failure.ToString());
            }
        }

        //Analytics
        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/RecentlyAddedProduct")]
        public IActionResult RecentlyAddedProduct(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var productDetails = _context.ProductStoreMapping.Where(x=>x.IsDeleted == false && x.BranchId == BranchId)
                    .Select(a => new
                    {
                        Price = _context.Pricing.Where(x=>x.Product == a.ProductId && x.IsDeleted == false && x.Branch == BranchId).Select(x=>x.Price).FirstOrDefault(),
                        SpecialPrice = _context.Pricing.Where(x => x.Product == a.ProductId && x.IsDeleted == false && x.Branch == BranchId).Select(x => x.SpecialPrice).FirstOrDefault(),
                        a.ProductId,
                        a.Name,
                        a.FullDescription,
                        a.Published,
                        a.IsDeleted,
                        a.ShowOnHomePage,
                        PictureName = (!string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.ProductId).FirstOrDefault().PictureName) && !_context.ProductImage.Where(b => b.ProductId == a.ProductId).FirstOrDefault().PictureName.Contains("http")) ? _appSettings.ImageUrlBase + _context.ProductImage.Where(b => b.ProductId == a.ProductId).FirstOrDefault().PictureName : !string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.ProductId).FirstOrDefault().PictureName) ? _context.ProductImage.Where(b => b.ProductId == a.ProductId).FirstOrDefault().PictureName : "",
                        a.PermaLink
                    }).ToList().OrderByDescending(a => a.ProductId).Take(5);
                return Ok(productDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/RecentlySoledProduct")]
        public IActionResult RecentlySoledProduct(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var orderIdPrefix = "";
                var branchDetails = _context.SellerBranch.Where(x => x.BranchId == BranchId).FirstOrDefault();
                if (branchDetails != null)
                {
                    orderIdPrefix = branchDetails.OrderIdPrefix == null ? branchDetails.BranchName : branchDetails.OrderIdPrefix;
                }
                var orderProduct = _context.OrderProduct.Where(x => x.BranchId == BranchId).OrderByDescending(x => x.OrderDateUtc).Select(x => new
                {
                    Id = x.Id,
                    OrderTotal = x.OrderTotal,
                    BranchOrderIdWithPrefix = orderIdPrefix.ToUpper() + "-" + x.BranchOrderId
                }).Take(5);

                return Ok(orderProduct);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Read)]
        [HttpGet("Seller/{BranchId}/PublishedProduct/{PageSize}")]
        public IActionResult PublishedProduct(int BranchId, int PageSize)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var productDetails = _context.ProductStoreMapping
                    .GroupJoin(
                    _context.NewInventory,
                    product => product.ProductId,
                    inventory => inventory.ProductId,
                    (product, inventory) => new { product, inventory })
                    .SelectMany(
                    a => a.inventory.DefaultIfEmpty(),
                    (newestProducts, newestInventory) => new
                    {
                        NewestProdcuts = newestProducts.product,
                        NewestInventory = newestInventory
                    })
                    .Where(a => a.NewestProdcuts.IsDeleted == false && a.NewestProdcuts.Published && a.NewestProdcuts.BranchId == BranchId)
                    .Select(a => new
                    {
                        Price = _context.Pricing.Where(b => b.Product == a.NewestProdcuts.ProductId && b.Branch == BranchId &&  (b.IsDeleted == false || b.IsDeleted == null)).FirstOrDefault().Price,
                        SpecialPrice = _context.Pricing.Where(b => b.Product == a.NewestProdcuts.ProductId && b.Branch == BranchId  && (b.IsDeleted == false || b.IsDeleted == null)).FirstOrDefault().SpecialPrice,
                        a.NewestProdcuts.ProductId,
                        a.NewestProdcuts.Name,
                        a.NewestProdcuts.FullDescription,
                        a.NewestProdcuts.Published,
                        a.NewestProdcuts.IsDeleted,
                        AvailableQuantity = (a.NewestInventory != null) ? (int?)a.NewestInventory.AvailableQuantity : null,
                        FlagTrackQuantity = (a.NewestInventory != null) ? (bool?)a.NewestInventory.FlagTrackQuantity : null,
                        a.NewestProdcuts.ShowOnHomePage,
                        PictureName = (!string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName) && !_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName.Contains("http")) ? _appSettings.ImageUrlBase + _context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName : !string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName) ? _context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName : "",
                        a.NewestProdcuts.PermaLink,
                        a.NewestProdcuts.FlagSampleProducts
                    }).ToList().OrderBy(a => a.ProductId).Take(PageSize);

                return Ok(productDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Read)]
        [HttpGet("Seller/{BranchId}/UnPublishedProduct/{PageSize}")]
        public IActionResult UnPublishedProduct(int BranchId, int PageSize)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var productDetails = _context.ProductStoreMapping
                    .GroupJoin(
                    _context.NewInventory,
                    product => product.ProductId,
                    inventory => inventory.ProductId,
                    (product, inventory) => new { product, inventory })
                    .SelectMany(
                    a => a.inventory.DefaultIfEmpty(),
                    (newestProducts, newestInventory) => new
                    {
                        NewestProdcuts = newestProducts.product,
                        NewestInventory = newestInventory
                    })
                    .Where(a => a.NewestProdcuts.IsDeleted == false && !a.NewestProdcuts.Published && a.NewestProdcuts.BranchId == BranchId)
                    .Select(a => new
                    {
                        Price = _context.Pricing.Where(b => b.Product == a.NewestProdcuts.ProductId && b.Branch == BranchId && (b.IsDeleted == false || b.IsDeleted == null)).FirstOrDefault().Price,
                        SpecialPrice = _context.Pricing.Where(b => b.Product == a.NewestProdcuts.ProductId && b.Branch == BranchId && (b.IsDeleted == false || b.IsDeleted == null)).FirstOrDefault().SpecialPrice,
                        a.NewestProdcuts.ProductId,
                        a.NewestProdcuts.Name,
                        a.NewestProdcuts.FullDescription,
                        a.NewestProdcuts.Published,
                        a.NewestProdcuts.IsDeleted,
                        AvailableQuantity = (a.NewestInventory != null) ? (int?)a.NewestInventory.AvailableQuantity : null,
                        FlagTrackQuantity = (a.NewestInventory != null) ? (bool?)a.NewestInventory.FlagTrackQuantity : null,
                        a.NewestProdcuts.ShowOnHomePage,
                        PictureName = (!string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName) && !_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName.Contains("http")) ? _appSettings.ImageUrlBase + _context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName : !string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName) ? _context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName : "",
                        a.NewestProdcuts.PermaLink,
                        a.NewestProdcuts.FlagSampleProducts
                    }).ToList().OrderBy(a => a.ProductId).Take(PageSize);
                return Ok(productDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Read)]
        [HttpGet("Seller/{BranchId}/SearchProduct/{searchProName}/{publishstatus}")]
        public IActionResult SearchProduct(int BranchId, string searchProName, int publishstatus)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var productDetails = new List<SearchProductResult>();
                if ((searchProName != null) && (publishstatus == 1))
                {
                    productDetails = _context.ProductStoreMapping
                            .GroupJoin(
                            _context.NewInventory,
                            product => product.ProductId,
                            inventory => inventory.ProductId,
                            (product, inventory) => new { product, inventory })
                            .SelectMany(
                            a => a.inventory.DefaultIfEmpty(),
                            (newestProducts, newestInventory) => new
                            {
                                NewestProdcuts = newestProducts.product,
                                NewestInventory = newestInventory
                            })
                            .Where(a => a.NewestProdcuts.Name.Contains(searchProName) && a.NewestProdcuts.IsDeleted == false && a.NewestProdcuts.Published == true && a.NewestProdcuts.BranchId == BranchId)
                            .Select(a => new SearchProductResult
                            {
                                Price = (decimal)_context.Pricing.Where(b => b.Product == a.NewestProdcuts.ProductId && b.Branch == BranchId && (b.IsDeleted == false || b.IsDeleted == null)).FirstOrDefault().Price,
                                SpecialPrice = (decimal)_context.Pricing.Where(b => b.Product == a.NewestProdcuts.ProductId && b.Branch == BranchId  && (b.IsDeleted == false || b.IsDeleted == null)).FirstOrDefault().SpecialPrice,
                                ProductId = a.NewestProdcuts.ProductId,
                                Name = a.NewestProdcuts.Name,
                                FullDescription = a.NewestProdcuts.FullDescription,
                                Published = a.NewestProdcuts.Published,
                                IsDeleted = (bool)a.NewestProdcuts.IsDeleted,
                                AvailableQuantity = (a.NewestInventory != null) ? (int?)a.NewestInventory.AvailableQuantity : null,
                                FlagTrackQuantity = (a.NewestInventory != null) ? (bool?)a.NewestInventory.FlagTrackQuantity : null,
                                ShowOnHomePage = a.NewestProdcuts.ShowOnHomePage,
                                PictureName = (!string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName) && !_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName.Contains("http")) ? _appSettings.ImageUrlBase + _context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName : !string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName) ? _context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName : "",
                                PermaLink = a.NewestProdcuts.PermaLink,
                                BrandName = _context.Manufacturer.Where(X => X.ManufacturerId == a.NewestProdcuts.Manufacturer).Select(x => x.Name).FirstOrDefault(),
                                ManufacturerId = (int)a.NewestProdcuts.Manufacturer,
                            }).ToList();

                }
                if ((searchProName != null) && (publishstatus == 0))
                {
                    productDetails = _context.ProductStoreMapping
                          .GroupJoin(
                          _context.NewInventory,
                          product => product.ProductId,
                          inventory => inventory.ProductId,
                          (product, inventory) => new { product, inventory })
                          .SelectMany(
                          a => a.inventory.DefaultIfEmpty(),
                          (newestProducts, newestInventory) => new
                          {
                              NewestProdcuts = newestProducts.product,
                              NewestInventory = newestInventory
                          })
                          .Where(a => a.NewestProdcuts.Name.Contains(searchProName) && a.NewestProdcuts.IsDeleted == false && a.NewestProdcuts.Published == false && a.NewestProdcuts.BranchId == BranchId)
                          .Select(a => new SearchProductResult
                          {
                              Price = (decimal)_context.Pricing.Where(b => b.Product == a.NewestProdcuts.ProductId && b.Branch == BranchId  && (b.IsDeleted == false || b.IsDeleted == null)).FirstOrDefault().Price,
                              SpecialPrice = (decimal)_context.Pricing.Where(b => b.Product == a.NewestProdcuts.ProductId && b.Branch == BranchId && (b.IsDeleted == false || b.IsDeleted == null)).FirstOrDefault().SpecialPrice,
                              ProductId = a.NewestProdcuts.ProductId,
                              Name = a.NewestProdcuts.Name,
                              FullDescription = a.NewestProdcuts.FullDescription,
                              Published = a.NewestProdcuts.Published,
                              IsDeleted = (bool)a.NewestProdcuts.IsDeleted,
                              AvailableQuantity = (a.NewestInventory != null) ? (int?)a.NewestInventory.AvailableQuantity : null,
                              FlagTrackQuantity = (a.NewestInventory != null) ? (bool?)a.NewestInventory.FlagTrackQuantity : null,
                              ShowOnHomePage = a.NewestProdcuts.ShowOnHomePage,
                              PictureName = (!string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName) && !_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName.Contains("http")) ? _appSettings.ImageUrlBase + _context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName : !string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName) ? _context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName : "",
                              PermaLink = a.NewestProdcuts.PermaLink,
                              BrandName = _context.Manufacturer.Where(X => X.ManufacturerId == a.NewestProdcuts.Manufacturer).Select(x => x.Name).FirstOrDefault(),
                              ManufacturerId = (int)a.NewestProdcuts.Manufacturer,
                          }).ToList();
                }
                if ((searchProName != null) && (publishstatus == 2))
                {
                    productDetails = _context.ProductStoreMapping
                          .GroupJoin(
                          _context.NewInventory,
                          product => product.ProductId,
                          inventory => inventory.ProductId,
                          (product, inventory) => new { product, inventory })
                          .SelectMany(
                          a => a.inventory.DefaultIfEmpty(),
                          (newestProducts, newestInventory) => new
                          {
                              NewestProdcuts = newestProducts.product,
                              NewestInventory = newestInventory
                          })
                          .Where(a => a.NewestProdcuts.Name.Contains(searchProName) && a.NewestProdcuts.IsDeleted == false && a.NewestProdcuts.BranchId == BranchId)
                          .Select(a => new SearchProductResult
                          {
                              Price = (decimal)_context.Pricing.Where(b => b.Product == a.NewestProdcuts.ProductId && b.Branch == BranchId && (b.IsDeleted == false || b.IsDeleted == null)).FirstOrDefault().Price,
                              SpecialPrice = (decimal)_context.Pricing.Where(b => b.Product == a.NewestProdcuts.ProductId && b.Branch == BranchId  && (b.IsDeleted == false || b.IsDeleted == null)).FirstOrDefault().SpecialPrice,
                              ProductId = a.NewestProdcuts.ProductId,
                              Name = a.NewestProdcuts.Name,
                              FullDescription = a.NewestProdcuts.FullDescription,
                              Published = a.NewestProdcuts.Published,
                              IsDeleted = (bool)a.NewestProdcuts.IsDeleted,
                              AvailableQuantity = (a.NewestInventory != null) ? (int?)a.NewestInventory.AvailableQuantity : null,
                              FlagTrackQuantity = (a.NewestInventory != null) ? (bool?)a.NewestInventory.FlagTrackQuantity : null,
                              ShowOnHomePage = a.NewestProdcuts.ShowOnHomePage,
                              PictureName = (!string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName) && !_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName.Contains("http")) ? _appSettings.ImageUrlBase + _context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName : !string.IsNullOrEmpty(_context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName) ? _context.ProductImage.Where(b => b.ProductId == a.NewestProdcuts.ProductId).FirstOrDefault().PictureName : "",
                              PermaLink = a.NewestProdcuts.PermaLink,
                              BrandName = _context.Manufacturer.Where(X => X.ManufacturerId == a.NewestProdcuts.Manufacturer).Select(x => x.Name).FirstOrDefault(),
                              ManufacturerId = (int)a.NewestProdcuts.Manufacturer,
                          }).ToList();
                }
                return Ok(productDetails);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        //category controller

        [Authorize(Policy = PolicyTypes.Category_Write)]
        [HttpPost("Seller/{BranchId}/Category")]
        public IActionResult AddNewCategory(int BranchId, CategoryModelDTO categoryModel)
        {
            BaseUpdateResultSet result = new BaseUpdateResultSet();
            result.Status = Enums.UpdateStatus.Failure;
            try
            {
                var currentUser = User.Identity.Name;
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }

                if (categoryModel.CategoryId > 0)
                {
                    var category = _context.Category.Where(x => x.CategoryId == categoryModel.CategoryId).FirstOrDefault();
                    if (category != null)
                    {
                        category.Name = categoryModel.Name;
                        category.CategoryGroupTag = categoryModel.CategoryGroupTag;
                        category.GroupDisplayOrder = categoryModel.GroupDisplayOrder;
                        category.DisplayOrder = categoryModel.DisplayOrder;
                        category.Published = categoryModel.Published;
                        category.ShowOnHomePage = categoryModel.flagTopCategory;
                        category.FlagShowBuy = categoryModel.FlagShowBuy;
                        category.ParentCategoryId = categoryModel.SelectedCategory;
                        category.UpdatedOnUtc = DateTime.Now;
                        category.PermaLink = categoryModel.Name.ToLower().Replace(" ", "-");
                        category.BranchId = BranchId;
                        _context.Category.Update(category);
                        _context.SaveChanges();
                        result.UpdatedId = category.CategoryId;
                        result.Status = Enums.UpdateStatus.Success;
                    }
                }
                else
                {
                    var categoryExist = _context.Category.Where(x => x.Name == categoryModel.Name && x.ParentCategoryId == categoryModel.SelectedCategory && x.BranchId == BranchId && (x.IsDeleted == false || x.IsDeleted == null)).ToList();
                    if (categoryExist.Count > 0)
                    {
                        result.Status = Enums.UpdateStatus.AlreadyExist;
                    }
                    else
                    {
                        var categoryResultSet = _categoryRepository.CreateCategory(BranchId, categoryModel, currentUser);
                        if (categoryResultSet != null && categoryResultSet.Status == Enums.UpdateStatus.Success)
                        {
                            result = categoryResultSet;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result.Status = Enums.UpdateStatus.Error;
            }
            return Ok(result);
        }

        [Authorize(Policy = PolicyTypes.Category_Read)]
        [HttpGet("Seller/{BranchId}/Category")]
        public IActionResult LoadCategory(int BranchId)
        {
            List<CategoryWithChildren> categoryWithChildrens = new List<CategoryWithChildren>();
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var parentCategory = _context.Category.Where(x => x.ParentCategoryId == null && (x.IsDeleted == false || x.IsDeleted == null) && x.BranchId == BranchId)
                    .Select(x => new MainCategory
                    {
                        Name = x.Name,
                        Id = x.CategoryId,
                        CategoryImage = x.CategoryImage,
                        PermaLink = x.PermaLink,
                    }).ToList();

                var subCategory = _context.Category.Where(x => (x.IsDeleted == false || x.IsDeleted == null) && x.BranchId == BranchId)
                    .Select(x => new MainCategory
                    {
                        Name = x.Name,
                        Id = x.CategoryId,
                        CategoryImage = x.CategoryImage,
                        ParentCategoryId = (int)(x.ParentCategoryId != null ? x.ParentCategoryId : 0),
                        PermaLink = x.PermaLink,

                    }).ToList();
                foreach (MainCategory eachCategory in parentCategory)
                {
                    var eachList = subCategory.Where(a => a.ParentCategoryId == eachCategory.Id).Select(b => b);
                    CategoryWithChildren categoryWithChildren = new CategoryWithChildren();
                    categoryWithChildren.Name = eachCategory.Name;
                    categoryWithChildren.Id = eachCategory.Id;
                    if (eachCategory.CategoryImage != null)
                    {
                        if (!eachCategory.CategoryImage.Contains("http"))
                        {
                            categoryWithChildren.CategoryImage = _appSettings.ImageUrlBase + eachCategory.CategoryImage;
                        }
                        else
                        {
                            categoryWithChildren.CategoryImage = eachCategory.CategoryImage;
                        }
                    }
                    categoryWithChildren.PermaLink = eachCategory.PermaLink;
                    if (eachList != null)
                    {
                        eachList = eachList.ToList();
                        List<MainCategory> theList = eachList.Cast<MainCategory>().ToList();
                        foreach (var eachData in theList)
                        {
                            if (eachData.CategoryImage != null)
                            {
                                if (!eachData.CategoryImage.Contains("http"))
                                {
                                    eachData.CategoryImage = _appSettings.ImageUrlBase + eachData.CategoryImage;
                                }
                                else
                                {
                                    eachData.CategoryImage = eachData.CategoryImage;
                                }
                            }
                        }
                        categoryWithChildren.Children = theList;
                    }
                    categoryWithChildrens.Add(categoryWithChildren);
                }

                return Ok(categoryWithChildrens);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Category_Read)]
        [HttpGet("Seller/{BranchId}/GetParentCategory")]
        public IActionResult GetParentCategory(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var categoryList = _context.Category.Where(a => a.IsDeleted != true && a.ParentCategoryId == null && a.BranchId == BranchId).ToList();
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

        [Authorize(Policy = PolicyTypes.Category_Read)]
        [HttpGet("Seller/{BranchId}/Category/{categoryId}")]
        public IActionResult GetCategoryDetails(int BranchId, int categoryId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var category = _context.Category.Where(x => x.CategoryId == categoryId && x.BranchId == BranchId).FirstOrDefault();
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

        [Authorize(Policy = PolicyTypes.Category_Read)]
        [HttpGet("Seller/{BranchId}/GetCategoryBasedProductDetails/{CateId}")]
        public IActionResult GetCategoryBasedProductDetails(int BranchId, int CateId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var mainCategories = _context.Category.Where(x => x.ParentCategoryId == CateId && x.BranchId == BranchId).Select(a => a.CategoryId).ToList();

                foreach (int eachCategory in mainCategories)
                {
                    var totalCount = _context.ProductStoreMapping.Where(a => a.Category == eachCategory && a.BranchId == BranchId && a.IsDeleted == false).Count();
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

        [Authorize(Policy = PolicyTypes.Category_Read)]
        [HttpGet("Seller/{BranchId}/GetSubCategoryBasedProductDetails/{CateId}")]
        public IActionResult GetSubCategoryBasedProductDetails(int BranchId, int CateId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var totalCount = _context.ProductStoreMapping.Where(x => x.Category == CateId && x.BranchId == BranchId && x.IsDeleted == false).Select(a => a.ProductId).Count();
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

        [Authorize(Policy = PolicyTypes.Category_Delete)]
        [HttpDelete("Seller/{BranchId}/Category/{categoryId}")]
        public IActionResult DeleteCategory(int BranchId, int categoryId)
        {
            try
            {
                var currentUser = User.Identity.Name;
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (categoryId > 0)
                {
                    var mainCategories = _context.Category.Where(x => x.ParentCategoryId == categoryId && x.BranchId == BranchId).Select(a => a.CategoryId).ToList();
                    if (mainCategories.Count > 0)
                    {
                        foreach (int eachCategory in mainCategories)
                        {
                            var subCategoryDetails = _context.Category.Where(a => a.CategoryId == eachCategory && a.BranchId == BranchId).FirstOrDefault();
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
                    var categoryDetails = _context.Category.Where(x => x.CategoryId == categoryId && x.BranchId == BranchId).FirstOrDefault();
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

        [Authorize(Policy = PolicyTypes.Category_Read)]
        [HttpGet("Seller/{BranchId}/LoadSubCategory/{CateId}")]
        public List<CategoryResult> LoadSubCategory(int BranchId, int CateId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return null;
                }
                var subCategoryDetails = _context.Category.Where(x => x.ParentCategoryId == CateId && x.BranchId == BranchId && (x.IsDeleted == false || x.IsDeleted == null)).ToList();
                List<CategoryResult> catDToList = new List<CategoryResult>();
                var categoeyDetails = _mapper.Map<List<Category>, List<CategoryResult>>(subCategoryDetails, catDToList);
                foreach (var eachData in categoeyDetails)
                {
                    if (eachData.CategoryImage != null)
                    {
                        if (!eachData.CategoryImage.Contains("http"))
                        {
                            eachData.CategoryImage = _appSettings.ImageUrlBase + eachData.CategoryImage;
                        }
                    }
                }
                return categoeyDetails;
            }
            catch (Exception Ex)
            {
                return null;
            }
        }

        [Authorize(Policy = PolicyTypes.Category_Edit)]
        [HttpPut("Seller/{BranchId}/UpdateCategory")]
        public IActionResult ModifyCategory(int BranchId, CategoryModelDTO categoryModel)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }

                string LoginUser = User.FindFirst("UserId")?.Value;
                var categoryDetail = _context.Category.Where(x => x.Name == categoryModel.Name && x.CategoryId != categoryModel.CategoryId && x.BranchId == BranchId && x.IsDeleted == false).FirstOrDefault();
                if (categoryDetail == null)
                {
                    var category = _context.Category.Where(x => x.CategoryId == categoryModel.CategoryId && x.BranchId == BranchId).FirstOrDefault();
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

        [Authorize(Policy = PolicyTypes.Category_Delete)]
        [HttpDelete("Seller/{BranchId}/DeleteCategoryImage/{categoryId}")]
        public IActionResult DeleteCategoryImage(int BranchId, int categoryId, string pictureName)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (categoryId > 0)
                {
                    var category = _context.Category.Where(x => x.CategoryId == categoryId && x.BranchId == BranchId).FirstOrDefault<Category>();
                    if (category != null)
                    {
                        category.CategoryImage = null;
                        category.UpdatedOnUtc = DateTime.UtcNow;
                    }
                    _context.Category.Update(category);
                    _context.SaveChanges();
                }
                return Ok(Enums.UpdateStatus.Success.ToString());
            }
            catch (Exception e)
            {
            }
            return Ok(Enums.UpdateStatus.Failure.ToString());
        }

        //Manufacture
        [Authorize(Policy = PolicyTypes.Category_Write)]
        [HttpPost("Seller/{BranchId}/Manufacturer")]
        public IActionResult AddManufacturer(int BranchId, ManufacturerDTO manufacturerDTO)
        {
            BaseUpdateResultSet result = new BaseUpdateResultSet();
            result.Status = Enums.UpdateStatus.Failure;
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var manufactureExist = _context.Manufacturer.Where(x => x.Name == manufacturerDTO.Name && x.BranchId == BranchId).ToList();
                if (manufactureExist.Count > 0)
                {
                    result.Status = Enums.UpdateStatus.AlreadyExist;
                }
                else
                {
                    Manufacturer manufacturer = new Manufacturer();
                    manufacturer.Name = manufacturerDTO.Name;
                    manufacturer.Description = manufacturerDTO.Description;
                    manufacturer.Deleted = false;
                    manufacturer.DisplayOrder = 0;
                    manufacturer.MetaTitle = manufacturerDTO.Name;
                    manufacturer.CreatedOnUtc = DateTime.UtcNow;
                    manufacturer.BranchId = BranchId;
                    _context.Manufacturer.Add(manufacturer);
                    _context.SaveChanges();
                    result.UpdatedId = manufacturer.ManufacturerId;
                    result.Status = Enums.UpdateStatus.Success;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
            return Ok(result);
        }

        [Authorize(Policy = PolicyTypes.Category_Write)]
        [HttpPut("Seller/{BranchId}/Manufacturer")]
        public IActionResult UpdateManufacturer(int BranchId, ManufacturerDTO manufacturerDTO)
        {
            BaseUpdateResultSet result = new BaseUpdateResultSet();
            result.Status = Enums.UpdateStatus.Failure;
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var manufactureExist = _context.Manufacturer.Where(x => x.Name == manufacturerDTO.Name && x.ManufacturerId != manufacturerDTO.ManufacturerId && x.BranchId == BranchId).ToList();
                if (manufactureExist.Count > 0)
                {
                    result.Status = Enums.UpdateStatus.AlreadyExist;
                }
                else
                {
                    var manufactureDetails = _context.Manufacturer.Where(x => x.ManufacturerId == manufacturerDTO.ManufacturerId && x.BranchId == BranchId).FirstOrDefault();
                    if (manufactureDetails != null)
                    {
                        manufactureDetails.Name = manufacturerDTO.Name;
                        manufactureDetails.Description = manufacturerDTO.Description;
                        manufactureDetails.UpdatedOnUtc = DateTime.UtcNow;
                        _context.Manufacturer.Update(manufactureDetails);
                        _context.SaveChanges();
                    }
                    result.Status = Enums.UpdateStatus.Success;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
            return Ok(result);
        }
        [Authorize(Policy = PolicyTypes.Category_Read)]
        [HttpGet("Seller/{BranchId}/Manufacturer")]
        public IActionResult GetAllManufacturer(int BranchId)
        {
            List<ManufacturerResult> manufacturerResultList = new List<ManufacturerResult>();
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var manufactureDetails = _context.Manufacturer.Where(x => x.BranchId == BranchId && x.Deleted != true).ToList();
                if (manufactureDetails.Count > 0)
                {
                    foreach (var manufacturer in manufactureDetails)
                    {
                        ManufacturerResult manufacturerResult = new ManufacturerResult();
                        manufacturerResult.ManufacturerId = manufacturer.ManufacturerId;
                        manufacturerResult.Name = manufacturer.Name;
                        manufacturerResult.Description = manufacturer.Description;
                        manufacturerResult.Deleted = manufacturer.Deleted;
                        if (manufacturer.ManufacturerImage != null)
                        {
                            if (!manufacturer.ManufacturerImage.Contains("http"))
                            {
                                manufacturerResult.ManufacturerImage = _appSettings.ImageUrlBase + manufacturer.ManufacturerImage;
                            }
                            else
                            {
                                manufacturerResult.ManufacturerImage = manufacturer.ManufacturerImage;
                            }
                        }
                        manufacturerResultList.Add(manufacturerResult);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
            return Ok(manufacturerResultList);
        }

        [Authorize(Policy = PolicyTypes.Category_Read)]
        [HttpGet("Seller/{BranchId}/Manufacturer/{ManufacturerId}")]
        public IActionResult GetManufacturerById(int BranchId, int ManufacturerId)
        {
            ManufacturerResult manufacturerResult = new ManufacturerResult();
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var manufactureDetails = _context.Manufacturer.Where(x => x.BranchId == BranchId && x.ManufacturerId == ManufacturerId).FirstOrDefault();
                if (manufactureDetails != null)
                {
                    manufacturerResult.ManufacturerId = manufactureDetails.ManufacturerId;
                    manufacturerResult.Name = manufactureDetails.Name;
                    manufacturerResult.Description = manufactureDetails.Description;
                    manufacturerResult.Deleted = manufactureDetails.Deleted;
                    if (manufactureDetails.ManufacturerImage != null)
                    {
                        if (!manufactureDetails.ManufacturerImage.Contains("http"))
                        {
                            manufacturerResult.ManufacturerImage = _appSettings.ImageUrlBase + manufactureDetails.ManufacturerImage;
                        }
                        else
                        {
                            manufacturerResult.ManufacturerImage = manufactureDetails.ManufacturerImage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
            return Ok(manufacturerResult);
        }

        [Authorize(Policy = PolicyTypes.Category_Delete)]
        [HttpDelete("Seller/{BranchId}/Manufacturer/{ManufacturerId}")]
        public IActionResult DeleteManufacturer(int BranchId, int ManufacturerId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (ManufacturerId > 0)
                {
                    var totalCount = _context.ProductStoreMapping.Where(x => x.Manufacturer == ManufacturerId && x.IsDeleted == false).Select(a => a.ProductId).Count();
                    if (Convert.ToInt32(totalCount) > 0)
                    {
                        return Ok("Unable to perform action, this brand has products added to it, to delete a brand there should be no product linked to it");
                    }
                    var manufacturerDetails = _context.Manufacturer.Where(x => x.ManufacturerId == ManufacturerId && x.BranchId == BranchId).FirstOrDefault();
                    if (manufacturerDetails != null)
                    {
                        manufacturerDetails.Deleted = true;
                        manufacturerDetails.UpdatedOnUtc = DateTime.UtcNow;
                        _context.Manufacturer.Update(manufacturerDetails);
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

        [Authorize(Policy = PolicyTypes.Category_Delete)]
        [HttpDelete("Seller/{BranchId}/DeleteManufacturerImage/{ManufacturerId}")]
        public IActionResult DeleteManufacturerImage(int BranchId, int ManufacturerId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (ManufacturerId > 0)
                {
                    var manufacturer = _context.Manufacturer.Where(x => x.ManufacturerId == ManufacturerId && x.BranchId == BranchId).FirstOrDefault();
                    if (manufacturer != null)
                    {
                        manufacturer.ManufacturerImage = null;
                        manufacturer.UpdatedOnUtc = DateTime.UtcNow;
                    }
                    _context.Manufacturer.Update(manufacturer);
                    _context.SaveChanges();
                }
                return Ok(Enums.UpdateStatus.Success.ToString());
            }
            catch (Exception e)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Category_Read)]
        [HttpGet("Seller/{BranchId}/SearchBrands/{searchString}")]
        public IActionResult SearchBrands(int BranchId, string searchString)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (!string.IsNullOrEmpty(searchString))
                {
                    var manufacturer = _context.Manufacturer.Where(x => x.Name.Contains(searchString) && x.BranchId == BranchId && (x.Deleted == false || x.Deleted == null)).ToList();
                    if (manufacturer != null)
                    {
                        List<ManufacturerResult> manufacturerResultList = new List<ManufacturerResult>();
                        var manufactureDetailsList = _mapper.Map<List<Manufacturer>, List<ManufacturerResult>>(manufacturer);
                        foreach (var manufacturerResult in manufactureDetailsList)
                        {
                            if (manufacturerResult.ManufacturerImage != null)
                            {
                                if (!manufacturerResult.ManufacturerImage.Contains("http"))
                                {
                                    manufacturerResult.ManufacturerImage = _appSettings.ImageUrlBase + manufacturerResult.ManufacturerImage;
                                }
                                else
                                {
                                    manufacturerResult.ManufacturerImage = manufacturerResult.ManufacturerImage;
                                }
                            }
                        }
                        return Ok(manufactureDetailsList);
                    }
                }
                return Ok(new List<ManufacturerResult>());
            }
            catch (Exception e)
            {
                return StatusCode(500, "There is some error");
            }
        }

        //For the sample or default products
        [Authorize(Policy = PolicyTypes.Category_Read)]
        [HttpGet("Seller/{BranchId}/SearchDefaultProduct/{searchString}")]
        public IActionResult SearchDefaultProduct(int BranchId, string? searchString)
        {
            try
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        var products = _context.Product.Where(x => x.Name.Contains(searchString) && x.FlagSharedInfo == true && (x.IsDeleted == false || x.IsDeleted == null)).ToList();
                        if (products.Count > 0)
                        {
                            return Ok(products);
                        }
                    }
                }
                return Ok(new List<Product>());
            }
            catch (Exception e)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Read)]
        [HttpGet("Seller/{BranchId}/GetDefaultProduct/{id}")]
        public IActionResult GetDefaultProduct(int BranchId, int id)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
        
                var productDetails = _context.Product.Where(x => x.ProductId == id).Include(x => x.CategoryDetails).First<Product>();
                var product = _mapper.Map<Product, ProductResult>(productDetails);
                if (product != null && product.ProductId > 0)
                {
                    product.ProductImages = new List<ProductImageDetails>();
                    var productImages = _context.ProductImage.Where(x => x.ProductId == id).ToList();
                    foreach (var images in productImages)
                    {
                        ProductImageDetails imageDetails = new ProductImageDetails();
                        imageDetails.Id = images.Id;
                        if (images.PictureName != null)
                        {
                            if (!images.PictureName.Contains("http"))
                            {
                                imageDetails.PictureName = _appSettings.ImageUrlBase + images.PictureName;
                            }
                            else
                            {
                                imageDetails.PictureName = images.PictureName;
                            }
                        }
                        product.ProductImages.Add(imageDetails);
                    }
                    product.VariantsPricing = new List<VariantsPricing>();
                    var VariantsPricing = _context.Pricing.Where(x => x.Product == id && x.ProductVariantId == null && (x.IsDeleted == false || x.IsDeleted == null)).ToList();
                    foreach (var eachProductPrice in VariantsPricing)
                    {

                        VariantsPricing pricing = new VariantsPricing();
                        pricing.PricingId = eachProductPrice.PricingId;
                        pricing.ProductVariantId = eachProductPrice.ProductVariantId;
                        pricing.Price = eachProductPrice.Price;
                        pricing.SpecialPrice = eachProductPrice.SpecialPrice;
                        pricing.SpecialPriceDescription = eachProductPrice.SpecialPriceDescription;
                        pricing.Store = eachProductPrice.Store;
                        var productVariant = _context.ProductVariants.Where(x => x.Id == eachProductPrice.ProductVariantId && x.FlagDeleted != true)
                            .Select(x => new options { option1 = x.Option1, option2 = x.Option2, option3 = x.Option3 }).FirstOrDefault();
                        pricing.ProductVariant = productVariant;
                        product.VariantsPricing.Add(pricing);
                    }
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Delete)]
        [Authorize(Policy = PolicyTypes.Category_Delete)]
        [HttpDelete("Seller/{branchId}/DeleteSampleProducts")]
        public IActionResult DeleteSampleProducts(int branchId)
        {
            Enums.UpdateStatus status = Enums.UpdateStatus.Failure;
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == branchId.ToString()).Any())
                {
                    return Unauthorized();
                }

                var categoryList = new List<int>();
                var productsList = new List<int>();
                var parentCategoryIds = new List<int>();

                var deleteProducts = _context.ProductStoreMapping.Where(x => x.FlagSampleProducts == true && x.BranchId == branchId).ToList();
                foreach (var eachProduct in deleteProducts)
                {
                    eachProduct.IsDeleted = true;
                    eachProduct.UpdatedOnUtc = DateTime.UtcNow;
                    categoryList.Add(eachProduct.Category);
                    productsList.Add(eachProduct.ProductId);
                    _context.ProductStoreMapping.Update(eachProduct);
                }
                _context.SaveChanges();
                foreach (var eachProductItem in productsList)
                {
                    var productPricings = _context.Pricing.Where(x => x.Product == eachProductItem && x.Branch == branchId).ToList();
                    foreach (var eachProductPricing in productPricings)
                    {
                        eachProductPricing.IsDeleted = true;
                        eachProductPricing.UpdatedOnUtc = DateTime.UtcNow;
                        _context.Pricing.Update(eachProductPricing);
                    }
                    _context.SaveChanges();
                }

                var distictCategories = categoryList.Distinct();
                foreach (var eachCategory in distictCategories)
                {
                    var category = _context.Category.Where(x => x.CategoryId == eachCategory && x.FlagSampleCategory == true && x.BranchId == branchId).ToList();
                    foreach (var categoryItem in category)
                    {
                        categoryItem.IsDeleted = true;
                        categoryItem.UpdatedOnUtc = DateTime.UtcNow;
                        parentCategoryIds.Add((int)categoryItem.ParentCategoryId);
                        _context.Update(categoryItem);
                    }
                    _context.SaveChanges();
                }
                var distinctParentCategories = parentCategoryIds.Distinct();
                foreach (var eachParentCategory in distinctParentCategories)
                {
                    var parentCategories = _context.Category.Where(x => x.CategoryId == eachParentCategory && x.FlagSampleCategory == true && x.BranchId == branchId).ToList();
                    foreach (var eachParentCategoryItem in parentCategories)
                    {
                        eachParentCategoryItem.IsDeleted = true;
                        eachParentCategoryItem.UpdatedOnUtc = DateTime.UtcNow;
                        _context.Update(eachParentCategoryItem);
                    }
                    _context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is an some error occured"); ;
            }
        }
    }
}
