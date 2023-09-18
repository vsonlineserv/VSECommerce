using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Domain
{
    public class SellerRepository
    {
        private readonly DataContext _context;
        private readonly RatingHelper _ratingHelper;
        public SellerRepository(DataContext context, RatingHelper ratingHelper)
        {
            _context = context;
            _ratingHelper = ratingHelper;
        }
        public RetailerInfoResult GetStoreDetails(Seller seller)
        {
            var retailerInfo = new RetailerInfoResult
            {
                StoreId = seller.StoreId,
                StoreName = seller.StoreName,
                Description = seller.Description,
                LogoPicture = seller.LogoPicture
            };

            retailerInfo.Branches = new List<BranchResults>();

            List<SellerBranch> sellerBranches = seller.Branches.ToList<SellerBranch>();
            foreach (SellerBranch branch in sellerBranches)
            {
                BranchResults newBranch = new BranchResults();
                newBranch.BranchId = branch.BranchId;
                newBranch.BranchName = branch.BranchName;
                newBranch.Address1 = branch.Address1;
                newBranch.Address2 = branch.Address2;
                newBranch.City = branch.City;
                newBranch.State = branch.State;
                newBranch.PostalCode = branch.PostalCode;
                newBranch.PhoneNumber = branch.PhoneNumber;
                newBranch.Email = branch.Email;
                newBranch.EnableBuy = branch.EnableBuy;
                newBranch.Latitude = branch.Latitude;
                newBranch.Longitude = branch.Longitude;
                newBranch.BranchRating = _ratingHelper.CalulateSellerRating(branch.BranchId);
                newBranch.RatingCount = _context.SellerRating.Where(x => x.BranchId == branch.BranchId).Count();
                newBranch.StoreType = branch.StoreType;
                retailerInfo.Branches.Add(newBranch);
            }
            return retailerInfo;
        }

        public RetailerInfoResult GetStoreDetailsForStaff(SellerStaffMapping sellerStaffMapping)
        {
            var retailerInfo = new RetailerInfoResult
            {
                StoreId = sellerStaffMapping.StoreId,
            };

            retailerInfo.Branches = new List<BranchResults>();

            List<SellerBranch> sellerBranches = sellerStaffMapping.Branches.ToList<SellerBranch>();
            foreach (SellerBranch branch in sellerBranches)
            {
                BranchResults newBranch = new BranchResults();
                newBranch.BranchId = branch.BranchId;
                newBranch.BranchName = branch.BranchName;
                newBranch.Address1 = branch.Address1;
                newBranch.Address2 = branch.Address2;
                newBranch.City = branch.City;
                newBranch.State = branch.State;
                newBranch.PostalCode = branch.PostalCode;
                newBranch.PhoneNumber = branch.PhoneNumber;
                newBranch.Email = branch.Email;
                newBranch.EnableBuy = branch.EnableBuy;
                newBranch.Latitude = branch.Latitude;
                newBranch.Longitude = branch.Longitude;
                retailerInfo.Branches.Add(newBranch);
            }
            return retailerInfo;
        }

        public RetailerInfoResult GetRetailerInfo(string currentUser)
        {
            var userId = Convert.ToInt64(currentUser);
            var seller = _context.Seller.Where(x => x.PrimaryContact == userId).Include(y => y.Branches).FirstOrDefault<Seller>();

            return GetStoreDetails(seller);

        }
        public string GetAllSearchAreaQuery()
        {
            string query = @"SELECT [City] ,[AreaName],[Latitude],[Longitude]
                    FROM [dbo].[Area]";
            return query;
        }

        // for Hyperlocal
        public List<RetailerInfoResult> GetStoreDetailsForVbuy(List<Seller> seller)
        {
            List<RetailerInfoResult> retailerInfoResultsList = new List<RetailerInfoResult>();
            foreach (var eachStore in seller)
            {
                RetailerInfoResult retailerInfoResult = new RetailerInfoResult();
                retailerInfoResult.StoreId = eachStore.StoreId;
                retailerInfoResult.StoreName = eachStore.StoreName;
                retailerInfoResult.Description = eachStore.Description;
                retailerInfoResult.LogoPicture = eachStore.LogoPicture;
                retailerInfoResult.Branches = new List<BranchResults>();
                var branchDetails = _context.SellerBranch.Where(x => x.Store == eachStore.StoreId).ToList();
                if (branchDetails.Count > 0)
                {
                    foreach (var branch in branchDetails)
                    {
                        BranchResults newBranch = new BranchResults();
                        newBranch.BranchId = branch.BranchId;
                        newBranch.BranchName = branch.BranchName;
                        newBranch.Address1 = branch.Address1;
                        newBranch.Address2 = branch.Address2;
                        newBranch.City = branch.City;
                        newBranch.State = branch.State;
                        newBranch.PostalCode = branch.PostalCode;
                        newBranch.PhoneNumber = branch.PhoneNumber;
                        newBranch.Email = branch.Email;
                        newBranch.EnableBuy = branch.EnableBuy;
                        newBranch.Latitude = branch.Latitude;
                        newBranch.Longitude = branch.Longitude;
                        newBranch.BranchRating = _ratingHelper.CalulateSellerRating(branch.BranchId);
                        newBranch.RatingCount = _context.SellerRating.Where(x => x.BranchId == branch.BranchId).Count();
                        newBranch.StoreType = branch.StoreType;
                        if (!string.IsNullOrEmpty(newBranch.StoreType))
                        {
                            newBranch.StoreType = newBranch.StoreType.Replace("_", " ");
                        }
                        retailerInfoResult.Branches.Add(newBranch);
                    }
                }
                retailerInfoResultsList.Add(retailerInfoResult);
            }
            return retailerInfoResultsList;
        }

        public RetailerInfoResult GetStoreDetailsForTemplate(Seller seller)
        {
            var retailerInfo = new RetailerInfoResult
            {
                StoreId = seller.StoreId,
                StoreName = seller.StoreName,
                Description = seller.Description
            };

            retailerInfo.Branches = new List<BranchResults>();

            List<SellerBranch> sellerBranches = seller.Branches.ToList<SellerBranch>();
            foreach (SellerBranch branch in sellerBranches)
            {
                BranchResults newBranch = new BranchResults();
                newBranch.BranchId = branch.BranchId;
                newBranch.BranchName = branch.BranchName;
                newBranch.EnableBuy = branch.EnableBuy;
                retailerInfo.Branches.Add(newBranch);
            }
            return retailerInfo;
        }
    }
}
