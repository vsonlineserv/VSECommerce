using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Domain.Helper
{
    public class UserActionHelper
    {
        private IMapper _imapper;
        public UserActionHelper(IMapper imapper)
        {
            _imapper = imapper;
        }

        public List<UserWishlistResult> GetUserWishlistModelFromUserWishlist(List<UserWishlist> userWishList)
        {
            List<UserWishlistResult> userWishlistDTO = new List<UserWishlistResult>();
            _imapper.Map<IEnumerable<UserWishlist>,
            IEnumerable<UserWishlistResult>>(userWishList, userWishlistDTO);

            return userWishlistDTO;
        }
    }
}
