using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Order
{
    public class ShoppingCartRepository
    {
        private readonly DataContext _context;
        private IMapper _mapper;
        IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        private readonly IConfigurationSection _appSettings;
        public ShoppingCartRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _appSettings = _configuration.GetSection("AppSettings");
        }

        public IEnumerable<ShoppingCartItem> Find(Expression<Func<ShoppingCartItem, bool>> predicate)
        {
            return _context.ShoppingCartItem.Where(predicate);
        }

        public void Add(ShoppingCartItem entity)
        {
            _context.ShoppingCartItem.Add(entity);
            _context.SaveChanges();
        }

        public bool Add(ShoppingCartResult cartItem)
        {
            try
            {
                //Add only if cart item is anot already available. 
                var prodExistCount = _context.ShoppingCartItem.Where(x => x.ProductId == cartItem.ProductId && x.CustomerId == cartItem.CustomerId && x.BranchId == cartItem.BranchId).Count();
                if (prodExistCount == 0)
                {
                    ShoppingCartItem shoppingCartItem = new ShoppingCartItem();
                    shoppingCartItem.ProductId = cartItem.ProductId;
                    shoppingCartItem.BranchId = cartItem.BranchId;
                    shoppingCartItem.UnitPrice = cartItem.UnitPrice;
                    shoppingCartItem.Quantity = cartItem.Quantity;
                    shoppingCartItem.ShippingCharges = cartItem.AdditionalShippingCharge;
                    shoppingCartItem.SelectedSize = cartItem.SelectedSize;
                    shoppingCartItem.CustomerId = cartItem.CustomerId;
                    shoppingCartItem.CreatedOnUtc = DateTime.UtcNow;
                    shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                    _context.ShoppingCartItem.Add(shoppingCartItem);
                    _context.Entry(shoppingCartItem).State = EntityState.Added;
                    var changes = _context.SaveChanges();
                    if (changes > 0)
                    {
                        return true;
                    }
                }
                else if (prodExistCount > 0)
                {
                    var shoppingCartItem = _context.ShoppingCartItem.Where(x => x.ProductId == cartItem.ProductId && x.CustomerId == cartItem.CustomerId && x.BranchId == cartItem.BranchId).FirstOrDefault();
                    if (shoppingCartItem != null)
                    {
                        shoppingCartItem.Quantity = cartItem.Quantity;
                        shoppingCartItem.ShippingCharges = cartItem.AdditionalShippingCharge;

                        shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                        _context.Entry(shoppingCartItem).State = EntityState.Modified;
                        var changes = _context.SaveChanges();
                        if (changes > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            catch
            {

            }
            return false;
        }
        public bool Add(List<ShoppingCartResult> cartItemList, int customerId)
        {
            try
            {

                foreach (ShoppingCartResult cartresult in cartItemList)
                {
                    //Add only if cart item is anot already available. 
                    var prodExistCount = _context.ShoppingCartItem.Where(x => x.ProductId == cartresult.ProductId && x.CustomerId == customerId && x.BranchId == cartresult.BranchId).Count();

                    if (prodExistCount == 0)
                    {
                        ShoppingCartItem shoppingCartItem = new ShoppingCartItem();
                        shoppingCartItem.ProductId = cartresult.ProductId;
                        shoppingCartItem.BranchId = cartresult.BranchId;
                        shoppingCartItem.UnitPrice = cartresult.SpecialPrice;
                        shoppingCartItem.Quantity = cartresult.Quantity;
                        shoppingCartItem.CustomerId = customerId;
                        shoppingCartItem.ShippingCharges = cartresult.AdditionalShippingCharge;
                        shoppingCartItem.SelectedSize = cartresult.SelectedSize;
                        shoppingCartItem.CreatedOnUtc = DateTime.UtcNow;
                        shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                        _context.ShoppingCartItem.Add(shoppingCartItem);
                        _context.Entry(shoppingCartItem).State = EntityState.Added;
                    }
                    else if (prodExistCount > 0)
                    {
                        var shoppingCartItem = _context.ShoppingCartItem.Where(x => x.ProductId == cartresult.ProductId && x.CustomerId == customerId && x.BranchId == cartresult.BranchId).FirstOrDefault();
                        if (shoppingCartItem != null)
                        {
                            shoppingCartItem.Quantity = cartresult.Quantity;
                            shoppingCartItem.ShippingCharges = cartresult.AdditionalShippingCharge;

                            shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                            _context.Entry(shoppingCartItem).State = EntityState.Modified;
                        }
                    }
                }
                var changes = _context.SaveChanges();
                if (changes > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {

            }
            return false;
        }
        public BuyerAddressResult GetBuyerAddressForUser(int customerId)
        {
            try
            {
                var buyerAddress = _context.BuyerAddress.Where(x => x.User == customerId).OrderByDescending(x => x.UpdatedOnUtc).FirstOrDefault<BuyerAddress>();
                if (buyerAddress != null)
                {
                    BuyerAddressResult addressDTO = new BuyerAddressResult();
                    addressDTO.State = buyerAddress.State;
                    addressDTO.City = buyerAddress.City;
                    addressDTO.Address1 = buyerAddress.Address1;
                    addressDTO.Address2 = buyerAddress.Address2;
                    addressDTO.PostalCode = buyerAddress.PostalCode;
                    addressDTO.PhoneNumber = buyerAddress.PhoneNumber;
                    addressDTO.AddressId = buyerAddress.BuyerAddressId;
                    addressDTO.Country = buyerAddress.Country;
                    return addressDTO;
                }
            }
            catch
            {

            }
            return null;
        }
        public bool AddBuyerAddress(BuyerAddressDTO addressDTO, int customerId)
        {
            try
            {
                BuyerAddress addressObj = new BuyerAddress();
                addressObj.User = customerId;
                addressObj.State = addressDTO.State;
                addressObj.City = addressDTO.City;
                addressObj.Address1 = addressDTO.Address1;
                addressObj.Address2 = addressDTO.Address2;
                addressObj.PostalCode = addressDTO.PostalCode;
                addressObj.PhoneNumber = addressDTO.PhoneNumber;
                addressObj.Country = addressDTO.Country;
                addressObj.CreatedOnUtc = DateTime.UtcNow;
                addressObj.UpdatedOnUtc = DateTime.UtcNow;
                _context.Entry(addressObj).State = EntityState.Added;
                var changes = _context.SaveChanges();
                if (changes > 0)
                {
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }
        public DiscountResult GetDiscountDetails(string couponCode)
        {
            try
            {
                if (!string.IsNullOrEmpty(couponCode))
                {
                    var discountObject = _context.Discount.Where(x => x.CouponCode.ToUpper() == couponCode.ToUpper() && x.StartDateUtc <= DateTime.UtcNow
                        && x.EndDateUtc >= DateTime.UtcNow && (x.IsDeleted != null && x.IsDeleted == false)).FirstOrDefault();
                    DiscountResult discountResult = new DiscountResult();
                    discountResult.CouponCode = couponCode;
                    if (discountObject != null && couponCode != null && couponCode.ToUpper() == discountObject.CouponCode.ToUpper())
                    {
                        discountResult.UsePercentage = discountObject.UsePercentage;
                        discountResult.DiscountDescription = discountObject.Name;
                        discountResult.DiscountPercentage = discountObject.DiscountPercentage;
                        discountResult.DiscountAmount = discountObject.DiscountAmount;
                        discountResult.MaxDiscountAmount = discountObject.MaxDiscountAmount;
                        discountResult.MinOrderValue = discountObject.MinOrderValue;
                    }
                    return discountResult;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public DiscountResult GetDiscountDetails(int BranchId, string couponCode)
        {
            try
            {
                if (!string.IsNullOrEmpty(couponCode))
                {
                    var discountObject = _context.Discount.Where(x => x.CouponCode.ToUpper() == couponCode.ToUpper() && x.StartDateUtc <= DateTime.UtcNow
                        && x.EndDateUtc >= DateTime.UtcNow && x.BranchId == BranchId && (x.IsDeleted != null && x.IsDeleted == false)).FirstOrDefault();
                    DiscountResult discountResult = new DiscountResult();
                    discountResult.CouponCode = couponCode;
                    if (discountObject != null && couponCode != null && couponCode.ToUpper() == discountObject.CouponCode.ToUpper())
                    {
                        discountResult.UsePercentage = discountObject.UsePercentage;
                        discountResult.DiscountDescription = discountObject.Name;
                        discountResult.DiscountPercentage = discountObject.DiscountPercentage;
                        discountResult.DiscountAmount = discountObject.DiscountAmount;
                        discountResult.MaxDiscountAmount = discountObject.MaxDiscountAmount;
                        discountResult.MinOrderValue = discountObject.MinOrderValue;
                    }
                    return discountResult;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public int CreateOrder(int userId, int BranchId, List<ShoppingCartResult> shoppingCartItemList, OrderDTO orderDTO, Enums.PaymentOption paymentOption, Enums.DeliveryOption deliveryOption, string couponCode, string orderOrigin)
        {
            var orderid = 0;
            OrderProduct orderProduct = new OrderProduct();
            try
            {
                decimal ordersubTotalInclTax = 0M;
                orderProduct.OrderGuid = Guid.NewGuid();
                orderProduct.BillingAddressId = orderDTO.BillingAddressId;
                orderProduct.ShippingAddressId = orderDTO.ShippingAddressId;
                orderProduct.CustomerIp = orderDTO.CustomerIp;
                orderProduct.CustomerId = orderDTO.CustomerId;
                orderProduct.OrderStatusId = orderDTO.OrderStatusId;
                orderProduct.PaymentStatusId = orderDTO.PaymentStatusId;
                orderProduct.PaymentMethod = (int)paymentOption;
                orderProduct.DeliveryMethod = (int)deliveryOption;

                orderProduct.OrderShippingCharges = 0;

                orderProduct.OrderDateUtc = DateTime.UtcNow;
                orderProduct.UpdatedOnUtc = DateTime.UtcNow;
                orderProduct.BranchId = BranchId;


                foreach (ShoppingCartResult shoppingCartItem in shoppingCartItemList)
                {
                    OrderProductItem item = new OrderProductItem();
                    item.BranchId = shoppingCartItem.BranchId;
                    item.ProductId = shoppingCartItem.ProductId;
                    item.OrderItemGuid = Guid.NewGuid();
                    item.Quantity = shoppingCartItem.Quantity;
                    item.UnitPriceInclTax = shoppingCartItem.UnitPrice;
                    item.PriceInclTax = item.UnitPriceInclTax * item.Quantity;

                    item.ShippingCharges = shoppingCartItem.AdditionalShippingCharge * item.Quantity;
                    item.OrderItemStatus = orderProduct.OrderStatusId;//when creating we will have same status.
                    item.SelectedSize = shoppingCartItem.SelectedSize;

                    var currentShoppingCartItem = _context.ShoppingCartItem.Where(x => x.BranchId == shoppingCartItem.BranchId && x.CustomerId == orderProduct.CustomerId
                        && x.ProductId == item.ProductId && x.Quantity == item.Quantity).FirstOrDefault();

                    orderProduct.OrderSubtotalInclTax += item.PriceInclTax;
                    orderProduct.OrderShippingCharges += item.ShippingCharges ?? 0;

                    orderProduct.OrderProductItem.Add(item);
                    _context.ShoppingCartItem.Remove(currentShoppingCartItem);
                }
                //
                orderProduct.OrderDiscount = 0;
                if (!string.IsNullOrEmpty(couponCode))
                {
                    var discountObject = _context.Discount.Where(x => x.CouponCode.ToUpper() == couponCode.ToUpper()
                        && x.StartDateUtc <= DateTime.UtcNow && x.EndDateUtc >= DateTime.UtcNow && x.IsDeleted == false).FirstOrDefault();
                    if (discountObject != null && discountObject.UsePercentage && discountObject.MinOrderValue < orderProduct.OrderSubtotalInclTax)
                    {
                        orderProduct.OrderDiscount = (orderProduct.OrderSubtotalInclTax * (discountObject.DiscountPercentage / 100));
                    }
                    else if (discountObject != null && discountObject.DiscountAmount > 0 && discountObject.MinOrderValue < orderProduct.OrderSubtotalInclTax)
                    {
                        orderProduct.OrderDiscount = orderProduct.OrderSubtotalInclTax - discountObject.DiscountAmount;
                    }
                    if (discountObject != null)
                    {
                        if (orderProduct.OrderDiscount > discountObject.MaxDiscountAmount)
                        {
                            orderProduct.OrderDiscount = discountObject.MaxDiscountAmount;
                        }
                    }

                }

                // for tax
                orderProduct.OrderTaxTotal = 0;

                var taxDetails = _context.TaxMaster.Where(x => x.BranchId == BranchId).FirstOrDefault();
                if (taxDetails != null)
                {
                    var TaxPercentage = decimal.Parse(taxDetails.TaxType);
                    orderProduct.OrderTaxTotal = ((orderProduct.OrderSubtotalInclTax * TaxPercentage) / 100);
                }

                // for shipping
                orderProduct.OrderShippingTotal = 0;

                var shippingDetails = _context.NewMasterSettingsSelections.Where(x => x.BranchId == BranchId).Select(x => x.CurrentSelection).FirstOrDefault();
                if (shippingDetails != null)
                {
                    if (shippingDetails == "FlatRate")
                    {
                        var shippingRateDetails = _context.NewMasterShippingCalculation.Where(x => x.Type == shippingDetails && x.BranchId == BranchId).FirstOrDefault();
                        if (shippingRateDetails != null)
                        {
                            orderProduct.OrderShippingTotal = shippingRateDetails.Rate;
                        }
                    }
                }
                orderProduct.FlagConfirmStatus = false;
                orderProduct.OrderOrigin = orderOrigin;

                orderProduct.OrderFromVbuy = false;
                orderProduct.OrderFromVshopper = false;

                if (!string.IsNullOrEmpty(orderOrigin))
                {
                    if (orderOrigin == _appSettings.GetValue<string>("VbuyHost"))
                    {
                        orderProduct.OrderFromVbuy = true;
                    }
                }

                orderProduct.PaymentMethodAdditionalFee = 0.0M;

                orderProduct.OrderTotal = (orderProduct.OrderSubtotalInclTax + orderProduct.OrderShippingCharges +
                orderProduct.PaymentMethodAdditionalFee + orderProduct.OrderTaxTotal + orderProduct.OrderShippingTotal) - orderProduct.OrderDiscount;

                _context.OrderProduct.Add(orderProduct);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            // add branchOrderidSequence
            if (orderProduct.Id > 0)
            {
                var orderDetails = _context.OrderProduct.Where(x => x.Id == orderProduct.Id && x.BranchId == BranchId).FirstOrDefault();
                if (orderDetails != null)
                {
                    var branchOrder = _context.OrderProduct.Where(x => x.BranchId == BranchId).Select(x => x.BranchOrderId).Max();
                    orderDetails.BranchOrderId = branchOrder == null ? 10001 : branchOrder + 1;
                    _context.OrderProduct.Update(orderDetails);
                    _context.SaveChanges();
                }
            }
            return orderProduct.Id;
        }
        public bool UpdateOrderProductItemStatus(int orderId, int OrderStatus)
        {
            try
            {
                List<OrderProductItem> orderProductItemList = _context.OrderProductItem.Where(x => x.OrderId == orderId).ToList();


                foreach (var item in orderProductItemList)
                {
                    item.OrderItemStatus = OrderStatus;
                    _context.Entry(item).State = EntityState.Modified;
                }
                _context.SaveChanges();
                return true;
            }
            catch
            {

            }
            return false;
        }
        public OrderDTO GetOrder(int userId, int orderId)
        {
            try
            {
                if (orderId > 0)
                {
                    var orderProduct = _context.OrderProduct.Where(x => x.Id == orderId && x.CustomerId == userId).First();
                    if (orderProduct != null)
                    {
                        OrderDTO orderDTOObj = new OrderDTO();
                        _mapper.Map<OrderProduct, OrderDTO>(orderProduct, orderDTOObj);
                        return orderDTOObj;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public List<OrderItemResult> GetOrderedItems(int orderId)
        {
            var orderItemList = _context.OrderProductItem.Where(x => x.OrderId == orderId).ToList();
            List<OrderItemResult> orderitemListDTO = new List<OrderItemResult>();

            _mapper.Map<IEnumerable<OrderProductItem>,
                IEnumerable<OrderItemResult>>(orderItemList, orderitemListDTO);

            return orderitemListDTO;
        }
        public bool UpdateAndSaveTransactionId(int orderId, OrderDTO orderDTO)
        {
            try
            {
                var orderProduct = _context.OrderProduct.Where(x => x.Id == orderId).FirstOrDefault();
                if (orderProduct != null)
                {
                    orderProduct.TransactionId = orderDTO.TransactionId;
                    _context.Entry(orderProduct).State = EntityState.Modified;
                    _context.SaveChanges();
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }
        public OrderProduct GetOrderProductUsingTransaction(string txnId)
        {
            try
            {
                if (!string.IsNullOrEmpty(txnId))
                {
                    var orderProduct = _context.OrderProduct.Where(x => x.TransactionId == txnId).First();
                    return orderProduct;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public bool UpdateAndSave(OrderProduct orderProduct)
        {
            try
            {
                orderProduct.UpdatedOnUtc = DateTime.UtcNow;
                _context.Entry(orderProduct).State = EntityState.Modified;
                _context.SaveChanges();
                return true;
            }
            catch
            {

            }
            return false;
        }
        public List<OrderItemResult> GetOrderedItemlist(int orderId)
        {
            var orderItemList = _context.OrderProductItem.Where(x => x.OrderId == orderId).Include(x => x.OrderProductMap).Include(x => x.SellerBranchMap).Include(x => x.ProductMap).ToList();

            List<OrderItemResult> orderitemListDTO = new List<OrderItemResult>();

            _mapper.Map<IEnumerable<OrderProductItem>, IEnumerable<OrderItemResult>>(orderItemList, orderitemListDTO);

            return orderitemListDTO;
        }
        public BuyerAddressResult GetAddress(int addressId)
        {
            var address = _context.BuyerAddress.Where(x => x.BuyerAddressId == addressId).First();
            BuyerAddressResult addressResult = new BuyerAddressResult();
            addressResult.Address1 = address.Address1;
            addressResult.Address2 = address.Address2;
            addressResult.City = address.City;
            addressResult.State = address.State;
            addressResult.PhoneNumber = address.PhoneNumber;
            addressResult.PostalCode = address.PostalCode;
            addressResult.AddressId = addressId;
            return addressResult;
        }
        public OrderDTO GetOrder(int orderId)
        {
            if (orderId > 0)
            {
                var orderProduct = _context.OrderProduct.Where(x => x.Id == orderId).First();
                if (orderProduct != null)
                {
                    OrderDTO orderDTOObj = new OrderDTO();
                    _mapper.Map<OrderProduct, OrderDTO>(orderProduct, orderDTOObj);
                    return orderDTOObj;
                }
            }
            return null;
        }
        public List<OrderItemResult> GetOrderedItemlist(int orderId, int branchId)
        {
            var orderItemList = _context.OrderProductItem.Where(x => x.OrderId == orderId && x.BranchId == branchId).Include(x => x.OrderProductMap)
                .Include(x => x.SellerBranchMap).Include(x => x.ProductMap)
                .ToList();
            List<OrderItemResult> orderitemListDTO = new List<OrderItemResult>();

            _mapper.Map<IEnumerable<OrderProductItem>, IEnumerable<OrderItemResult>>(orderItemList, orderitemListDTO);

            return orderitemListDTO;
        }
        public bool Add(ShoppingCartDTO cartItem)
        {
            try
            {
                //Add only if cart item is anot already available. 
                var prodExistCount = _context.ShoppingCartItem.Where(x => x.ProductId == cartItem.ProductId
                    && x.CustomerId == cartItem.CustomerId &&
                    x.BranchId == cartItem.BranchId).Count();
                if (prodExistCount == 0)
                {
                    ShoppingCartItem shoppingCartItem = new ShoppingCartItem();
                    shoppingCartItem.ProductId = cartItem.ProductId;
                    shoppingCartItem.BranchId = cartItem.BranchId;
                    shoppingCartItem.UnitPrice = cartItem.UnitPrice;
                    shoppingCartItem.Quantity = cartItem.Quantity;
                    shoppingCartItem.ShippingCharges = cartItem.AdditionalShippingCharge;
                    shoppingCartItem.SelectedSize = cartItem.SelectedSize;
                    shoppingCartItem.CustomerId = cartItem.CustomerId;
                    shoppingCartItem.CreatedOnUtc = DateTime.UtcNow;
                    shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

                    _context.ShoppingCartItem.Add(shoppingCartItem);
                    _context.Entry(shoppingCartItem).State = EntityState.Added;
                    var changes = _context.SaveChanges();
                    if (changes > 0)
                    {
                        return true;
                    }
                }
                else if (prodExistCount > 0)
                {
                    var shoppingCartItem = _context.ShoppingCartItem.Where(x => x.ProductId == cartItem.ProductId && x.CustomerId == cartItem.CustomerId &&
                    x.BranchId == cartItem.BranchId).FirstOrDefault();
                    if (shoppingCartItem != null)
                    {
                        shoppingCartItem.Quantity = cartItem.Quantity;
                        shoppingCartItem.ShippingCharges = cartItem.AdditionalShippingCharge;

                        shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                        _context.Entry(shoppingCartItem).State = EntityState.Modified;
                        var changes = _context.SaveChanges();
                        if (changes > 0)
                        {
                            return true;
                        }
                    }

                }
            }
            catch
            {

            }
            return false;
        }

        public List<BuyerAddressResult> GetBuyerAddressForVbuyUser(int customerId)
        {
            try
            {
                List<BuyerAddressResult> buyerAddressResults = new List<BuyerAddressResult>();
                var buyerAddress = _context.BuyerAddress.Where(x => x.User == customerId).OrderByDescending(x => x.UpdatedOnUtc).ToList();
                if (buyerAddress.Count > 0)
                {
                    foreach (var data in buyerAddress)
                    {
                        BuyerAddressResult addressDTO = new BuyerAddressResult();
                        addressDTO.State = data.State;
                        addressDTO.City = data.City;
                        addressDTO.Address1 = data.Address1;
                        addressDTO.Address2 = data.Address2;
                        addressDTO.PostalCode = data.PostalCode;
                        addressDTO.PhoneNumber = data.PhoneNumber;
                        addressDTO.AddressId = data.BuyerAddressId;
                        addressDTO.Country = data.Country;
                        buyerAddressResults.Add(addressDTO);
                    }
                    return (List<BuyerAddressResult>)buyerAddressResults.Take(2).ToList();
                }
            }
            catch
            {

            }
            return null;
        }

        public bool UpdateBuyerAddress(BuyerUpdateAddressDTO addressDTO, int customerId)
        {
            try
            {
                var buyerAddress = _context.BuyerAddress.Where(x => x.BuyerAddressId == Convert.ToInt16(addressDTO.AddressId) && x.User == customerId).FirstOrDefault();
                if (buyerAddress != null)
                {
                    buyerAddress.State = addressDTO.State;
                    buyerAddress.City = addressDTO.City;
                    buyerAddress.Address1 = addressDTO.Address1;
                    buyerAddress.Address2 = addressDTO.Address2;
                    buyerAddress.PostalCode = addressDTO.PostalCode;
                    buyerAddress.PhoneNumber = addressDTO.PhoneNumber;
                    buyerAddress.Country = addressDTO.Country;
                    buyerAddress.UpdatedOnUtc = DateTime.UtcNow;
                    _context.BuyerAddress.Update(buyerAddress);
                    var changes = _context.SaveChanges();
                    if (changes > 0)
                    {
                        return true;
                    }
                }
            }
            catch
            {

            }
            return false;
        }
    }
}
