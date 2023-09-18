using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Domain.Mapping
{
    public class ObjectMapper : Profile
    {
        public ObjectMapper()
        {
            CreateMap<CategoryResult, Category>();
            CreateMap<Category, CategoryResult>();
            CreateMap<Product, ProductDTO>().
                 ForSourceMember(x => x.ManufacturerDetails, t => t.DoNotValidate())
                .ForMember(x => x.PictureName, t => t.Ignore());
            CreateMap<ProductDTO, Product>().
               ForMember(x => x.ManufacturerDetails, t => t.Ignore())
               .ForMember(x => x.ProductDescriptionHtml, t => t.Ignore())
               .ForMember(x => x.CategoryDetails, t => t.Ignore())
               .ForMember(x => x.PricingCollection, t => t.Ignore())
               .ForMember(x => x.Size1, t => t.Ignore())
               .ForMember(x => x.Size2, t => t.Ignore())
               .ForMember(x => x.Size3, t => t.Ignore())
               .ForMember(x => x.Size4, t => t.Ignore())
               .ForMember(x => x.Size5, t => t.Ignore())
               .ForMember(x => x.ProductImages, t => t.Ignore())
               .ForMember(x => x.Size6, t => t.Ignore())
               .ForMember(x => x.PermaLink, t => t.Ignore())
               .ForMember(x => x.VariantsPricing, t => t.Ignore());

            CreateMap<Product, ProductModel>()
              .ForSourceMember(x => x.ManufacturerDetails, t => t.DoNotValidate())
              .ForSourceMember(x => x.PricingCollection, t => t.DoNotValidate())
              .ForSourceMember(x => x.CategoryDetails, t => t.DoNotValidate())
              .ForSourceMember(x => x.ProductDescriptionHtml, t => t.DoNotValidate())
              .ForMember(x => x.FlagWishlist, t => t.Ignore())
              .ForMember(x => x.PictureName, t => t.Ignore())
              .ForMember(x => x.Price, o => o.MapFrom(x => x.PricingCollection.Min(y => y.Price)))
              .ForMember(x => x.SpecialPrice, o => o.MapFrom(x => x.PricingCollection.Min(y => y.SpecialPrice)))
              .ForMember(x => x.StoresCount, o => o.MapFrom(x => x.PricingCollection.Count()));

            CreateMap<Product, ProductModelForBranchCatalog>()
         .ForSourceMember(x => x.ManufacturerDetails, t => t.DoNotValidate())
         .ForSourceMember(x => x.PricingCollection, t => t.DoNotValidate())
         .ForSourceMember(x => x.CategoryDetails, t => t.DoNotValidate())
         .ForSourceMember(x => x.ProductDescriptionHtml, t => t.DoNotValidate())
         .ForMember(x => x.FlagExist, t => t.Ignore())
          .ForMember(x => x.PictureName, t => t.Ignore())
         .ForMember(x => x.FlagWishlist, t => t.Ignore())
         .ForMember(x => x.Price, o => o.MapFrom(x => x.PricingCollection.Min(y => y.SpecialPrice)))
         .ForMember(x => x.BranchList, o => o.MapFrom(x => x.PricingCollection.Select(z => z.Branch)))
         .ForMember(x => x.StoresCount, o => o.MapFrom(x => x.PricingCollection.Count()));

            CreateMap<Product, ProductModelWithCategory>()
               .ForSourceMember(x => x.PricingCollection, t => t.DoNotValidate())
               .ForSourceMember(x => x.CategoryDetails, t => t.DoNotValidate())
               .ForMember(x => x.ParentCategoryName, t => t.Ignore())
               .ForMember(x => x.TotalCount, t => t.Ignore())
               .ForMember(x => x.FlagWishlist, t => t.Ignore())
                .ForMember(x => x.PictureName, t => t.Ignore())
                .ForMember(x => x.BranchId, t => t.Ignore())
                .ForMember(x => x.AdditionalShippingCharge, t => t.Ignore())
                .ForMember(x => x.BranchName, t => t.Ignore())
                 .ForMember(x => x.DeliveryTime, t => t.Ignore())
               .ForMember(x => x.SubCategoryId, o => o.MapFrom(x => x.CategoryDetails.CategoryId))
               .ForMember(x => x.SubCategoryName, o => o.MapFrom(x => x.CategoryDetails.Name))
               .ForMember(x => x.CategoryId, o => o.MapFrom(x => x.CategoryDetails.ParentCategoryId))
               .ForMember(x => x.Price, o => o.MapFrom(x => x.PricingCollection.Max(y => y.Price)))
               .ForMember(x => x.SpecialPrice, o => o.MapFrom(x => x.PricingCollection.Min(y => y.SpecialPrice)))
               .ForMember(x => x.StoresCount, o => o.MapFrom(x => x.PricingCollection.Count()));

            CreateMap<Product, ProductDetailModel>()
                .ForSourceMember(x => x.ManufacturerDetails, t => t.DoNotValidate())
                .ForMember(x => x.BrandName, t => t.Ignore())
                  .ForMember(x => x.ProductImages, t => t.Ignore())
                .ForMember(x => x.AndroidInformation1, t => t.Ignore())
                .ForMember(x => x.CategoryName, t => t.Ignore())
                .ForMember(x => x.CategoryGroupTag, t => t.Ignore())
                .ForMember(x => x.RelatedProductList, t => t.Ignore())
                .ForMember(x => x.ParentCategoryId, t => t.Ignore())
                .ForMember(x => x.ParentCategoryName, t => t.Ignore())
                .ForMember(x => x.StorePricingModel, t => t.Ignore());

            CreateMap<UserResult, User>()
                .ForMember(x => x.UserId, t => t.Ignore())
                .ForMember(x => x.PasswordFormatId, t => t.Ignore())
             .ForMember(x => x.Password, t => t.Ignore())
             .ForMember(x => x.PasswordSalt, t => t.Ignore())
             .ForMember(x => x.UserId, t => t.Ignore())
             .ForMember(x => x.UserGuid, t => t.Ignore())
             .ForMember(x => x.IsSupport, t => t.Ignore())
             .ForMember(x => x.IsSales, t => t.Ignore())
             .ForMember(x => x.IsMarketing, t => t.Ignore())
             .ForMember(x => x.IsSuperAdmin, t => t.Ignore())
             .ForMember(x => x.IsAdmin, t => t.Ignore());

            CreateMap<Pricing, RetailerPricingModel>()
                .ForMember(x => x.ProductName, opt => opt.MapFrom(src => src.ProductDetails.Name))
                .ForMember(x => x.BranchName, opt => opt.MapFrom(src => src.BranchDetails.BranchName))
                .ForMember(x => x.ProductId, opt => opt.MapFrom(src => src.Product))
                .ForMember(x => x.BranchId, opt => opt.MapFrom(src => src.Branch))
                .ForMember(x => x.PriceStartTime, opt => opt.MapFrom(src => src.SpecialPriceStartDateTimeUtc))
                .ForMember(x => x.PriceEndTime, opt => opt.MapFrom(src => src.SpecialPriceEndDateTimeUtc))
                .ForMember(x => x.PictureName, t => t.Ignore());

            CreateMap<SellerBranch, RetailerLocationMapResult>()
                .ForMember(x => x.StoreId, opt => opt.MapFrom(src => src.SellerMap.StoreId))
                .ForMember(x => x.StoreName, opt => opt.MapFrom(src => src.SellerMap.StoreName));

            CreateMap<UserWishlist, UserWishlistResult>()
                  .ForSourceMember(x => x.Product, t => t.DoNotValidate())
                .ForSourceMember(x => x.User, t => t.DoNotValidate());
            CreateMap<UserWishlistResult, UserWishlist>()
                .ForMember(x => x.Product, t => t.Ignore())
                .ForMember(x => x.User, t => t.Ignore());

            CreateMap<Manufacturer, BrandFilterDTO>()
              .ForMember(x => x.Id, opt => opt.MapFrom(src => src.ManufacturerId))
                .ForMember(x => x.BrandName, opt => opt.MapFrom(src => src.Name));

            CreateMap<OrderDTO, OrderProduct>()
                .ForMember(x => x.OrderProductItem, t => t.Ignore())
                .ForMember(x => x.OrderCancel, t => t.Ignore())
                .ForSourceMember(x => x.OrderStatus, t => t.DoNotValidate());

            CreateMap<OrderProduct, OrderDTO>()
                .ForSourceMember(x => x.OrderProductItem, t => t.DoNotValidate())
                 .ForMember(x => x.OrderStatus, t => t.Ignore());

            CreateMap<OrderProductItem, OrderItemResult>()
                .ForMember(x => x.Name, opt => opt.MapFrom(src => src.ProductMap.Name))
                .ForMember(x => x.StoreId, opt => opt.MapFrom(src => src.BranchId))
                .ForMember(x => x.BranchId, opt => opt.MapFrom(src => src.BranchId))
                .ForMember(x => x.Branch, opt => opt.MapFrom(src => src.SellerBranchMap.BranchName))
                .ForMember(x => x.Quantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(x => x.UnitPrice, opt => opt.MapFrom(src => src.UnitPriceInclTax))
                .ForMember(x => x.SpecialPrice, opt => opt.MapFrom(src => src.UnitPriceInclTax))
                 .ForMember(x => x.Price, opt => opt.MapFrom(src => src.PriceInclTax))
                 .ForMember(x => x.PictureName, t => t.Ignore())
                       .ForMember(x => x.CustomerId, t => t.Ignore());

            CreateMap<ProductSpecification, ProductSpecificationResult>()
                .ForSourceMember(x => x.DisplayOrder, t => t.DoNotValidate());


            CreateMap<ProductDTOVariant, Product>().
              ForMember(x => x.ManufacturerDetails, t => t.Ignore())
              .ForMember(x => x.ProductDescriptionHtml, t => t.Ignore())
              .ForMember(x => x.CategoryDetails, t => t.Ignore())
              .ForMember(x => x.PricingCollection, t => t.Ignore())
              .ForMember(x => x.Size1, t => t.Ignore())
              .ForMember(x => x.Size2, t => t.Ignore())
              .ForMember(x => x.Size3, t => t.Ignore())
              .ForMember(x => x.Size4, t => t.Ignore())
              .ForMember(x => x.Size5, t => t.Ignore())
              .ForMember(x => x.ProductImages, t => t.Ignore())
              .ForMember(x => x.Size6, t => t.Ignore())
              .ForMember(x => x.PermaLink, t => t.Ignore())
              .ForMember(x => x.VariantsPricing, t => t.Ignore());

            CreateMap<Product, ProductDetailModelWithVariant>()
             .ForSourceMember(x => x.ManufacturerDetails, t => t.DoNotValidate())
             .ForMember(x => x.BrandName, t => t.Ignore())
             .ForMember(x => x.ProductImages, t => t.Ignore())
             .ForMember(x => x.AndroidInformation1, t => t.Ignore())
             .ForMember(x => x.CategoryName, t => t.Ignore())
             .ForMember(x => x.CategoryGroupTag, t => t.Ignore())
             .ForMember(x => x.RelatedProductList, t => t.Ignore())
             .ForMember(x => x.ParentCategoryId, t => t.Ignore())
             .ForMember(x => x.ParentCategoryName, t => t.Ignore())
             .ForMember(x => x.StorePricingModel, t => t.Ignore())
             .ForMember(x => x.VariantOptions, t => t.Ignore())
             .ForMember(x => x.ProductVariants, t => t.Ignore());

            CreateMap<ProductDTOWithPrice, Product>().
               ForMember(x => x.ManufacturerDetails, t => t.Ignore())
               .ForMember(x => x.ProductDescriptionHtml, t => t.Ignore())
               .ForMember(x => x.CategoryDetails, t => t.Ignore())
               .ForMember(x => x.PricingCollection, t => t.Ignore())
               .ForMember(x => x.Size1, t => t.Ignore())
               .ForMember(x => x.Size2, t => t.Ignore())
               .ForMember(x => x.Size3, t => t.Ignore())
               .ForMember(x => x.Size4, t => t.Ignore())
               .ForMember(x => x.Size5, t => t.Ignore())
               .ForMember(x => x.ProductImages, t => t.Ignore())
               .ForMember(x => x.Size6, t => t.Ignore())
               .ForMember(x => x.PermaLink, t => t.Ignore())
               .ForMember(x => x.VariantsPricing, t => t.Ignore());

            CreateMap<ProductResult, Product>();
            CreateMap<Product, ProductResult>();
            CreateMap<BuyerAddressDTO, BuyerAddressResult>();
            CreateMap<ManufacturerResult, Manufacturer>();
            CreateMap<Manufacturer, ManufacturerResult>();
            CreateMap<ProductStoreMapping, ProductModel>()
              .ForSourceMember(x => x.ManufacturerDetails, t => t.DoNotValidate())
              .ForSourceMember(x => x.PricingCollection, t => t.DoNotValidate())
              .ForSourceMember(x => x.CategoryDetails, t => t.DoNotValidate())
              .ForMember(x => x.FlagWishlist, t => t.Ignore())
              .ForMember(x => x.PictureName, t => t.Ignore())
              .ForMember(x => x.Price, o => o.MapFrom(x => x.PricingCollection.Min(y => y.Price)))
              .ForMember(x => x.SpecialPrice, o => o.MapFrom(x => x.PricingCollection.Min(y => y.SpecialPrice)))
              .ForMember(x => x.StoresCount, o => o.MapFrom(x => x.PricingCollection.Count()));

            CreateMap<ProductStoreMapping, ProductDetailModel>()
                .ForSourceMember(x => x.ManufacturerDetails, t => t.DoNotValidate())
                .ForMember(x => x.BrandName, t => t.Ignore())
                  .ForMember(x => x.ProductImages, t => t.Ignore())
                .ForMember(x => x.AndroidInformation1, t => t.Ignore())
                .ForMember(x => x.CategoryName, t => t.Ignore())
                .ForMember(x => x.CategoryGroupTag, t => t.Ignore())
                .ForMember(x => x.RelatedProductList, t => t.Ignore())
                .ForMember(x => x.ParentCategoryId, t => t.Ignore())
                .ForMember(x => x.ParentCategoryName, t => t.Ignore())
                .ForMember(x => x.StorePricingModel, t => t.Ignore());

            CreateMap<ProductStoreMapping, ProductModelWithCategory>()
                 .ForSourceMember(x => x.PricingCollection, t => t.DoNotValidate())
                 .ForSourceMember(x => x.CategoryDetails, t => t.DoNotValidate())
                 .ForMember(x => x.ParentCategoryName, t => t.Ignore())
                 .ForMember(x => x.TotalCount, t => t.Ignore())
                 .ForMember(x => x.FlagWishlist, t => t.Ignore())
                  .ForMember(x => x.PictureName, t => t.Ignore())
                  .ForMember(x => x.BranchId, t => t.Ignore())
                  .ForMember(x => x.AdditionalShippingCharge, t => t.Ignore())
                  .ForMember(x => x.BranchName, t => t.Ignore())
                   .ForMember(x => x.DeliveryTime, t => t.Ignore())
                 .ForMember(x => x.SubCategoryId, o => o.MapFrom(x => x.CategoryDetails.CategoryId))
                 .ForMember(x => x.SubCategoryName, o => o.MapFrom(x => x.CategoryDetails.Name))
                 .ForMember(x => x.CategoryId, o => o.MapFrom(x => x.CategoryDetails.ParentCategoryId))
                 .ForMember(x => x.Price, o => o.MapFrom(x => x.PricingCollection.Max(y => y.Price)))
                 .ForMember(x => x.SpecialPrice, o => o.MapFrom(x => x.PricingCollection.Min(y => y.SpecialPrice)))
                 .ForMember(x => x.StoresCount, o => o.MapFrom(x => x.PricingCollection.Count()));

            CreateMap<ProductStoreMapping, ProductDetailModelWithVariant>()
                 .ForSourceMember(x => x.ManufacturerDetails, t => t.DoNotValidate())
                 .ForMember(x => x.BrandName, t => t.Ignore())
                 .ForMember(x => x.ProductImages, t => t.Ignore())
                 .ForMember(x => x.AndroidInformation1, t => t.Ignore())
                 .ForMember(x => x.CategoryName, t => t.Ignore())
                 .ForMember(x => x.CategoryGroupTag, t => t.Ignore())
                 .ForMember(x => x.RelatedProductList, t => t.Ignore())
                 .ForMember(x => x.ParentCategoryId, t => t.Ignore())
                 .ForMember(x => x.ParentCategoryName, t => t.Ignore())
                 .ForMember(x => x.StorePricingModel, t => t.Ignore())
                 .ForMember(x => x.VariantOptions, t => t.Ignore())
                 .ForMember(x => x.ProductVariants, t => t.Ignore());

            CreateMap<ProductStoreMapping, ProductResult>();
        }

    }
}
