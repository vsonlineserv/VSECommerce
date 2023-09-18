using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.Order;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class OrderTrackingController : ControllerBase
    {
        private readonly EfContext _efContext;
        private readonly DataContext _context;
        private readonly ShoppingCartRepository _shoppingCartRepository;
        private readonly TrackingOrderHelper _trackingOrderHelper;
        private readonly OrderHelper _orderHelper;
        private readonly CartRepository _cartRepository;
        private readonly UserService _userService;
        private readonly MailHelper _mailHelper;
        private readonly MessageHelper _messageHelper;

        public OrderTrackingController(EfContext efContext, DataContext context, ShoppingCartRepository shoppingCartRepository, TrackingOrderHelper trackingOrderHelper, OrderHelper orderHelper, CartRepository cartRepository, UserService userService, MailHelper mailHelper, MessageHelper messageHelper)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _efContext = efContext;
            _context = context;
            _trackingOrderHelper = trackingOrderHelper;
            _orderHelper = orderHelper;
            _cartRepository = cartRepository;
            _userService = userService;
            _mailHelper = mailHelper;
            _messageHelper = messageHelper;
        }

        [HttpGet("GetOrdersList")]
        [HttpGet("Seller/{BranchId}/GetOrdersList")]
        public IActionResult GetOrdersList(int BranchId)
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
                if (!string.IsNullOrEmpty(User.Identity.Name))
                {
                    var currentUserId = User.FindFirst("UserId")?.Value;
                    var query = _trackingOrderHelper.GetOrdersList(Convert.ToInt32(currentUserId));
                    var result = _efContext.Database.SqlQuery<OrderTrackingResult>(query).ToList();
                    return Ok(result);
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("GetProductList/{OrderId}")]
        [HttpGet("Seller/{BranchId}/GetProductList/{OrderId}")]
        public IActionResult GetProductList(int BranchId, int OrderId)
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
                var query = _trackingOrderHelper.GetProductList(OrderId);
                var result = _efContext.Database.SqlQuery<OrderTrackingResult>(query).ToList();
                foreach (var orderresult in result)
                {
                    if (orderresult.OrderItemStatusId != null)
                    {
                        orderresult.OrderItemStatus = _orderHelper.GetEnumDescription((Enums.OrderStatus)orderresult.OrderItemStatusId);

                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("SearchOrders/{OrderId}")]
        [HttpGet("Seller/{BranchId}/SearchOrders/{OrderId}")]
        public IActionResult SearchOrders(int BranchId, string OrderId)
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
                string currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                var query = _trackingOrderHelper.SearchOrders(OrderId, Convert.ToInt32(currentUserId));
                var result = _efContext.Database.SqlQuery<OrderTrackingResult>(query).ToList();
                foreach (var orderresult in result)
                {
                    orderresult.OrderStatus = _orderHelper.GetEnumDescription((Enums.OrderStatus)orderresult.OrderStatusId);
                    orderresult.PaymentStatus = _orderHelper.GetEnumDescription((Enums.PaymentOption)orderresult.PaymentMethod);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }

        }

        [HttpGet("GetTrackingOrders")]
        [HttpGet("Seller/{BranchId}/GetTrackingOrders")]
        public IActionResult GetTrackingOrders(int BranchId)
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
                if (!string.IsNullOrEmpty(User.Identity.Name))
                {
                    var currentUserId = User.FindFirst("UserId")?.Value;
                    var query = _trackingOrderHelper.GetTrackingOrder(Convert.ToInt32(currentUserId));
                    var result = _efContext.Database.SqlQuery<OrderTrackingResult>(query).ToList();
                    foreach (var orderresult in result)
                    {
                        orderresult.OrderStatus = _orderHelper.GetEnumDescription((Enums.OrderStatus)orderresult.OrderStatusId);
                        orderresult.PaymentStatus = _orderHelper.GetEnumDescription((Enums.PaymentStatus)orderresult.PaymentStatusId);
                        orderresult.PaymentMethodString = _orderHelper.GetEnumDescription((Enums.PaymentOption)orderresult.PaymentMethod);
                    }
                    return Ok(result);
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("CancelOrders/{OrderId}")]
        [HttpGet("Seller/{BranchId}/CancelOrders/{OrderId}")]
        public IActionResult CancelOrders(int BranchId, int OrderId)
        {
            try
            {
                var oderIdPrefix = "";
                if (BranchId > 0)
                {
                    var branchIds = User.FindAll("BranchId").ToList();
                    if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                    var sellerBranch = _context.SellerBranch.Where(x => x.BranchId == BranchId).FirstOrDefault();
                    if (sellerBranch != null)
                    {
                        oderIdPrefix = sellerBranch.OrderIdPrefix;
                    }
                }
                var cancelOrders = _context.OrderProduct.Where(x => x.Id == OrderId).Include(y => y.OrderProductItem).FirstOrDefault();
                var shippingAddress = _shoppingCartRepository.GetAddress(cancelOrders.ShippingAddressId);
                var user = _userService.GetUser(cancelOrders.CustomerId);
                if (cancelOrders.CustomerId == CurrentUserId())
                {
                    var orderStatusId = (int)((Enums.OrderStatus)Enum.Parse(typeof(Enums.OrderStatus), "Cancelled"));

                    if (cancelOrders.OrderStatusId < 10)
                    {
                        foreach (var orderItem in cancelOrders.OrderProductItem)
                        {
                            if (orderItem.OrderItemStatus < 10)
                            {
                                orderItem.OrderItemStatus = orderStatusId;
                                orderItem.OrderCancel = true;
                                _cartRepository.AddQuantityInInventory(orderItem.ProductId, orderItem.Quantity);
                            }
                        }
                        cancelOrders.OrderStatusId = orderStatusId;
                        cancelOrders.OrderCancel = true;
                        _context.OrderProduct.Update(cancelOrders);
                        _context.SaveChanges();
                        try
                        {
                            string orderIdWithPrefix = string.IsNullOrEmpty(cancelOrders.BranchOrderId.ToString()) ? OrderId.ToString() : oderIdPrefix + "-" + cancelOrders.BranchOrderId.ToString();

                            _mailHelper.SendOrderCancellationMail(user.Email, orderIdWithPrefix.ToString());
                            _messageHelper.SendOrderCancellationSMS(orderIdWithPrefix, shippingAddress.PhoneNumber);
                        }
                        catch
                        {

                        }
                        return Ok(true);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
            return BadRequest(false);
        }
        private int CurrentUserId()
        {
            var currentUser = User.Identity.Name;
            var currentUserId = User.FindFirst("UserId")?.Value;
            var orderId = 0;
            if (currentUser != null)
            {
                var user = _userService.GetUser(Convert.ToInt32(currentUserId));
                if (user != null && (!string.IsNullOrEmpty(user.Email) || !string.IsNullOrEmpty(user.PhoneNumber1.ToLower())))
                {
                    return user.UserId;
                }
            }
            return 0;
        }
    }
}

