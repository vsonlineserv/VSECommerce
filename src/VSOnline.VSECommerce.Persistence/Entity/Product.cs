using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Persistence.Entity
{
    public class Product
    {
        public Product()
        {
            this.PricingCollection = new HashSet<Pricing>();
        }
        [Key]
        public int ProductId { get; set; }
        public int ProductTypeId { get; set; }
        public int Category { get; set; }
        public int? Manufacturer { get; set; }
        public List<ProductImageDetails> ProductImages { get; set; }
        public string? Name { get; set; }
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
        public bool? IsGiftCard { get; set; }
        public int? GiftCardTypeId { get; set; }
        public string? Weight { get; set; }
        public string? Length { get; set; }
        public string? Width { get; set; }
        public string? Height { get; set; }
        public string? Color { get; set; }

        public string? Size1 { get; set; }
        public string? Size2 { get; set; }
        public string? Size3 { get; set; }
        public string? Size4 { get; set; }
        public string? Size5 { get; set; }
        public string? Size6 { get; set; }

        public int? DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool? IsDeleted { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public System.DateTime UpdatedOnUtc { get; set; }

        public string? ProductDescriptionHtml { get; set; }
        [ForeignKey("Manufacturer")]
        public virtual Manufacturer ManufacturerDetails { get; set; }
        [ForeignKey("Category")]
        public virtual Category CategoryDetails { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<Pricing> PricingCollection { get; set; }
        public string? PermaLink { get; set; }
        public List<VariantsPricing> VariantsPricing { get; set; }
        public int? StoreId { get; set; }
        public int? BranchId { get; set; }
        public string? CreatedBy { get; set; }
        public bool? FlagSharedInfo { get; set; }
        public bool? FlagSampleProducts {get;set;}
    }

    public class ProductImageDetails
    {
        public int Id { get; set; }
        public string? PictureName { get; set; }
    }

    public class VariantsPricing
    {
        [Key]
        public int PricingId { get; set; }
        public int? ProductVariantId { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> SpecialPrice { get; set; }
        public string? SpecialPriceDescription { get; set; }
        public int Store { get; set; }
        public options ProductVariant { get; set; }

    }
    public class options
    {
        [Key]
        public string option1 { get; set; }
        public string option2 { get; set; }
        public string option3 { get; set; }
    }
}
