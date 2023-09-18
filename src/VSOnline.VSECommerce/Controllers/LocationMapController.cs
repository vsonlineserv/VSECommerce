using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]

    [ApiController]
    public class LocationMapController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly SellerBranchRepository _sellerBranchRepository;
        private readonly EfContext _efContext;
        private readonly ProductRepository _productRepository;
        public LocationMapController(EfContext efContext, DataContext context, SellerBranchRepository sellerBranchRepository, ProductRepository productRepository)
        {
            _context = context;
            _sellerBranchRepository = sellerBranchRepository;
            _efContext = efContext;
            _productRepository = productRepository;
        }

        [HttpGet("Seller/{StoreId}/GetCategoryStoreLocations/{id}/{lat}/{lng}/{mapRadius}")]
        public List<RetailerLocationMapResult> GetCategoryStoreLocations(int StoreId,int id, decimal lat, decimal lng, int mapRadius)
        {
            return GetCategoryStoreLocations(StoreId, id, lat, lng, mapRadius, 0, 0);
        }

        [HttpGet("GetCategoryStoreLocations/{id}/{lat}/{lng}/{mapRadius}/{priceRangeFrom}/{PriceRangeTo}")]
        [HttpGet("Seller/{StoreId}/GetCategoryStoreLocations/{id}/{lat}/{lng}/{mapRadius}/{priceRangeFrom}/{PriceRangeTo}")]
        public List<RetailerLocationMapResult> GetCategoryStoreLocations(int StoreId,int id, decimal lat, decimal lng, int mapRadius, int priceRangeFrom, int PriceRangeTo)
        {
            var catIdList = _context.Category.Where(x => x.ParentCategoryId == id || x.CategoryId == id)
                .Select(x => x.CategoryId)
                .ToList<int>();
            var productIdList = _context.ProductStoreMapping.Where(x => catIdList.Contains(x.Category))
                .Select(x => x.ProductId)
                .ToList<int>();


            var branchIdListLocation = GetBranchIdListBasedOnLocation(StoreId, lat, lng, mapRadius);

            if (priceRangeFrom > 0 && PriceRangeTo > 0)
            {
                var branchIdFilteredList = _context.Pricing.Where(x => productIdList.Contains(x.Product) && branchIdListLocation.Contains(x.Branch)
                    && x.SpecialPrice >= priceRangeFrom && x.SpecialPrice <= PriceRangeTo)
                    .Select(x => x.Branch)
                    .Distinct()
                    .ToList<int>();
                if (branchIdListLocation.Count > 0)
                {
                    return _sellerBranchRepository.GetStoreLocations(branchIdFilteredList);
                }
                return new List<RetailerLocationMapResult>();
            }

            var branchIdList = _context.Pricing.Where(x => productIdList.Contains(x.Product) && branchIdListLocation.Contains(x.Branch)
          && x.SpecialPrice >= priceRangeFrom && x.SpecialPrice <= PriceRangeTo)
          .Select(x => x.Branch)
          .Distinct()
          .ToList<int>();

            return _sellerBranchRepository.GetStoreLocations(branchIdList);
        }

        private List<int> GetBranchIdListBasedOnLocation(int StoreId,decimal? lat, decimal? lng, int? mapRadius)
        {
            var query = _sellerBranchRepository.GetStoresWithinAreaQuery(StoreId, lat, lng, mapRadius);
            var branchIdListLocation = _efContext.Database.SqlQuery<LocationBoundaryResult>(query)
                .Distinct()
                .Select(x => x.BranchId)
                .ToList<int>();
            return branchIdListLocation;
        }

        [HttpGet("Seller/{StoreId}/GetWishlistStoreLocations/{lat}/{lng}/{mapRadius}")]
        public List<RetailerLocationMapResult> GetWishlistStoreLocations(int StoreId,decimal lat, decimal lng, int mapRadius)
        {
            var branchIdListLocation = GetBranchIdListBasedOnLocation(StoreId,lat, lng, mapRadius);

            var curUserId = 0;
            var currentUser = User.Identity.Name;
            var currentUserId = User.FindFirst("UserId")?.Value;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                curUserId = Convert.ToInt32(currentUserId);
            }

            var productList = _context.UserWishlist.Where(x => x.User == curUserId).Select(x => x.Product);


            var branchIdList = _context.Pricing.Where(x => productList.Contains(x.Product) && branchIdListLocation.Contains(x.Branch))
                .Select(x => x.Branch)
                .Distinct()
                .ToList<int>();

            return _sellerBranchRepository.GetStoreLocations(branchIdList);
        }

        //for vbuy
        [HttpGet("GetSearchStoreLocations/{productFilter}/{priceRangeFrom}/{PriceRangeTo}")]
        public IActionResult GetSearchStoreLocations(string productFilter, int priceRangeFrom, int PriceRangeTo)
        {
            return GetSearchStoreLocations(productFilter, null, null, null, priceRangeFrom, PriceRangeTo);
        }

        [HttpGet("GetSearchStoreLocations/{productFilter}/{lat}/{lng}/{mapRadius}/{priceRangeFrom}/{PriceRangeTo}")]
        public IActionResult GetSearchStoreLocations(string productFilter, decimal? lat, decimal? lng, int? mapRadius, int priceRangeFrom, int PriceRangeTo)
        {
            try
            {
                var branchIdListLocation = GetBranchIdListBasedOnLocation(0,lat, lng, mapRadius);
                var productIdList = _productRepository.SearchCatalogue(productFilter, branchIdListLocation).Select(x => x.ProductId).ToList();

                if (priceRangeFrom > 0 && PriceRangeTo > 0)
                {
                    var branchIdFilteredList = _context.Pricing.Where(x => productIdList.Contains(x.Product) && branchIdListLocation.Contains(x.Branch)
                        && x.SpecialPrice >= priceRangeFrom && x.SpecialPrice <= PriceRangeTo)
                        .Select(x => x.Branch)
                        .Distinct()
                        .ToList<int>();
                    if (branchIdListLocation.Count > 0)
                    {
                        var storeLocations =  _sellerBranchRepository.GetStoreLocations(branchIdFilteredList);
                        return Ok(storeLocations);
                    }
                    return Ok(new List<RetailerLocationMapResult>());
                }

                var branchIdList = _context.Pricing.Where(x => productIdList.Contains(x.Product) && branchIdListLocation.Contains(x.Branch))
                   .Select(x => x.Branch)
                   .Distinct()
                   .ToList<int>();
                var storeLocationsDetail = _sellerBranchRepository.GetStoreLocations(branchIdList);
                return Ok(storeLocationsDetail);
            }
            catch(Exception ex)
            {
                return StatusCode(500, "There is some error");
            }

        }
    }
}
