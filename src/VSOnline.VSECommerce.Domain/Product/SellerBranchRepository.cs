using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain
{
    public class SellerBranchRepository
    {
        private readonly DataContext _context;
        private IMapper _mapper;
        public SellerBranchRepository(IMapper mapper, DataContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public string GetStoresWithinAreaQuery(int StoreId,decimal? latitude, decimal? longitude, int? radius)
        {   
            if (latitude > 0 && longitude > 0 && radius > 0 && StoreId > 0)
            {
                //Earth Radius 6371 KM. 
                string query = @"SELECT BranchId,Store,
                    ACOS( SIN( RADIANS( Latitude ) ) * SIN( RADIANS( {lat} ) ) + COS( RADIANS( Latitude ) )
                    * COS( RADIANS( {lat} )) * COS( RADIANS( Longitude ) - RADIANS( {lng} )) ) * 6380 AS 'Distance'
                    FROM SellerBranch
                    WHERE
                    ACOS( SIN( RADIANS( Latitude) ) * SIN( RADIANS( {lat} ) ) + COS( RADIANS( Latitude ) )
                    * COS( RADIANS( {lat} )) * COS( RADIANS( Longitude ) - RADIANS( {lng} )) ) * 6371 < {radius}
                   and Store = {StoreId} ORDER BY 'Distance'".FormatWith(new { lat = latitude, lng = longitude, radius, StoreId });
                return query;
            }
            else if ((latitude == null && longitude == null && radius == null) ||(latitude == 0 && longitude == 0 && radius == 0) && StoreId > 0)
            {
                string query = @"SELECT BranchId,Store
                    FROM SellerBranch where Store = {StoreId}".FormatWith(new { StoreId });
                return query;
            }
            else if((latitude <= 90 && latitude >=-90)&& (longitude <= 180 && longitude >= -180) && radius > 0 && StoreId == 0)
            {
                string query = @"SELECT BranchId,Store,
                    ACOS( SIN( RADIANS( Latitude ) ) * SIN( RADIANS( {lat} ) ) + COS( RADIANS( Latitude ) )
                    * COS( RADIANS( {lat} )) * COS( RADIANS( Longitude ) - RADIANS( {lng} )) ) * 6380 AS 'Distance'
                    FROM SellerBranch
                    WHERE
                    ACOS( SIN( RADIANS( Latitude) ) * SIN( RADIANS( {lat} ) ) + COS( RADIANS( Latitude ) )
                    * COS( RADIANS( {lat} )) * COS( RADIANS( Longitude ) - RADIANS( {lng} )) ) * 6371 < {radius}
                    and FlagvBuy = 'true'
                    ORDER BY 'Distance'".FormatWith(new { lat = latitude, lng = longitude, radius, StoreId });
                return query;
            }
            else
            {
                string query = @"SELECT BranchId,Store FROM SellerBranch";
                return query;
            }
        }
        public List<RetailerLocationMapResult> GetStoreLocations(IEnumerable<int> branchIdList)
        {
            var sellerBranches = _context.SellerBranch.Where(x => branchIdList.Contains(x.BranchId)).Include(y => y.SellerMap).ToList<SellerBranch>();

            List<RetailerLocationMapResult> retailerLocationMapDTOList = new List<RetailerLocationMapResult>();

            _mapper.Map<IEnumerable<SellerBranch>, IEnumerable<RetailerLocationMapResult>>(sellerBranches, retailerLocationMapDTOList);

            return retailerLocationMapDTOList;
        }
    }
}
