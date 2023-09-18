using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Helper
{
    public class RatingHelper
    {
        private readonly DataContext _context;
        public RatingHelper(DataContext dataContext)
        {
            _context = dataContext;
        }
        public string _GetProductRating(int productId)
        {
            var query = @"SELECT [ProductId], Rating, Count(*) RatingCount from ProductRating
                        WHERE ProductId = {productId}
                        GROUP BY [ProductId], Rating".FormatWith(new { productId = productId });
            return query;
        }
        public List<ProductRatingResult> GetProductRating(int productId)
        {
            var productRating = _context.ProductRating.Where(x => x.ProductId == productId).GroupBy(x => new { Rating = x.Rating, ProductId = x.ProductId })
                       .Select(x => new ProductRatingResult
                       {
                           Rating = x.Key.Rating,
                           ProductId = x.Key.ProductId,
                           RatingCount = x.Count()
                       }).ToList();
            return productRating;
        }
        public string _GetSellerRating(int branchid)
        {
            var query = @"SELECT BranchId, Rating, Count(*) RatingCount from SellerRating
                        WHERE BranchId = {branchid}
                        GROUP BY BranchId,Rating".FormatWith(new { branchid = branchid });
            return query;
        }
        public List<BranchRatingResult> GetSellerRating(int branchId)
        {
            var branchRatingResultSet = _context.SellerRating.Where(x => x.BranchId == branchId).GroupBy(x => new { BranchId = x.BranchId, Rating = x.Rating })
                   .Select(x => new BranchRatingResult
                   {
                       Rating = x.Key.Rating,
                       BranchId = x.Key.BranchId,
                       RatingCount = x.Count()
                   }).ToList();
            return branchRatingResultSet;
        }
        public string _InsertProductRatingQuery(int productId, int rating, int currentUserId, int lowest, int highest)
        {
            int verifiedRating = rating < lowest ? lowest : rating;
            verifiedRating = verifiedRating > highest ? highest : rating;

            var query = @"INSERT INTO [ProductRating]
           ([ProductId]
           ,[Rating]
           , [User]
           ,[UpdatedOnUtc])
            VALUES
           (@productId
           ,{rating}
           , {user}
           ,'{date}')".FormatWith(new { productId = productId, rating = verifiedRating, user = currentUserId, date = DateTime.UtcNow.ToString("yyyy-MM-dd") });
            return query;
        }
        public void InsertProductRatingQuery(int productId, int rating, int currentUserId, int lowest, int highest)
        {
            int verifiedRating = rating < lowest ? lowest : rating;
            verifiedRating = verifiedRating > highest ? highest : rating;

            ProductRating productRating = new ProductRating();
            productRating.Rating = verifiedRating;
            productRating.User = currentUserId;
            productRating.ProductId = productId;
            productRating.UpdatedOnUtc = DateTime.UtcNow;
             _context.ProductRating.Add(productRating);
            _context.SaveChanges();
        }

        public int CalulateProductRating(int? productId)
        {
            var rating = 0;
            var totalCount = 0;
            var productRating = _context.ProductRating.Where(x => x.ProductId == productId).GroupBy(x => new { Rating = x.Rating, ProductId = x.ProductId })
                       .Select(x => new ProductRatingResult
                       {
                           Rating = x.Key.Rating,
                           ProductId = x.Key.ProductId,
                           RatingCount = x.Count()
                       }).ToList();

            foreach (var ratingItem in productRating)
            {
                rating = rating + (ratingItem.Rating * ratingItem.RatingCount);
                totalCount = totalCount + ratingItem.RatingCount;
            }
            if(rating > 0 && totalCount > 0)
            {
                return rating / totalCount;

            }
            return 0;
        }

        public void InsertSellerRating(int branchId, int rating, int currentUserId, int lowest, int highest)
        {
            int verifiedRating = rating < lowest ? lowest : rating;
            verifiedRating = verifiedRating > highest ? highest : rating;

            SellerRating sellerRating = new SellerRating();
            sellerRating.BranchId = branchId;
            sellerRating.Rating = verifiedRating;
            sellerRating.UpdatedOnUtc = DateTime.UtcNow;
            _context.SellerRating.Add(sellerRating);
            _context.SaveChanges();
        }

        public int CalulateSellerRating(int branchId)
        {
            var rating = 0;
            var totalCount = 0;
            var branchRatingResultSet = _context.SellerRating.Where(x => x.BranchId == branchId).GroupBy(x => new { BranchId = x.BranchId, Rating = x.Rating })
                   .Select(x => new BranchRatingResult
                   {
                       Rating = x.Key.Rating,
                       BranchId = x.Key.BranchId,
                       RatingCount = x.Count()
                   }).ToList();
            foreach (var ratingItem in branchRatingResultSet)
            {
                rating = rating + (ratingItem.Rating * ratingItem.RatingCount);
                totalCount = totalCount + ratingItem.RatingCount;
            }
            if (rating > 0 && totalCount > 0)
            {
                return rating / totalCount;

            }
            return 0;
        }

    }
}
