using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;
using Microsoft.EntityFrameworkCore;
using PagedList;
using Microsoft.Extensions.Configuration;
using VSOnline.VSECommerce.Domain.Helper;

namespace VSOnline.VSECommerce.Domain
{
    public class PricingRepository
    {
        private readonly DataContext _context;
        private IMapper _mapper;
        IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        private readonly IConfigurationSection _appSettings;
        private readonly RatingHelper _ratingHelper;
        public PricingRepository(DataContext context, IMapper mapper, RatingHelper ratingHelper)
        {
            _context = context;
            _mapper = mapper;
            _appSettings = _configuration.GetSection("AppSettings");
            _ratingHelper = ratingHelper;
        }
        public Enums.UpdateStatus IncludeProduct(RetailerAddProductDTO retailerProductDTO, int branchId, string user)
        {
            var status = Enums.UpdateStatus.Failure;

            try
            {
                if (!FlagProductExistInStore(retailerProductDTO, branchId))
                {
                    Pricing priceProduct = new Pricing
                    {
                        Product = retailerProductDTO.ProductId,
                        CallForPrice = false,
                        CreatedUser = user,
                        SpecialPrice = retailerProductDTO.NewSpecialPrice,
                        SpecialPriceDescription = retailerProductDTO.NewSpecialPriceDescription,
                        Price = retailerProductDTO.NewPrice,
                        AdditionalTax = retailerProductDTO.NewAdditionalTax,
                        SpecialPriceStartDateTimeUtc = retailerProductDTO.NewPriceStartTime,
                        SpecialPriceEndDateTimeUtc = retailerProductDTO.NewPriceEndTime,
                        OldPrice = 0,
                        Store = retailerProductDTO.StoreId,
                        Branch = branchId,
                        IsDeleted = false,
                    };

                    _context.Pricing.Add(priceProduct);
                    status = Enums.UpdateStatus.Success;
                }
                else
                {
                    status = Enums.UpdateStatus.AlreadyExist;
                }
            }
            catch
            {
                status = Enums.UpdateStatus.Error;
            }
            return status;
        }

        public bool DeleteProductInPricing(int productId,int BranchId)
        {
            try
            {
                var product = _context.Pricing.Where(x => x.Product == productId && x.Branch == BranchId).ToList();
                if (product.Count > 0)
                {
                    foreach (var item in product)
                    {
                        item.IsDeleted = true;
                        item.UpdatedOnUtc = DateTime.UtcNow;
                        _context.Pricing.Update(item);
                    }
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public bool FlagProductExistInStore(RetailerAddProductDTO retailerProductDTO, int branchId)
        {
            var pricing = _context.Pricing.Where(x => x.Store == retailerProductDTO.StoreId && x.Branch == branchId && x.Product == retailerProductDTO.ProductId && (x.IsDeleted == false || x.IsDeleted == null)).ToList();
            if (pricing.Count > 0)
            {
                return true;
            }
            return false;
        }
        public List<RetailerPricingModel> GetMyFilteredProductsPaging(int BranchId, int selectedCategory, int selectedSubCategory, int storeId, int? selectedBrand, int pageNo, int PageSize)
        {
            var pricePaging = _context.Pricing.Where(x => x.Branch == BranchId && x.Store == storeId && x.ProductDetails.Category == selectedSubCategory
         && (selectedBrand != null ? x.ProductDetails.Manufacturer == selectedBrand : x.ProductDetails.Manufacturer != null)).Include(y => y.ProductDetails).Include(z => z.BranchDetails)
         .OrderBy(x => x.PricingId).ToPagedList(pageNo, PageSize);

            List<RetailerPricingModel> retailPricingModelList = new List<RetailerPricingModel>();
            _mapper.Map<IEnumerable<Pricing>, IEnumerable<RetailerPricingModel>>(pricePaging, retailPricingModelList);

            return retailPricingModelList;
        }
        public Pricing UpdatePricing(RetailerUpdateProductDTO retailerUpdateProductDTO, Pricing pricing, string user, int branch)
        {
            try
            {
                if (pricing != null)
                {
                    var oldPrice = pricing.Price;

                    pricing.OldPrice = oldPrice ?? 0;
                    pricing.Price = retailerUpdateProductDTO.NewPrice;
                    pricing.SpecialPrice = retailerUpdateProductDTO.NewSpecialPrice;
                    pricing.SpecialPriceDescription = retailerUpdateProductDTO.NewSpecialPriceDescription;

                    pricing.AdditionalShippingCharge = retailerUpdateProductDTO.NewShippingCharge;
                    pricing.AdditionalTax = retailerUpdateProductDTO.NewAdditionalTax;
                    pricing.SpecialPriceStartDateTimeUtc = retailerUpdateProductDTO.NewPriceStartTime;
                    pricing.SpecialPriceEndDateTimeUtc = retailerUpdateProductDTO.NewPriceEndTime;
                    pricing.DeliveryTime = retailerUpdateProductDTO.NewDeliveryTime;
                    pricing.UpdatedUser = user;
                    _context.Pricing.Update(pricing);
                    _context.SaveChanges();
                    return pricing;
                }
                else
                {
                    Pricing priceProduct = new Pricing
                    {
                        Product = retailerUpdateProductDTO.ProductId,
                        CallForPrice = false,
                        CreatedUser = user,
                        SpecialPrice = retailerUpdateProductDTO.NewSpecialPrice,
                        SpecialPriceDescription = retailerUpdateProductDTO.NewSpecialPriceDescription,
                        Price = retailerUpdateProductDTO.NewPrice,
                        AdditionalTax = retailerUpdateProductDTO.NewAdditionalTax,
                        SpecialPriceStartDateTimeUtc = retailerUpdateProductDTO.NewPriceStartTime,
                        SpecialPriceEndDateTimeUtc = retailerUpdateProductDTO.NewPriceEndTime,
                        OldPrice = 0,
                        Store = retailerUpdateProductDTO.StoreId,
                        Branch = branch,
                        IsDeleted = false,
                    };
                    _context.Pricing.Add(priceProduct);
                    _context.SaveChanges();
                    return priceProduct;
                }
            }
            catch
            {
                return null;
            }
        }

        //product variant
        public Enums.UpdateStatus IncludeProductForVariant(RetailerAddProductDTOVariant retailerProductDTO, int branchId, string user)
        {
            var status = Enums.UpdateStatus.Failure;

            try
            {
                if (!FlagProductExistInStoreNew(retailerProductDTO, branchId))
                {
                    Pricing priceProduct = new Pricing
                    {
                        Product = retailerProductDTO.ProductId,
                        CallForPrice = false,
                        CreatedUser = user,
                        SpecialPrice = retailerProductDTO.NewSpecialPrice,
                        SpecialPriceDescription = retailerProductDTO.NewSpecialPriceDescription,
                        Price = retailerProductDTO.NewPrice,
                        AdditionalTax = retailerProductDTO.NewAdditionalTax,
                        SpecialPriceStartDateTimeUtc = retailerProductDTO.NewPriceStartTime,
                        SpecialPriceEndDateTimeUtc = retailerProductDTO.NewPriceEndTime,
                        OldPrice = 0,
                        Store = retailerProductDTO.StoreId,
                        Branch = branchId,
                        IsDeleted = false,
                        ProductVariantId = retailerProductDTO.ProductVariantId
                    };

                    _context.Pricing.Add(priceProduct);
                    status = Enums.UpdateStatus.Success;
                }
                else
                {
                    status = Enums.UpdateStatus.AlreadyExist;
                }
            }
            catch
            {
                status = Enums.UpdateStatus.Error;
            }
            return status;
        }
        public bool FlagProductExistInStoreNew(RetailerAddProductDTOVariant retailerProductDTO, int branchId)
        {
            var pricing = _context.Pricing.Where(x => x.Store == retailerProductDTO.StoreId && x.Branch == branchId
                 && x.Product == retailerProductDTO.ProductId && x.ProductVariantId == retailerProductDTO.ProductVariantId && (x.IsDeleted == false || x.IsDeleted == null)).ToList();

            if (pricing.Count > 0)
            {
                return true;
            }
            return false;
        }
        // for Hyperlocal
        public List<ProductModel> GetStoreProducts(int selectedCategory, int selectedSubCategory, int storeId, int selectedBranchId, int? selectedBrand)
        {

            var pricingList = _context.ProductStoreMapping.Where(x => x.BranchId == selectedBranchId && x.StoreId == storeId && x.Category == selectedSubCategory
         && (selectedBrand != null ? x.Manufacturer == selectedBrand : x.Manufacturer != null) && (x.IsDeleted == false || x.IsDeleted == null)).Include(y => y.PricingCollection).ToList();


            List<ProductModel> productModelList = new List<ProductModel>();
            foreach (var pricing in pricingList)
            {
                ProductModel item = new ProductModel();
                item.ProductId = pricing.ProductId;
                item.Name = pricing.Name;
                item.SpecialPrice =  _context.Pricing.Where(x=>x.Product == pricing.ProductId && (x.IsDeleted == null || x.IsDeleted == false)).Select(x=>x.SpecialPrice).FirstOrDefault();
                item.Price = _context.Pricing.Where(x=>x.Product == pricing.ProductId && (x.IsDeleted == null || x.IsDeleted == false)).Select(x=>x.Price).FirstOrDefault();
                item.StoresCount = 0;
                item.PermaLink = pricing.PermaLink;
                var pictureName = _context.ProductImage.Where(b => b.ProductId == pricing.ProductId).FirstOrDefault();
                if (pictureName != null)
                {
                    item.PictureName = (!string.IsNullOrEmpty(pictureName.PictureName)
                    && !pictureName.PictureName.Contains("http")) ? _appSettings.GetValue<string>("ImageUrlBase") + pictureName.PictureName
                    : !string.IsNullOrEmpty(pictureName.PictureName) ? pictureName.PictureName : "";
                }
                item.ProductRating = _ratingHelper.CalulateProductRating(pricing.ProductId);
                item.ProductRatingCount = _context.ProductRating.Where(x => x.ProductId == pricing.ProductId).Count();
                productModelList.Add(item);
            }

            return productModelList;
        }

    }
}
