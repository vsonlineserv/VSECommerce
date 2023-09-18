using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.ResultSet
{
    public class SearchProductResult
    {
        public string BrandName { get; set; }
        public int ManufacturerId { get; set; }
        public string? ManufacturerPartNumber { get; set; }
        public string ShortDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? ProductDescriptionHtml { get; set; }
        public decimal? Price { get; set; }
        public decimal? SpecialPrice { get; set; }
        public int ProductId { get; set; }
        public string? PictureName { get; set; }
        public string Name { get; set; }
        public bool Published { get; set; }
        public bool IsDeleted { get; set; }
        public int? AvailableQuantity { get; set; }
        public bool? FlagTrackQuantity { get; set; }
        public bool? ShowOnHomePage { get; set; }
        public string? PermaLink { get; set; }


    }
}
