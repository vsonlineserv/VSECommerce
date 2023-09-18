using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Order;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class CartController : VSControllerBase
    {
        private readonly CartRepository _cartRepository;
        private readonly EfContext _efContext;
        private readonly ShoppingCartRepository _shoppingCartRepository;
        private readonly DataContext _Context;
        private readonly IMapper _imapper;
        public CartController(CartRepository cartRepository, EfContext efContext, ShoppingCartRepository shoppingCartRepository, DataContext context, IMapper imapper, IOptions<AppSettings> _appSettings) : base(_appSettings)
        {
            _cartRepository = cartRepository;
            _efContext = efContext;
            _shoppingCartRepository = shoppingCartRepository;
            _Context = context;
            _imapper = imapper;
        }

        [HttpGet("GetShoppingCartItems/{userName}")]
        [HttpGet("Seller/{BranchId}/GetShoppingCartItems/{userName}")]
        public IActionResult GetShoppingCartItems(int BranchId, string userName)
        {
            try
            {
                if (BranchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUser != null && currentUser == userName)
                {
                    var cartItemDetails = GetShoppingCartItemForUser(Convert.ToInt32(currentUserId));
                    return Ok(cartItemDetails);
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("AddShoppingCartItemList")]
        [HttpPost("Seller/{BranchId}/AddShoppingCartItemList")]
        public IActionResult AddShoppingCartItemList(int BranchId, ShoppingCartItemListDTO shoppingCartItemListDTO)
        {
            try
            {
                if (BranchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (shoppingCartItemListDTO != null && shoppingCartItemListDTO.shoppingCartDTOList != null && shoppingCartItemListDTO.shoppingCartDTOList.Count > 0 && currentUser != null)
                {
                    _shoppingCartRepository.Add(shoppingCartItemListDTO.shoppingCartDTOList, Convert.ToInt32(currentUserId));
                    var cartItemDetails = GetShoppingCartItemForUser(Convert.ToInt32(currentUserId));
                    return Ok(cartItemDetails);
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPut("UpdateCartItemQuantity")]
        [HttpPut("Seller/{BranchId}/UpdateCartItemQuantity")]
        public IActionResult UpdateCartItemQuantity(int BranchId, ShoppingCartDTO cartItem)
        {
            try
            {
                if (BranchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                List<ShoppingCartResult> shoppingCartResultSet = new List<ShoppingCartResult>();
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (!string.IsNullOrEmpty(currentUser) && cartItem != null && currentUser == cartItem.UserName)
                {
                    cartItem.CustomerId = Convert.ToInt32(currentUserId);
                    var updateCartItem = _Context.ShoppingCartItem.Where(x => x.ProductId == cartItem.ProductId && x.CustomerId == Convert.ToInt32(currentUserId)).FirstOrDefault();
                    if (updateCartItem != null)
                    {
                        var checkProductAvailability = _cartRepository.CheckAvailableQuantityInInventory(cartItem.ProductId, cartItem.Quantity);
                        if (checkProductAvailability)
                        {
                            ShoppingCartResult ShoppingCartResult = new ShoppingCartResult();
                            ShoppingCartResult.FlagQuantityExceeded = true;
                            shoppingCartResultSet.Add(ShoppingCartResult);
                            return Ok(shoppingCartResultSet);
                        }
                        else
                        {
                            updateCartItem.Quantity = cartItem.Quantity;
                            _Context.ShoppingCartItem.Update(updateCartItem);
                            _Context.SaveChanges();
                            var cartItemDetails = GetShoppingCartItemForUser(Convert.ToInt32(currentUserId));
                            return Ok(cartItemDetails);
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("RemoveShoppingCartItem")]
        [HttpPost("Seller/{BranchId}/RemoveShoppingCartItem")]
        public IActionResult RemoveShoppingCartItem(int BranchId, ShoppingCartDTO cartItem)
        {
            try
            {
                if (BranchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUser != null && currentUser == cartItem.UserName)
                {
                    cartItem.CustomerId = Convert.ToInt32(currentUserId);
                    var updateCartItem = _Context.ShoppingCartItem.Where(x => x.ProductId == cartItem.ProductId && x.CustomerId == Convert.ToInt32(currentUserId)).FirstOrDefault<ShoppingCartItem>();
                    if (updateCartItem != null)
                    {
                        _Context.ShoppingCartItem.Remove(updateCartItem);
                        _Context.Entry(updateCartItem).State = EntityState.Deleted;
                        _Context.SaveChanges();
                    }
                    var cartItemDetails = GetShoppingCartItemForUser(Convert.ToInt32(currentUserId));
                    return Ok(cartItemDetails);
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("GetBuyerAddress/{userName}")]
        [HttpGet("Seller/{BranchId}/GetBuyerAddress/{userName}")]
        public IActionResult GetBuyerAddress(int BranchId, string userName)
        {
            try
            {
                if (BranchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUser != null && currentUser == userName)
                {
                    var buyerAddresDetails = _shoppingCartRepository.GetBuyerAddressForUser(Convert.ToInt32(currentUserId));
                    return Ok(buyerAddresDetails);
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("AddBuyerAddress")]
        [HttpPost("Seller/{BranchId}/AddBuyerAddress")]
        public IActionResult AddBuyerAddress(int BranchId, BuyerAddressDTO buyerAddressDTO)
        {
            try
            {
                if (BranchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;

                if (buyerAddressDTO != null && currentUser != null && currentUser == buyerAddressDTO.UserName)
                {
                    _shoppingCartRepository.AddBuyerAddress(buyerAddressDTO, Convert.ToInt32(currentUserId));
                    var buyerAddresDetails = _imapper.Map<BuyerAddressResult>(buyerAddressDTO);
                    return Ok(buyerAddresDetails);
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("AddShoppingCartItem")]
        [HttpPost("Seller/{BranchId}/AddShoppingCartItem")]
        public IActionResult AddShoppingCartItem(int BranchId, ShoppingCartDTO cartItem)
        {
            try
            {
                if (BranchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                List<ShoppingCartResult> shoppingCartResultSet = new List<ShoppingCartResult>();
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;

                if (currentUser != null && currentUser == cartItem.UserName)
                {
                    cartItem.CustomerId = Convert.ToInt32(currentUserId);
                    // Check if the product is available in inventory
                    var checkProductAvailability = _cartRepository.CheckAvailableQuantityInInventory(cartItem.ProductId, cartItem.Quantity);
                    if (checkProductAvailability)
                    {
                        ShoppingCartResult ShoppingCartResult = new ShoppingCartResult();
                        ShoppingCartResult.FlagQuantityExceeded = true;
                        shoppingCartResultSet.Add(ShoppingCartResult);
                        return Ok(shoppingCartResultSet);
                    }
                    else
                    {
                        _shoppingCartRepository.Add(cartItem);
                        var cartItemDetails = GetShoppingCartItemForUser(Convert.ToInt32(currentUserId));
                        return Ok(cartItemDetails);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        private List<ShoppingCartResult> GetShoppingCartItemForUser(int userId)
        {
            try
            {
                var currentUser = User.Identity.Name;
                if (userId > 0)
                {
                    var query = _cartRepository.GetShoppingCartForUserQuery(userId);
                    var cartList = _efContext.Database.SqlQuery<ShoppingCartResult>(query).ToList();
                    foreach (var eachCart in cartList)
                    {
                        var availableQuantity = _cartRepository.getAvailableQuantity(eachCart.ProductId);
                        var checkProductAvailability = _cartRepository.CheckAvailableQuantityInInventory(eachCart.ProductId, eachCart.Quantity);
                        if (eachCart.SpecialPrice < eachCart.Price)
                        {
                            eachCart.TotalSaved = (eachCart.Price - eachCart.SpecialPrice) * eachCart.Quantity;
                        }
                        if (checkProductAvailability)
                        {
                            eachCart.FlagQuantityExceeded = true;
                        }
                        if (availableQuantity != null)
                        {
                            eachCart.AvailableQuantity = availableQuantity;
                        }
                        if (eachCart.PictureName != null)
                        {
                            if (!eachCart.PictureName.Contains("http"))
                            {
                                eachCart.PictureName = _appSettings.ImageUrlBase + eachCart.PictureName;
                            }
                        }
                    }
                    return cartList;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [HttpGet("GetBuyerAddressForVbuy/{userName}")]
        public IActionResult GetBuyerAddress(string userName)
        {
            try
            {
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUser != null && currentUser == userName)
                {
                    var buyerAddresDetails = _shoppingCartRepository.GetBuyerAddressForVbuyUser(Convert.ToInt32(currentUserId));
                    return Ok(buyerAddresDetails);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [HttpPost("UpdateBuyerAddress")]
        public IActionResult UpdateBuyerAddress(BuyerUpdateAddressDTO buyerUpdateAddressDTO)
        {
            try
            {
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;

                if (buyerUpdateAddressDTO != null && currentUser != null && currentUser == buyerUpdateAddressDTO.UserName)
                {
                    _shoppingCartRepository.UpdateBuyerAddress(buyerUpdateAddressDTO, Convert.ToInt32(currentUserId));
                    return Ok("Address updated successfully");
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
    }
}
