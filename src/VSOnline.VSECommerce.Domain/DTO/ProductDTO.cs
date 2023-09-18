using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class ProductDTO
    {
        public int? ProductId { get; set; }
        public int ProductTypeId { get; set; }
        public int Category { get; set; }
        public int Manufacturer { get; set; }
        public string? PictureName { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public bool ShowOnHomePage { get; set; }
        public string? MetaKeywords { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaTitle { get; set; }
        public bool? SubjectToAcl { get; set; }
        public bool? LimitedToStores { get; set; }
        public string? Sku { get; set; }
        public string? ManufacturerPartNumber { get; set; }
        public string? Gtin { get; set; }
        public bool IsGiftCard { get; set; }
        public int GiftCardTypeId { get; set; }
        public string? Weight { get; set; }
        public string? Length { get; set; }
        public string? Width { get; set; }
        public string? Height { get; set; }
        public string? Color { get; set; }
        public int DisplayOrder { get; set; }
        public bool? Published { get; set; }
        public bool IsDeleted { get; set; }
        public string? Size1 { get; set; }
        public string? Size2 { get; set; }
        public string? Size3 { get; set; }
        public string? Size4 { get; set; }
        public string? Size5 { get; set; }
        public string? Size6 { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public System.DateTime UpdatedOnUtc { get; set; }
    }

    public class ProductModel
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public string? PictureName { get; set; }
        public decimal? Price { get; set; }
        public int? StoresCount { get; set; }
        public bool? FlagWishlist { get; set; }
        public string? FullDescription { get; set; }
        public decimal? SpecialPrice { get; set; }
        public string? BranchName { get; set; }
        public int? BranchId { get; set; }
        public bool? EnableBuy { get; set; }
        public bool? FlagShowBuy { get; set; }
        public int? Category { get; set; }
        public decimal? AdditionalShippingCharge { get; set; }
        public string? PermaLink { get; set; }
        public int ProductRating { get; set; }  
        public int ProductRatingCount { get; set; }   
    }


    public class ProductModelForBranchCatalog
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string PictureName { get; set; }
        public decimal? Price { get; set; }
        public int StoresCount { get; set; }
        public bool? FlagWishlist { get; set; }
        public List<int> BranchList { get; set; }
        public bool FlagExist { get; set; }
    }

    public class ProductModelWithCategory
    {
        public int? ProductId { get; set; }
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string? PictureName { get; set; }
        public decimal? SpecialPrice { get; set; }
        public decimal? Price { get; set; }
        public int? StoresCount { get; set; }
        public int? TotalCount { get; set; }
        public bool? FlagWishlist { get; set; }
        public int? BranchId { get; set; }
        public decimal? AdditionalShippingCharge { get; set; }
        public string? BranchName { get; set; }
        public int? DeliveryTime { get; set; }
        public string? PermaLink { get; set; }
        public int ProductRating { get; set; }
        public int ProductRatingCount { get; set; } 

    }

    public class ProductModelWithCategoryBuy
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string PictureName { get; set; }
        public decimal? SpecialPrice { get; set; }
        public decimal? Price { get; set; }
        public int StoresCount { get; set; }
        public int? TotalCount { get; set; }
        public bool? FlagWishlist { get; set; }
        public string BranchName { get; set; }
        public int? Branch { get; set; }
        public int? DeliveryTime { get; set; }
        public decimal? AdditionalShippingCharge { get; set; }
        public bool? EnableBuy { get; set; }
        public bool? FlagShowBuy { get; set; }
    }

    public class ProductDetailModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        //public string PictureName { get; set; }
        //public string PictureName1 { get; set; }
        //public string PictureName2 { get; set; }
        public string FullDescription { get; set; }
        public int Manufacturer { get; set; }
        public string BrandName { get; set; }
        public List<StorePricingModel> StorePricingModel { get; set; }

        public List<string> ProductImages { get; set; }

        public string ManufacturerPartNumber { get; set; }
        public string Weight { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Color { get; set; }

        public string Size1 { get; set; }
        public string Size2 { get; set; }
        public string Size3 { get; set; }
        public string Size4 { get; set; }
        public string Size5 { get; set; }
        public string Size6 { get; set; }

        public string AndroidInformation1 { get; set; }

        public int Category { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public string CategoryGroupTag { get; set; }
        public string? PermaLink { get; set; }
        public List<ProductModelWithCategory> RelatedProductList { get; set; }
    }

    public class ProductPricingModel
    {
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public string MetaTitle { get; set; }
        public string ImageUrl { get; set; }
        public List<StorePricingModel> StorePricing { get; set; }
    }

    public class RetailerPricingModel
    {
        public int ProductId { get; set; }
        public string PictureName { get; set; }
        public int Store { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public decimal SpecialPrice { get; set; }
        public string SpecialPriceDescription { get; set; }
        public decimal OldPrice { get; set; }
        public decimal AdditionalTax { get; set; }
        public Nullable<DateTime> PriceStartTime { get; set; }
        public Nullable<DateTime> PriceEndTime { get; set; }
        public bool IsFreeShipping { get; set; }
        public bool? IsDeleted { get; set; }
        public decimal AdditionalShippingCharge { get; set; }
        public int DeliveryTime { get; set; }
    }

    public class StorePricingModel
    {
        public int StoreId { get; set; }
        public int BranchId { get; set; }
        public string StoreName { get; set; }
        public string BranchName { get; set; }
        public string BranchAddress1 { get; set; }
        public string BranchAddress2 { get; set; }
        public string BranchCity { get; set; }
        public decimal? Price { get; set; }
        public decimal? SpecialPrice { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public decimal? BranchRating { get; set; }
        public decimal? AdditionalTax { get; set; }
        public decimal? AdditionalShippingCharge { get; set; }
        public int? DeliveryTime { get; set; }
        public bool? EnableBuy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public String SpecialPriceDescription { get; set; }
        public bool FlagQuantityExceeded { get; set; }
        public int? AvailableQuantity { get; set; }
        public int? ProductVariantId { get; set; }
    }

    public class ProductModelWithDescription
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public string SubCategoryName { get; set; }
        public string PictureName { get; set; }
        public decimal? SpecialPrice { get; set; }
        public decimal? Price { get; set; }
        public int StoresCount { get; set; }
        public int? TotalCount { get; set; }
        public bool? FlagWishlist { get; set; }
        public string BranchName { get; set; }
        public int? Branch { get; set; }
        public int? DeliveryTime { get; set; }
        public decimal? AdditionalShippingCharge { get; set; }
        public bool? EnableBuy { get; set; }
        public bool? FlagShowBuy { get; set; }
        public string FullDescription { get; set; }
        public string PermaLink { get; set; }
        public int ProductRating { get; set; }
        public int ProductRatingCount { get; set; }
    }

    public class SearchProductModelWithBuy
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }
        public string? ParentCategoryName { get; set; }
        public string? SubCategoryName { get; set; }
        public string? PictureName { get; set; }
        public decimal? SpecialPrice { get; set; }
        public decimal? Price { get; set; }
        public int StoresCount { get; set; }
        public int? TotalCount { get; set; }
        public bool? FlagWishlist { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public decimal? AdditionalShippingCharge { get; set; }
        public string PermaLink { get; set; }
        public int ProductRating { get; set; }
        public int ProductRatingCount { get; set; }
    }

    public class ProductDTOVariant
    {
        public int? ProductId { get; set; }
        public int ProductTypeId { get; set; }
        public int Category { get; set; }
        public int Manufacturer { get; set; }
        public string? PictureName { get; set; }
        public string Name { get; set; }
        public string? ShortDescription { get; set; }
        public string? FullDescription { get; set; }
        public bool ShowOnHomePage { get; set; }
        public string? MetaKeywords { get; set; }
        public string? MetaDescription { get; set; }
        public string? MetaTitle { get; set; }
        public bool? SubjectToAcl { get; set; }
        public bool? LimitedToStores { get; set; }
        public string? Sku { get; set; }
        public string? ManufacturerPartNumber { get; set; }
        public string? Gtin { get; set; }
        public bool IsGiftCard { get; set; }
        public int GiftCardTypeId { get; set; }
        public string? Weight { get; set; }
        public string? Length { get; set; }
        public string? Width { get; set; }
        public string? Height { get; set; }
        public string? Color { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool IsDeleted { get; set; }
        public string? Size1 { get; set; }
        public string? Size2 { get; set; }
        public string? Size3 { get; set; }
        public string? Size4 { get; set; }
        public string? Size5 { get; set; }
        public string? Size6 { get; set; }
        public List<string> Options { get; set; }
        public List<Variants> Variants { get; set; }
        public decimal NewPrice { get; set; }
        public decimal? NewSpecialPrice { get; set; }
        public string? NewSpecialPriceDescription { get; set; }
        public DateTime NewPriceStartTime { get; set; }
        public DateTime NewPriceEndTime { get; set; }
        public decimal NewAdditionalTax { get; set; }
        public int StoreId { get; set; }
        public List<int>? BranchIdList { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public System.DateTime UpdatedOnUtc { get; set; }
        public int BranchId { get; set; }
        public int NewDeliveryTime { get; set; }
        public bool? FlagSharedInfo { get; set; }
    }

    public class Variants
    {
        public List<string> Combination { get; set; }
        public decimal NewPriceVariant { get; set; }
        public decimal NewSpecialPriceVariant { get; set; }

    }

    public class ProductDetailModelWithVariant
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string FullDescription { get; set; }
        public int Manufacturer { get; set; }
        public string BrandName { get; set; }
        public List<StorePricingModel> StorePricingModel { get; set; }
        public List<string> ProductImages { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string Weight { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Color { get; set; }
        public string Size1 { get; set; }
        public string Size2 { get; set; }
        public string Size3 { get; set; }
        public string Size4 { get; set; }
        public string Size5 { get; set; }
        public string Size6 { get; set; }
        public string AndroidInformation1 { get; set; }
        public int Category { get; set; }
        public string CategoryName { get; set; }
        public string SubCategoryPermaLink { get; set; }
        public int? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public string ParentCategoryPermaLink { get; set; }
        public string CategoryGroupTag { get; set; }
        public string PermaLink { get; set; }  
        public List<ProductModelWithCategory> RelatedProductList { get; set; }
        public List<string> VariantOptions { get; set; }
        public List<VariantsDetail> ProductVariants { get; set; }
        public List<string> option1 { get; set; }
        public List<string> option2 { get; set; }
        public List<string> option3 { get; set; }
        public int ProductRating { get; set; }
        public int ProductRatingCount { get; set; }
        public List<StorePricingModel> StorePricingModelForVbuy { get; set; }       

    }

    public class VariantsDetail
    {
        public int ProductVariantId { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
    }

    public class ProductDTOWithPrice
    {
        public int ProductId { get; set; }
        public int ProductTypeId { get; set; }
        public int Category { get; set; }
        public int Manufacturer { get; set; }
        public string? PictureName { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string FullDescription { get; set; }
        public bool ShowOnHomePage { get; set; }
        public string MetaKeywords { get; set; }
        public string MetaDescription { get; set; }
        public string? MetaTitle { get; set; }
        public bool? SubjectToAcl { get; set; }
        public bool? LimitedToStores { get; set; }
        public string Sku { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string Gtin { get; set; }
        public bool IsGiftCard { get; set; }
        public int GiftCardTypeId { get; set; }
        public string Weight { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Color { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool IsDeleted { get; set; }
        public string Size1 { get; set; }
        public string Size2 { get; set; }
        public string Size3 { get; set; }
        public string Size4 { get; set; }
        public string Size5 { get; set; }
        public string Size6 { get; set; }
        public decimal NewPrice { get; set; }
        public decimal NewSpecialPrice { get; set; }
        public string NewSpecialPriceDescription { get; set; }
        public DateTime NewPriceStartTime { get; set; }
        public DateTime NewPriceEndTime { get; set; }
        public decimal NewAdditionalTax { get; set; }
        public int StoreId { get; set; }
        public List<int>? BranchIdList { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public System.DateTime UpdatedOnUtc { get; set; }
        public decimal NewShippingCharge { get; set; }
        public int NewDeliveryTime { get; set; }
    }
}
