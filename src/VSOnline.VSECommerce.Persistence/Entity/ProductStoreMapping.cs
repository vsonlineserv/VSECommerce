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
    public class ProductStoreMapping
    {
        public ProductStoreMapping()
        {
            this.PricingCollection = new HashSet<Pricing>();
        }
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public string? FullDescription { get; set; }
        public int? Manufacturer { get; set; }
        public int Category { get; set; }
        public bool ShowOnHomePage { get; set; }
        public bool Published { get; set; }
        public bool? IsDeleted { get; set; }
        public string? PermaLink { get; set; }
        public int StoreId { get; set; }
        public int BranchId { get; set; }
        public System.DateTime CreatedOnUtc { get; set; }
        public System.DateTime? UpdatedOnUtc { get; set; }
        [JsonIgnore]
        [IgnoreDataMember]
        public virtual ICollection<Pricing> PricingCollection { get; set; }
        [ForeignKey("Manufacturer")]
        public virtual Manufacturer ManufacturerDetails { get; set; }
        [ForeignKey("Category")]    
        public virtual Category CategoryDetails { get; set; }
        public bool? FlagSampleProducts { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product ProductDetails { get; set; }
    }
}
