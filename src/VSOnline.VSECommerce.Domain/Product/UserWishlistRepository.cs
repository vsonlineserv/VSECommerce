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
    public class UserWishlistRepository
    {
        private readonly DataContext _context;
        private readonly UserActionHelper _userActionHelper;
        public UserWishlistRepository(DataContext context, UserActionHelper userActionHelper)
        {
            _context = context;
            _userActionHelper = userActionHelper;
        }

        public List<UserWishlistResult> AddUserWishList(int userid, int productId, int? branchId)
        {
            var selectedUserWishList = _context.UserWishlist.Where(x => x.User == userid && x.Product == productId).ToList<UserWishlist>();
            if (branchId > 0)
            {
                selectedUserWishList = _context.UserWishlist.Where(x => x.User == userid && x.Product == productId && x.BranchId == branchId).ToList<UserWishlist>();
            }
            if (selectedUserWishList == null || selectedUserWishList.Count == 0)
            {
                UserWishlist wishList = new UserWishlist();
                wishList.User = userid;
                wishList.Product = productId;
                wishList.BranchId = branchId;
                wishList.CreatedOnUtc = DateTime.UtcNow;
                _context.UserWishlist.Add(wishList);
                _context.SaveChanges();
            }
            var userList = _context.UserWishlist.Where(x => x.User == userid).ToList<UserWishlist>();
            return _userActionHelper.GetUserWishlistModelFromUserWishlist(userList);
        }
        public List<UserWishlistResult> RemoveUserWishList(int userid, int productId, int? branchId)
        {
            var userWishList = _context.UserWishlist.Where(x => x.User == userid && x.Product == productId).ToList<UserWishlist>();

            if (branchId > 0)
            {
                userWishList = _context.UserWishlist.Where(x => x.User == userid && x.Product == productId && x.BranchId == branchId).ToList<UserWishlist>();
            }
            foreach (UserWishlist product in userWishList)
            {
                _context.UserWishlist.Remove(product);
            }
            _context.SaveChanges();
            var userList = _context.UserWishlist.Where(x => x.User == userid).ToList<UserWishlist>();
            return _userActionHelper.GetUserWishlistModelFromUserWishlist(userList);
        }
    }
}
