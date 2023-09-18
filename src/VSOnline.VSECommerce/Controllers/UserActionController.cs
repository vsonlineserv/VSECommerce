using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class UserActionController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserActionHelper _userActionHelper;
        private readonly UserWishlistRepository _userWishlistRepository;
        public UserActionController(DataContext context, UserActionHelper userActionHelper, UserWishlistRepository userWishlistRepository)
        {
            _context = context;
            _userActionHelper = userActionHelper;
            _userWishlistRepository = userWishlistRepository;
        }

        [HttpGet("GetUserWishlist/{userName}")]
        [HttpGet("Seller/{branchId}/GetUserWishlist/{userName}")]
        public IActionResult GetUserWishlist(int branchId, string userName)
        {
            try
            {
                if (branchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == branchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUser == userName)
                {
                    var userList = _context.UserWishlist.Where(x => x.User == Convert.ToInt32(currentUserId)).ToList<UserWishlist>();
                    if (branchId > 0)
                    {
                        userList = _context.UserWishlist.Where(x => x.User == Convert.ToInt32(currentUserId) && x.BranchId == branchId).ToList<UserWishlist>();
                    }
                    var userListDetails = _userActionHelper.GetUserWishlistModelFromUserWishlist(userList);
                    return Ok(userListDetails);
                }
                return Ok(new List<UserWishlistResult>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }

        }

        [HttpGet("AddUserWishList/{userName}/{productId}")]
        [HttpGet("Seller/{branchId}/AddUserWishList/{userName}/{productId}")]
        public IActionResult AddUserWishList(int branchId, string userName, int productId)
        {
            try
            {
                if (branchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == branchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUser == userName)
                {
                    var userWishList = _userWishlistRepository.AddUserWishList(Convert.ToInt32(currentUserId), productId, branchId);
                    return GetUserWishlist(branchId, userName);
                }
                else return Ok(new List<UserWishlistResult>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("GetUserWishlistCount/{userName}")]
        [HttpGet("Seller/{branchId}/GetUserWishlistCount/{userName}")]
        public IActionResult GetUserWishlistCount(int branchId, string userName)
        {
            try
            {
                if (branchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == branchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUser == userName)
                {
                    var wishListCount = _context.UserWishlist.Where(x => x.User == Convert.ToInt32(currentUserId)).ToList<UserWishlist>().Count;
                    if (branchId > 0)
                    {
                        wishListCount = _context.UserWishlist.Where(x => x.User == Convert.ToInt32(currentUserId) && x.BranchId == branchId).ToList<UserWishlist>().Count;
                    }
                    return Ok(wishListCount);
                }
                return Ok(0);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }

        }

        [HttpGet("RemoveUserWishList/{userName}/{productId}")]
        [HttpGet("Seller/{branchId}/RemoveUserWishList/{userName}/{productId}")]
        public IActionResult RemoveUserWishList(int branchId, string userName, int productId)
        {
            try
            {
                if (branchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == branchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUser == userName)
                {
                    var userWishList = _userWishlistRepository.RemoveUserWishList(Convert.ToInt32(currentUserId), productId, branchId);
                    return GetUserWishlist(branchId, userName);
                }
                else
                {
                    return Ok(new List<UserWishlistResult>());
                }
            }
            catch (Exception EX)
            {
                return StatusCode(500, "There is some error");
            }
        }
    }
}
