using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NWebsec.AspNetCore.Core.Web;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.Notifications;
using VSOnline.VSECommerce.Domain.Order;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Utilities;
using XSystem.Security.Cryptography;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class CheckoutController : VSControllerBase
    {
        private readonly UserService _userService;
        private readonly ShoppingCartRepository _shoppingCartRepository;
        private readonly CartRepository _cartRepository;
        private readonly DataContext _context;
        private readonly EfContext _efContext;
        private readonly MailHelper _mailHelper;
        private readonly MessageHelper _messageHelper;
        private readonly NotificationServices _notificationServices;
        public CheckoutController(UserService userService, ShoppingCartRepository shoppingCartRepository, CartRepository cartRepository, DataContext context, EfContext efContext, MailHelper mailHelper, MessageHelper messageHelper, IOptions<AppSettings> _appSettings, NotificationServices notificationServices) : base(_appSettings)
        {
            _userService = userService;
            _shoppingCartRepository = shoppingCartRepository;
            _cartRepository = cartRepository;
            _context = context;
            _efContext = efContext;
            _mailHelper = mailHelper;
            _messageHelper = messageHelper;
            _notificationServices = notificationServices;
        }

        [HttpGet("Seller/{BranchId}/GetCartDiscount/{userName}/{couponCode}")]
        public IActionResult GetCartDiscount(int BranchId, string userName, string couponCode)
        {
            decimal? totalDiscount = 0.0M;
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var discountDetails = _shoppingCartRepository.GetDiscountDetails(BranchId, couponCode);
                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUser != null)
                {
                    var user = _userService.GetUser(Convert.ToInt32(currentUserId));
                    if (user != null && (user.Email == userName.ToLower() || user.PhoneNumber1.ToLower() == userName.ToLower()))
                    {
                        var shoppingCartlist = GetShoppingCartItemForUser(user.UserId);
                        var OrderSubtotalInclTax = 0.0M;

                        foreach (var shoppingCart in shoppingCartlist)
                        {
                            OrderSubtotalInclTax += (shoppingCart.UnitPrice * shoppingCart.Quantity);
                        }

                        if (discountDetails != null && discountDetails.UsePercentage && discountDetails.MinOrderValue < OrderSubtotalInclTax)
                        {
                            totalDiscount = (OrderSubtotalInclTax * (discountDetails.DiscountPercentage / 100));
                            if (totalDiscount != null && totalDiscount > discountDetails.MaxDiscountAmount)
                            {
                                totalDiscount = discountDetails.MaxDiscountAmount;
                            }
                        }
                        else if (discountDetails != null && discountDetails.DiscountAmount > 0 && discountDetails.MinOrderValue < OrderSubtotalInclTax)
                        {
                            totalDiscount = discountDetails.DiscountAmount;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
            return Ok(totalDiscount);
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

        [HttpPost("Orders")]
        [HttpPost("Seller/{BranchId}/Orders")]
        public async Task<IActionResult> CreateOrderForCart(int BranchId, CreateOrderItemListDTO shoppingCartItemListDTO)
        {
            var orderId = 0;
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
                if (BranchId > 0)
                {
                    var currencyDetails = _context.Currency.Where(x => x.BranchId == BranchId).FirstOrDefault();
                    if (currencyDetails == null)
                    {
                        return BadRequest("Currency need to be added for your site. If you're an store owner. Kindly add the currency in Settings/Payments");
                    }
                }
                var orderOrigin = Request.Headers.Origin;

                var currentUser = User.Identity.Name;
                var currentUserId = User.FindFirst("UserId")?.Value;
                if (currentUser != null)
                {
                    var user = _userService.GetUser(Convert.ToInt32(currentUserId));
                    var userEmail = string.IsNullOrEmpty(user.Email) ? user.Email : user.Email.ToLower();
                    if (user != null && ((userEmail == shoppingCartItemListDTO.UserName.ToLower()) || user.PhoneNumber1.ToLower() == shoppingCartItemListDTO.UserName.ToLower()))
                    {
                        var shoppingCartlist = GetShoppingCartItemForUser(user.UserId);
                        OrderDTO orderDTO = new OrderDTO();
                        if (shoppingCartlist != null && shoppingCartlist.Count > 0 && shoppingCartlist.Count == shoppingCartItemListDTO.shoppingCartDTOList.Count)
                        {
                            orderDTO.CustomerId = user.UserId;
                            orderDTO.CustomerIp = Request.HttpContext.Connection.RemoteIpAddress.ToString();

                            orderDTO.BillingAddressId = shoppingCartItemListDTO.AddressId;
                            if (shoppingCartItemListDTO.AddressId <= 0)
                            {
                                orderDTO.BillingAddressId = GetCurrentUserAddress(user.UserId);
                            }
                            orderDTO.ShippingAddressId = orderDTO.BillingAddressId;

                            if (shoppingCartItemListDTO.PaymentMethod == Enums.PaymentOption.PaymentGateway1)
                            {
                                orderDTO.OrderStatusId = (int)Enums.OrderStatus.Verified;
                                orderDTO.PaymentStatusId = (int)Enums.PaymentStatus.PaymentInProgress;
                            }
                            else if (shoppingCartItemListDTO.PaymentMethod == Enums.PaymentOption.PaymentGateway6)
                            {
                                orderDTO.OrderStatusId = (int)Enums.OrderStatus.Verified;
                                orderDTO.PaymentStatusId = (int)Enums.PaymentStatus.PaymentInProgress;
                            }
                            else
                            {
                                orderDTO.OrderStatusId = (int)Enums.OrderStatus.Created;
                                orderDTO.PaymentStatusId = (int)Enums.PaymentStatus.PaymentInProgress;
                            }

                            orderId = _shoppingCartRepository.CreateOrder(user.UserId, BranchId, shoppingCartlist, orderDTO,
                                shoppingCartItemListDTO.PaymentMethod, shoppingCartItemListDTO.DeliveryMethod, shoppingCartItemListDTO.CouponCode, orderOrigin);

                            _shoppingCartRepository.UpdateOrderProductItemStatus(orderId, orderDTO.OrderStatusId);
                        }
                        if (shoppingCartItemListDTO.PaymentMethod == Utilities.Enums.PaymentOption.CashOnDelivery || shoppingCartItemListDTO.PaymentMethod == Utilities.Enums.PaymentOption.CardOnDelivery)
                        {
                            //Calling for Reducing Ordered Quantities in Inventory
                            ReduceInventoryQuantity(orderId);
                        }
                        if (orderId > 0 && shoppingCartItemListDTO.PaymentMethod == Utilities.Enums.PaymentOption.PaymentGateway1)
                        {
                            var payGateway = PayUsingGateway(user.UserId, user.FirstName, user.Email, user.PhoneNumber1, orderId);
                            return Ok(payGateway);
                        }
                        else if (orderId > 0 && shoppingCartItemListDTO.PaymentMethod == Utilities.Enums.PaymentOption.PaymentGateway2)
                        {
                            var payPayPal = VerifyPayPal(shoppingCartItemListDTO.PayPalOrderId, user.UserId, orderId);
                            return Ok(payPayPal);
                        }
                        else if (orderId > 0 && shoppingCartItemListDTO.PaymentMethod == Utilities.Enums.PaymentOption.PaymentGateway3)
                        {
                            var payRazorPay = VerifyRazorPay(shoppingCartItemListDTO.RazorPaymentId, shoppingCartItemListDTO.RazorOrderId, shoppingCartItemListDTO.RazorSignature, user.UserId, orderId, shoppingCartItemListDTO.RazorGeneratedOrderId);
                            return Ok(payRazorPay);
                        }

                        else if (orderId > 0 && shoppingCartItemListDTO.PaymentMethod == Utilities.Enums.PaymentOption.PaymentGateway6)
                        {
                            var orderDetails = _shoppingCartRepository.GetOrder(user.UserId, orderId);
                            var payCCAvenue = PayUsingCCAvenue(orderId.ToString(), orderDetails.OrderTotal);
                            return Ok(payCCAvenue);
                        }

                        if (orderId > 0)
                        {
                            //send notification
                            var BranchIdList = new List<int>();
                            if (BranchId > 0)
                            {
                                BranchIdList.Add(BranchId);
                            }
                            else if (BranchId == 0)
                            {
                                BranchIdList = shoppingCartItemListDTO.shoppingCartDTOList.Select(x => x.BranchId).ToList();
                            }
                            if (BranchIdList.Count > 0)
                            {
                                foreach (var branchId in BranchIdList)
                                {
                                    var notifationDetails = _context.PushNotification.Where(x => x.BranchId == branchId).ToList();
                                    var branchName = _context.SellerBranch.Where(x => x.BranchId == branchId).Select(x => x.BranchName).FirstOrDefault();
                                    if (notifationDetails.Count > 0)
                                    {
                                        foreach (var eachNotice in notifationDetails)
                                        {
                                            if (eachNotice.FlagNotification)
                                            {
                                                // send notification for the device //SendTotificationBYToKen
                                                string title = "New Order";
                                                string body = "Hey there, We have recieved a new order from customer " + user.FirstName + " for the store " + branchName;
                                                await _notificationServices.SendTotificationBYToKen(title, body, eachNotice.DeviceToken);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return Ok(orderId.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
            return Ok(orderId.ToString());
        }
        private int GetCurrentUserAddress(int customerId)
        {
            var address = _shoppingCartRepository.GetBuyerAddressForUser(customerId);
            return address.AddressId;
        }
        private void ReduceInventoryQuantity(int orderId)
        {
            try
            {
                if (orderId > 0)
                {
                    var orderedItemList = _context.OrderProduct.Where(a => a.Id == orderId).FirstOrDefault();
                    if (orderedItemList != null)
                    {
                        if (orderedItemList.PaymentStatusId == (int)Utilities.Enums.PaymentStatus.PaymentInProgress || orderedItemList.PaymentStatusId == (int)Enums.PaymentStatus.PaymentCompleted)
                        {
                            var orderedItems = _shoppingCartRepository.GetOrderedItems(orderId);
                            foreach (var eachOrderedItem in orderedItems)
                            {
                                _cartRepository.ReduceQuantityInInventory(eachOrderedItem.ProductId, eachOrderedItem.Quantity);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        private string PayUsingGateway(int userid, string userFirstName, string userEmail, string userPhoneNumber, int orderid)
        {
            var order = _shoppingCartRepository.GetOrder(userid, orderid);
            _shoppingCartRepository.UpdateAndSaveTransactionId(orderid, order);
            string firstName = userFirstName;
            string amount = order.OrderTotal.ToString();
            string productInfo = orderid.ToString();
            string email = userEmail;
            string phone = userPhoneNumber;
            string surl = _appSettings.PayUSuccessurl;
            string furl = _appSettings.PayUFailureurl;

            string txnid = Generatetxnid();
            order.TransactionId = txnid;
            _shoppingCartRepository.UpdateAndSaveTransactionId(orderid, order);


            RemotePost myremotepost = new RemotePost();
            string key = _appSettings.PAYUKEY;
            string salt = _appSettings.PAYUSALT;

            //posting all the parameters required for integration.

            myremotepost.Url = _appSettings.PAYUUrl;
            myremotepost.Add("key", _appSettings.PAYUKEY);

            //  myremotepost.Add("txnid", orderid.ToString());
            myremotepost.Add("txnid", txnid);
            myremotepost.Add("amount", amount);
            myremotepost.Add("productinfo", productInfo);
            myremotepost.Add("firstname", firstName);
            myremotepost.Add("phone", phone);
            myremotepost.Add("email", email);
            myremotepost.Add("surl", surl);//Change the success url here depending upon the port number of your local system.
            myremotepost.Add("furl", furl);//Change the failure url here depending upon the port number of your local system.
            myremotepost.Add("service_provider", "payu_paisa");
            string hashString = key + "|" + txnid + "|" + amount + "|" + productInfo + "|" + firstName + "|" + email + "|||||||||||" + salt;
            //string hashString = "3Q5c3q|2590640|3053.00|OnlineBooking|vimallad|ladvimal@gmail.com|||||||||||mE2RxRwx";
            string hash = Generatehash512(hashString);
            myremotepost.Add("hash", hash);

            return myremotepost.Post();
        }


        #region PG Code
        public class RemotePost
        {
            private System.Collections.Specialized.NameValueCollection Inputs = new System.Collections.Specialized.NameValueCollection();
            public string Url = "";
            public string Method = "post";
            public string FormName = "form1";
            public void Add(string name, string value)
            {
                Inputs.Add(name, value);
            }

            public string Post()
            {
                StringBuilder formBuilder = new StringBuilder();
                formBuilder.Append("<html><head>");
                formBuilder.Append(string.Format("</head><body onload=\"document.{0}.submit()\">", FormName));
                formBuilder.Append(string.Format("<form name=\"{0}\" id=\"payuForm\" method=\"{1}\" action=\"{2}\" >", FormName, Method, Url));
                for (int i = 0; i < Inputs.Keys.Count; i++)
                {
                    formBuilder.Append(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", Inputs.Keys[i], Inputs[Inputs.Keys[i]]));
                }
                formBuilder.Append("</form>");
                formBuilder.Append("</body></html>");
                return formBuilder.ToString();
            }
        }
        private string Generatehash512(string text)
        {
            byte[] message = Encoding.UTF8.GetBytes(text);
            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            XSystem.Security.Cryptography.SHA512Managed hashString = new XSystem.Security.Cryptography.SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;
        }
        private string Generatetxnid()
        {
            Random rnd = new Random();
            string strHash = Generatehash512(rnd.ToString() + DateTime.Now);
            string txnid1 = strHash.ToString().Substring(0, 20);
            return txnid1;
        }
        #endregion
        private string VerifyPayPal(string paypalOrderId, int userid, int orderid)
        {
            var paypal = PaypalSecrectKey();
            string token = GetPayPalBearer(paypal.Item1, paypal.Item2);
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_appSettings.PayPalOrders.ToString() + paypalOrderId);
            request.Headers["Authorization"] = "Bearer " + token;
            request.Accept = "application/json";
            request.Headers.Add("Accept-Language", "en_US");
            request.Method = "GET";
            request.ContentType = "application/json";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream streamResponse = response.GetResponseStream();
            StreamReader streamRead = new StreamReader(streamResponse);
            string responseString = streamRead.ReadToEnd();
            dynamic serializedResponse = JsonConvert.DeserializeObject(responseString);

            var orderRepo = _shoppingCartRepository.GetOrder(userid, orderid);
            _shoppingCartRepository.UpdateAndSaveTransactionId(orderid, orderRepo);
            string txnid = Generatetxnid();
            orderRepo.TransactionId = txnid;
            _shoppingCartRepository.UpdateAndSaveTransactionId(orderid, orderRepo);

            //responseString needs to be saved for TransactionResult
            //success needs to be saved for TransactionResultDetails
            //generate id from Generatetxnid and save it to TransactionId
            if (serializedResponse["status"].Value.ToString() == "COMPLETED")
            {
                var order = _shoppingCartRepository.GetOrderProductUsingTransaction(txnid);

                if (order != null)
                {
                    order.TransactionResult = responseString;
                    order.TransactionResultDetails = "Payment is Successful";
                    order.PaymentStatusId = (int)Utilities.Enums.PaymentStatus.PaymentCompleted;
                    order.OrderStatusId = (int)Utilities.Enums.OrderStatus.Verified;
                    _shoppingCartRepository.UpdateAndSave(order);
                    _shoppingCartRepository.UpdateOrderProductItemStatus(order.Id, order.OrderStatusId);
                    //Called for Reducing Ordered Quantities in Inventory
                    ReduceInventoryQuantity(order.Id);
                }
            }
            else
            {
                var order = _shoppingCartRepository.GetOrderProductUsingTransaction(txnid);
                if (order != null)
                {
                    order.TransactionResult = responseString;
                    order.TransactionResultDetails = "Payment Failed";
                    order.PaymentStatusId = (int)Utilities.Enums.PaymentStatus.PaymentInProgress;
                    order.OrderStatusId = (int)Utilities.Enums.OrderStatus.VerificationInProgress;
                    _shoppingCartRepository.UpdateAndSave(order);
                    _shoppingCartRepository.UpdateOrderProductItemStatus(order.Id, order.OrderStatusId);
                }
            }
            return orderid.ToString();
        }
        private Tuple<string, string> PaypalSecrectKey()
        {
            try
            {
                string SecretKey = "";
                string SecretId = "";
                var paypalDetails = _context.SubscriptionProvider.Where(x => x.Provider == "PayPal").FirstOrDefault();
                if (paypalDetails != null)
                {
                    SecretKey = paypalDetails.SecretKey.ToString();
                    SecretId = paypalDetails.SecretId.ToString();
                }
                return Tuple.Create(SecretKey.ToString(), SecretId.ToString());

            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private Tuple<string, string> RazorSecrectKey()
        {
            try
            {
                string SecretKey = "";
                string SecretId = "";
                var paypalDetails = _context.SubscriptionProvider.Where(x => x.Provider == "Razor").FirstOrDefault();
                if (paypalDetails != null)
                {
                    SecretKey = paypalDetails.SecretKey.ToString();
                    SecretId = paypalDetails.SecretId.ToString();
                }
                return Tuple.Create(SecretKey.ToString(), SecretId.ToString());
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private string GetPayPalBearer(string clientId, string clientSecret)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_appSettings.PayPalToken.ToString());
                request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(clientId + ":" + clientSecret));
                request.Accept = "application/json";
                request.Headers.Add("Accept-Language", "en_US");
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 10000;
                byte[] postBytes = Encoding.ASCII.GetBytes("grant_type=client_credentials");
                Stream postStream = request.GetRequestStream();
                postStream.Write(postBytes, 0, postBytes.Length);
                postStream.Flush();
                postStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse);
                string responseString = streamRead.ReadToEnd();
                dynamic serializedResponse = JsonConvert.DeserializeObject(responseString);
                return serializedResponse["access_token"].Value.ToString();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        private string VerifyRazorPay(string razorPaymentId, string razorOrderId, string razorSignature, int userid, int orderid, string razorGeneratedOrderId)
        {
            //get keySecret from db
            var razor = RazorSecrectKey();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] keyBytes = encoding.GetBytes(razor.Item2);
            byte[] messageBytes = encoding.GetBytes(razorGeneratedOrderId.ToString() + "|" + razorPaymentId);
            HMACSHA256 cryptographer = new HMACSHA256(keyBytes);
            byte[] bytes = cryptographer.ComputeHash(messageBytes);
            var signatureToVerify = BitConverter.ToString(bytes).Replace("-", "").ToLower();
            var orderRepo = _shoppingCartRepository.GetOrder(userid, orderid);
            _shoppingCartRepository.UpdateAndSaveTransactionId(orderid, orderRepo);
            string txnid = Generatetxnid();
            orderRepo.TransactionId = txnid;
            _shoppingCartRepository.UpdateAndSaveTransactionId(orderid, orderRepo);
            string trasactionResultString = @"{'razorpay_payment_id': '" + razorPaymentId + "', 'razorpay_order_id':'" + razorOrderId + "','razorpay_signature': '" + razorSignature + "'}";
            if (signatureToVerify == razorSignature)
            {
                var order = _shoppingCartRepository.GetOrderProductUsingTransaction(txnid);
                if (order != null)
                {
                    order.TransactionResult = trasactionResultString;
                    order.TransactionResultDetails = "Payment is Successful";
                    order.PaymentStatusId = (int)Enums.PaymentStatus.PaymentCompleted;
                    order.OrderStatusId = (int)Enums.OrderStatus.Verified;
                    _shoppingCartRepository.UpdateAndSave(order);
                    _shoppingCartRepository.UpdateOrderProductItemStatus(order.Id, order.OrderStatusId);
                    //Called for Reducing Ordered Quantities in Inventory
                    ReduceInventoryQuantity(order.Id);
                }
            }
            else
            {
                var order = _shoppingCartRepository.GetOrderProductUsingTransaction(txnid);
                if (order != null)
                {
                    order.TransactionResult = trasactionResultString;
                    order.TransactionResultDetails = "Payment Failed";
                    order.PaymentStatusId = (int)Enums.PaymentStatus.PaymentInProgress;
                    order.OrderStatusId = (int)Enums.OrderStatus.VerificationInProgress;
                    _shoppingCartRepository.UpdateAndSave(order);
                    _shoppingCartRepository.UpdateOrderProductItemStatus(order.Id, order.OrderStatusId);
                }
            }
            return orderid.ToString();
        }

        [HttpGet("GetOrderConfirmationDetails/{orderId}/{userName}")]
        [HttpGet("Seller/{BranchId}/GetOrderConfirmationDetails/{orderId}/{userName}")]
        public IActionResult GetOrderConfirmationDetails(int BranchId, int orderId, string userName)
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
                        oderIdPrefix = !string.IsNullOrEmpty(sellerBranch.OrderIdPrefix) ? sellerBranch.OrderIdPrefix : sellerBranch.BranchName;
                    }
                }
                if (orderId > 0)
                {
                    var currentUser = User.Identity.Name;
                    var currentUserId = User.FindFirst("UserId")?.Value;

                    if (currentUser != null)
                    {
                        var user = _userService.GetUser(Convert.ToInt32(currentUserId));
                        OrderConfirmationResult orderDetail = new OrderConfirmationResult();
                        string storeName = "";
                        if (user.Email == userName.ToLower() || user.PhoneNumber1.ToLower() == userName.ToLower())
                        {
                            var orderDTOObj = _shoppingCartRepository.GetOrder(user.UserId, orderId);
                            orderDetail.OrderDetails = orderDTOObj;
                            orderDetail.OrderItemDetails = _shoppingCartRepository.GetOrderedItemlist(orderId);
                            foreach (var orderItem in orderDetail.OrderItemDetails)
                            {
                                var productImages = _context.ProductImage.Where(x => x.ProductId == orderItem.ProductId).FirstOrDefault();
                                if (productImages != null)
                                {
                                    if (!productImages.PictureName.Contains("http"))
                                    {
                                        orderItem.PictureName = _appSettings.ImageUrlBase + productImages.PictureName;
                                    }
                                    else
                                    {
                                        orderItem.PictureName = productImages.PictureName;
                                    }
                                }
                            }
                            orderDetail.ShippingAddress = _shoppingCartRepository.GetAddress(orderDTOObj.ShippingAddressId);
                            orderDetail.CustomerName = user.FirstName;
                            orderDetail.CustomerEmail = user.Email;

                            //send order confirmation email 
                            StringBuilder trConfirmation = new StringBuilder();
                            foreach (var item in orderDetail.OrderItemDetails)
                            {
                                storeName = item.Branch;
                                trConfirmation.Append("<tr>");

                                trConfirmation.Append("<td width='30%'>");
                                trConfirmation.Append(item.Name + "<br> Store: " + item.Branch);
                                if (item.SelectedSize != null && !string.IsNullOrEmpty(item.SelectedSize))
                                {
                                    trConfirmation.Append(" / Size: " + item.SelectedSize);
                                }

                                trConfirmation.Append("</td>");
                                trConfirmation.Append("<td>");
                                trConfirmation.Append(item.Quantity);
                                trConfirmation.Append("</td>");

                                trConfirmation.Append(" <td align='right'>");
                                trConfirmation.Append(item.UnitPrice);
                                trConfirmation.Append(" </td>");

                                trConfirmation.Append(" <td align='right'>");
                                trConfirmation.Append(item.Price);
                                trConfirmation.Append(" </td>");
                                trConfirmation.Append(" </tr>");
                            }
                            try
                            {
                                string orderIdWithPrefix = string.IsNullOrEmpty(orderDetail.OrderDetails.BranchOrderId.ToString()) ? orderId.ToString() : oderIdPrefix.ToUpper() + "-" + orderDetail.OrderDetails.BranchOrderId;

                                _mailHelper.SendOrderConfirmationMail(orderDetail.CustomerEmail,
                                orderIdWithPrefix.ToString(), trConfirmation.ToString(), orderDetail.OrderDetails.OrderSubtotalInclTax, orderDetail.OrderDetails.OrderShippingTotal,
                                orderDetail.OrderDetails.OrderDiscount, orderDetail.OrderDetails.OrderTotal, orderDetail.CustomerName
                                , orderDetail.ShippingAddress.Address1, orderDetail.ShippingAddress.Address2, orderDetail.ShippingAddress.City
                                , orderDetail.ShippingAddress.State, orderDetail.ShippingAddress.PostalCode, orderDetail.ShippingAddress.PhoneNumber, storeName, orderDetail.OrderDetails.OrderTaxTotal); ;

                                _messageHelper.SendOrderConfirmationSMS(orderIdWithPrefix, orderDetail.ShippingAddress.PhoneNumber);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                        return Ok(orderDetail);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        //Razor
        [HttpGet("CreateRazorOrderId/{amount}")]
        [HttpGet("Seller/{BranchId}/CreateRazorOrderId/{amount}")]
        public IActionResult CreateRazorOrderId(int BranchId, string amount)
        {
            if (BranchId > 0)
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
            }
            RazorOrderResult razorOrderResult = new RazorOrderResult();
            string razorOrderId = "0";
            try
            {
                var razor = RazorSecrectKey();
                string receiptId = Generatetxnid();
                decimal amountForRazor = Convert.ToDecimal(amount) * 100; //need to enter amount as paise(razor docs)
                int amountForRazorInt = (int)amountForRazor;
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_appSettings.RazorOrders.ToString());
                request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(razor.Item1 + ":" + razor.Item2));
                request.Accept = "application/json";
                request.Headers.Add("Accept-Language", "en_US");
                request.Method = "POST";
                request.ContentType = "application/json";
                string postBody = @"{'amount': '" + amountForRazorInt.ToString() + "', 'currency':'INR','receipt': '" + receiptId + "'}";
                var body = JsonConvert.DeserializeObject(postBody);
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(body, Formatting.Indented);
                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse);
                string responseString = streamRead.ReadToEnd();
                dynamic obj = JsonConvert.DeserializeObject(responseString);
                razorOrderId = obj["id"].Value.ToString();
                razorOrderResult.RazorOrderId = razorOrderId;
                razorOrderResult.RazorKeyId = razor.Item1;
                razorOrderResult.Amount = amount;
            }
            catch (Exception ex)
            {
                razorOrderResult.RazorOrderId = ex.ToString();
            }
            return Ok(razorOrderResult);
        }
        private string PayUsingCCAvenue(string orderUniqueId, decimal orderTotal)
        {
            CCA.Util.CCACrypto crypto = new CCA.Util.CCACrypto();

            string currencyCode = _appSettings.CCCurrency;
            string amount = orderTotal.ToString(); // set the amount of the transaction
            string orderId = orderUniqueId; // set the order ID for the transaction
            string redirectUrl = _appSettings.CCRedirectURL; // set the redirect URL for the transaction
            string cancelUrl = _appSettings.CCCancelURL; // set the cancel URL for the transaction
            string language = _appSettings.CCLanguage; // set the cancel URL for the transaction

            string requestString = "merchant_id=" + _appSettings.CCMerchantId + "&order_id=" + orderId + "&amount=" + amount + "&currency=" + currencyCode + "&redirect_url=" + redirectUrl + "&cancel_url=" + cancelUrl + "&language=" + language;
            string encryptedRequest = crypto.Encrypt(requestString.ToString(), _appSettings.CCSecretkey);
            string ccBillingURL = _appSettings.CCStagingURL + "&encRequest=" + HttpUtility.UrlEncode(encryptedRequest) + "&access_code=" + HttpUtility.UrlEncode(_appSettings.CCAccessCode);
            return ccBillingURL.ToString();
        }

        [HttpPost("UpdateCCResponse")]
        public IActionResult UpdateCCResponse()
        {
            CCA.Util.CCACrypto crypto = new CCA.Util.CCACrypto();

            // Read the parameters sent by CCAvenue in the request body
            var encResponse = Request.Form["encResp"];
            var orderNo = Request.Form["orderNo"];
            var crossUrl = Request.Form["crossSellUrl"];

            string decrpytResponse = crypto.Decrypt(encResponse.ToString(), _appSettings.CCSecretkey);
            string[] responsePairs = decrpytResponse.Split('&');
            Dictionary<string, string> responseDictionary = new Dictionary<string, string>();
            foreach (string pair in responsePairs)
            {
                string[] keyValue = pair.Split('=');
                responseDictionary.Add(keyValue[0], keyValue[1]);
            }
            string orderstatus = responseDictionary["order_status"];
            string orderId = responseDictionary["order_id"];
            string transactionId = responseDictionary["bank_ref_no"];

            if (orderstatus == "Success")
            {
                if (int.Parse(orderId) > 0)
                {
                    // change in OrderProductItem table
                    var orderProductItem = _shoppingCartRepository.UpdateOrderProductItemStatus(int.Parse(orderId), (int)Enums.OrderStatus.Verified);

                    // change in OrderProduct table
                    var orderDetails = _context.OrderProduct.Where(x => x.Id == int.Parse(orderId)).FirstOrDefault();
                    if (orderDetails != null)
                    {
                        orderDetails.TransactionId = transactionId;
                        orderDetails.TransactionResult = decrpytResponse.ToString();
                        orderDetails.TransactionResultDetails = "Payment is Successful";
                        orderDetails.PaymentStatusId = (int)Enums.PaymentStatus.PaymentCompleted;
                        orderDetails.OrderStatusId = (int)Enums.OrderStatus.Verified;
                        _context.OrderProduct.Update(orderDetails);
                        _context.SaveChanges();
                    }
                }
                var htmlString = GetStatusBody("Success");
                string htmlContent = htmlString.Replace("{orderNo}", orderId);
                return Content(htmlContent, "text/html");
            }
            else
            {
                if (int.Parse(orderId) > 0)
                {
                    // change in OrderProductItem table
                    var orderProductItem = _shoppingCartRepository.UpdateOrderProductItemStatus(int.Parse(orderId), (int)Enums.OrderStatus.VerificationInProgress);

                    // change in OrderProduct table
                    var orderDetails = _context.OrderProduct.Where(x => x.Id == int.Parse(orderId)).FirstOrDefault();
                    if (orderDetails != null)
                    {
                        orderDetails.TransactionId = transactionId;
                        orderDetails.TransactionResult = decrpytResponse.ToString();
                        orderDetails.TransactionResultDetails = "Payment Failed";
                        orderDetails.PaymentStatusId = (int)Utilities.Enums.PaymentStatus.PaymentInProgress;
                        orderDetails.OrderStatusId = (int)Utilities.Enums.OrderStatus.VerificationInProgress;
                        _context.OrderProduct.Update(orderDetails);
                        _context.SaveChanges();
                    }
                }
                var htmlString = GetStatusBody("Failure");
                string htmlContent = htmlString.Replace("{orderId}", orderId);
                return Content(htmlContent, "text/html");
            }
        }

        private string GetStatusBody(string templateEnum)
        {
            string body = "";
            //Read template file from the App_Data folder
            string eMailTemplateLocation = @"EmailTemplates/";

            switch (templateEnum)
            {
                case "Success":
                    using (var sr = new StreamReader(eMailTemplateLocation + "CCAvenueSuccess.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case "Failure":
                    using (var sr = new StreamReader(eMailTemplateLocation + "CCAvenueFailure.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
            }
            return body;
        }

    }

}
