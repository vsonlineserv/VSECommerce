using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.ShiprocketDTO;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class ShiprocketController : VSControllerBase
    {
        private readonly DataContext _context;
        public ShiprocketController(DataContext context, IOptions<AppSettings> _appSettings) : base(_appSettings)
        {
            _context = context;
        }

        //Basic Apis 
        //https://support.shiprocket.in/support/solutions/articles/43000337456

        [HttpGet("Seller/{BranchId}/GetShiprocketToken")]
        public async Task<IActionResult> GetShiprocketToken(int BranchId)
        {
            var ShiprocketHost = _appSettings.ShiprocketHost.ToString();
            try
            {
                ShiprocketApiUser shiprocketApiUser = new ShiprocketApiUser();
                var shiprocket = _context.ShiprocketApiUser.Where(x => x.BranchId == BranchId).FirstOrDefault();
                if (shiprocket != null)
                {
                    if (shiprocket.ExpireTime != null && shiprocket.ExpireTime.Value.AddDays(9) > DateTime.UtcNow)
                    {
                        return Ok(shiprocket.ShiprocketToken);
                    }
                    else
                    {
                        var newJsonData = new
                        {
                            email = shiprocket.Email,
                            password = shiprocket.Password
                        };
                        var ShipRocketjson = JsonConvert.SerializeObject(newJsonData);
                        var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                        ShipRocketcontent.Headers.Clear();
                        ShipRocketcontent.Headers.Add("Content-Type", "application/json");

                        ServicePointManager.Expect100Continue = true;
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                        using (var httpClients = new HttpClient())
                        {
                            HttpResponseMessage response = await httpClients.PostAsync(ShiprocketHost + "auth/login", ShipRocketcontent);
                            if (response.IsSuccessStatusCode)
                            {
                                var result = response.Content.ReadAsStringAsync().Result;
                                dynamic shiprocketToken = JsonConvert.DeserializeObject<object>(result);
                                shiprocket.ShiprocketToken = shiprocketToken.token;
                                shiprocket.ExpireTime = DateTime.UtcNow;
                            }
                        }
                        shiprocket.UpdatedOnUtc = DateTime.UtcNow;
                        _context.ShiprocketApiUser.Update(shiprocket);
                        _context.SaveChanges();
                    }
                    return Ok(shiprocket.ShiprocketToken);
                }
                return BadRequest("Shiprocket api user not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("Seller/{BranchId}/AddShiprocketApiUser")]
        public IActionResult AddShiprocketApiUser(int BranchId, ShiprocketApiUserDTO shiprocketApiUserDTO)
        {
            try
            {
                if (checkShiprocketApiUser(shiprocketApiUserDTO).Result == true)
                {
                    var shiprocketDetails = _context.ShiprocketApiUser.Where(x => x.Email == shiprocketApiUserDTO.Email && x.BranchId == BranchId).FirstOrDefault();
                    if (shiprocketDetails == null)
                    {
                        ShiprocketApiUser shiprocketApiUser = new ShiprocketApiUser();
                        shiprocketApiUser.BranchId = BranchId;
                        shiprocketApiUser.Email = shiprocketApiUserDTO.Email;
                        shiprocketApiUser.Password = shiprocketApiUserDTO.Password;
                        shiprocketApiUser.CreatedOnUtc = DateTime.UtcNow;
                        _context.ShiprocketApiUser.Add(shiprocketApiUser);
                        _context.SaveChanges();
                        return Ok("Api User Added successfully");
                    }
                    return BadRequest("UserAlready Exists");
                }
                else
                {
                    return BadRequest("Not a Valid Shiprocket Api User");
                }
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("Seller/{BranchId}/UpdateShiprocketApiUser")]
        public IActionResult UpdateShiprocketApiUser(int BranchId, ShiprocketApiUserDTO shiprocketApiUserDTO)
        {
            try
            {
                if (checkShiprocketApiUser(shiprocketApiUserDTO).Result == true)
                {
                    var shiprocketDetails = _context.ShiprocketApiUser.Where(x => x.BranchId == BranchId && x.Id == shiprocketApiUserDTO.Id).FirstOrDefault();
                    if (shiprocketDetails != null)
                    {
                        shiprocketDetails.Email = shiprocketApiUserDTO.Email;
                        shiprocketDetails.Password = shiprocketApiUserDTO.Password;
                        shiprocketDetails.UpdatedOnUtc = DateTime.UtcNow;
                        _context.ShiprocketApiUser.Update(shiprocketDetails);
                        _context.SaveChanges();
                        return Ok();
                    }
                    return BadRequest("User Not Found");
                }
                else
                {
                    return BadRequest("Not a Valid Shiprocket Api User");
                }
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        private async Task<bool> checkShiprocketApiUser(ShiprocketApiUserDTO shiprocketApiUserDTO)
        {
            try
            {
                var newJsonData = new
                {
                    email = shiprocketApiUserDTO.Email,
                    password = shiprocketApiUserDTO.Password
                };
                var ShipRocketjson = JsonConvert.SerializeObject(newJsonData);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");

                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "auth/login", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        dynamic shiprocketToken = JsonConvert.DeserializeObject<object>(result);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [HttpGet("Seller/{BranchId}/GetShiprocketApiUser")]
        public IActionResult GetShiprocketApiUser(int BranchId)
        {
            try
            {
                var shiprocketDetails = _context.ShiprocketApiUser.Where(x => x.BranchId == BranchId).FirstOrDefault();
                if (shiprocketDetails != null)
                {
                    var data = new
                    {
                        id = shiprocketDetails.Id,
                        email = shiprocketDetails.Email,
                    };
                    return Ok(data);
                }
                return Ok("User Not Found");
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("Seller/{BranchId}/GetShipmentID/{OrderId}")]
        public IActionResult GetShipmentID(int BranchId, int OrderId)
        {
            try
            {
                var shiprocketDetails = _context.ShiprocketOrderDetails.Where(x => x.OrderId == OrderId.ToString()).FirstOrDefault();
                if (shiprocketDetails != null)
                {
                    return Ok(shiprocketDetails);
                }
                return Ok("Order Not Found");
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [HttpGet("Seller/{BranchId}/GetTrackingByShipmentID/{ShipmentID}")]
        public IActionResult GetTrackingByShipmentID(int BranchId, string ShipmentID)
        {
            try
            {
                //Get Tracking through Shipment ID
                //GetTrackingByShipmentID

                //Get the tracking details of your shipment by entering the shipment_id of the same in the endpoint URL. No other body parameters are required to access this API.
                var tokenResult = GetShiprocketToken(BranchId);
                var token = tokenResult.Result as OkObjectResult;
                if (token != null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                        HttpResponseMessage response = null;

                        response = httpClient.GetAsync(_appSettings.ShiprocketHost + "courier/track/shipment/" + ShipmentID).GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            return Ok(result);
                        }
                        return Ok();
                    }
                }
                else
                {
                    return BadRequest("Not a Valid Shiprocket Api User");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("Seller/{BranchId}/CreateOrder")]
        public async Task<IActionResult> CreateOrder(int BranchId, ShiprocketOrderDTO shiprocketOrderDTO)
        {
            try
            {
                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;
                if (token != null)
                {
                    shiprocketOrderDTO.billing_last_name = !string.IsNullOrEmpty(shiprocketOrderDTO.billing_last_name) ? shiprocketOrderDTO.billing_last_name : shiprocketOrderDTO.billing_customer_name;
                    shiprocketOrderDTO.shipping_email = !string.IsNullOrEmpty(shiprocketOrderDTO.shipping_email) ? shiprocketOrderDTO.shipping_email : _appSettings.SupportEmailId;
                    shiprocketOrderDTO.billing_email = !string.IsNullOrEmpty(shiprocketOrderDTO.billing_email) ? shiprocketOrderDTO.billing_email : _appSettings.SupportEmailId;

                    var ShipRocketjson = JsonConvert.SerializeObject(shiprocketOrderDTO);
                    var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                    ShipRocketcontent.Headers.Clear();
                    ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    using (var httpClients = new HttpClient())
                    {
                        httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                        HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "orders/create/adhoc", ShipRocketcontent);
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            dynamic shiprocketDetails = JsonConvert.DeserializeObject<object>(result);
                            ShiprocketOrderDetails shiprocketOrderDetails = new ShiprocketOrderDetails();
                            shiprocketOrderDetails.OrderId = shiprocketOrderDTO.order_id;
                            shiprocketOrderDetails.ShiprocketOrderId = shiprocketDetails.order_id;
                            shiprocketOrderDetails.ShipmentId = shiprocketDetails.shipment_id;
                            _context.ShiprocketOrderDetails.Add(shiprocketOrderDetails);
                            _context.SaveChanges();
                            changeOrderStatus(Convert.ToInt16(shiprocketOrderDTO.order_id));
                            return Ok(result);
                        }
                        else
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            return BadRequest(result);
                        }
                    }
                }
                return BadRequest("Not a Valid Shiprocket Api User");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        private void changeOrderStatus(int OrderId)
        {
            var order = _context.OrderProduct.Where(x => x.Id == OrderId).Include(y => y.OrderProductItem).FirstOrDefault();
            OrderProductItem subOrder = order.OrderProductItem.FirstOrDefault(x => x.OrderId == OrderId);
            if (subOrder != null)
            {
                order.OrderStatusId = (int)Enums.OrderStatus.ShippingInProgress;
                foreach (var item in order.OrderProductItem)
                {
                    if (item.OrderItemStatus < (int)Enums.OrderStatus.ShippingInProgress)
                    {
                        item.OrderItemStatus = (int)Enums.OrderStatus.ShippingInProgress;
                    }
                }
                _context.Update(order);
                _context.SaveChanges();
            }
        }
        [HttpPost("Seller/{BranchId}/GenerateAirWaybillNumber")]
        public async Task<IActionResult> GenerateAirWaybillNumber(int BranchId, AirWaybillDTO airWaybillDTO)
        {
            try
            {
                //This API can be used to assign the AWB (Air Waybill Number) to your shipment.
                //The AWB is a unique number that helps you track the shipment and get details about it.

                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;

                var ShipRocketjson = JsonConvert.SerializeObject(airWaybillDTO);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "courier/assign/awb", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        dynamic airwayBillDetails = JsonConvert.DeserializeObject<object>(result);
                        var orderDetails = _context.ShiprocketOrderDetails.Where(x => x.ShipmentId == airWaybillDTO.shipment_id.ToString()).FirstOrDefault();
                        if (orderDetails != null)
                        {
                            orderDetails.AwbCode = airwayBillDetails.awb_code;
                            _context.ShiprocketOrderDetails.Update(orderDetails);
                            _context.SaveChanges();
                        }
                        return Ok(result);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("Seller/{BranchId}/GetCourierServiceability")]
        public IActionResult GetCourierServiceability(int BranchId, int pickup_postcode, int delivery_postcode, string weight, int order_id, int cod)
        {
            try
            {
                var tokenResult = GetShiprocketToken(BranchId);
                var token = tokenResult.Result as OkObjectResult;
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = null;
                    if (cod > 0)
                    {
                        response = httpClient.GetAsync(_appSettings.ShiprocketHost + "courier/serviceability?pickup_postcode=" + pickup_postcode + "&delivery_postcode=" + delivery_postcode + "&weight=" + weight + "&cod=" + cod).GetAwaiter().GetResult();
                    }
                    else
                    {
                        response = httpClient.GetAsync(_appSettings.ShiprocketHost + "courier/serviceability?pickup_postcode=" + pickup_postcode + "&delivery_postcode=" + delivery_postcode + "&weight=" + weight + "&order_id=" + order_id).GetAwaiter().GetResult();
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return Ok(result);
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("Seller/{BranchId}/GeneratePickup")]
        public async Task<IActionResult> GeneratePickup(int BranchId, PickupDTO pickupDTO)
        {
            try
            {
                //This API can be used to assign the AWB (Air Waybill Number) to your shipment.
                //The AWB is a unique number that helps you track the shipment and get details about it.

                //Use this API to create a pickup request for your order shipment. The API returns the pickup status along with the estimated pickup time.
                //You will have to call the 'Generate Manifest' API after the successful response of this API.

                //NOTE
                //The AWB must be already generated for the shipment id to generate the pickup request.
                //Only one shipment_id can be passed at a time.

                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;

                var ShipRocketjson = JsonConvert.SerializeObject(pickupDTO);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "courier/generate/pickup", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result1 = response.Content.ReadAsStringAsync().Result;
                        return Ok(result1);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("Seller/{BranchId}/GenerateManifests")]
        public async Task<IActionResult> GenerateManifests(int BranchId, ShipmentIdsDTO shipmentIdsDTO)
        {
            try
            {
                // Using this API, you can generate the manifest for your order.
                // This API generates the manifest and displays the download URL of the same.

                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;

                var ShipRocketjson = JsonConvert.SerializeObject(shipmentIdsDTO);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "manifests/generate", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result1 = response.Content.ReadAsStringAsync().Result;
                        return Ok(result1);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [HttpPost("Seller/{BranchId}/PrintManifests")]
        public async Task<IActionResult> PrintManifests(int BranchId, OrderIdDTO orderIdDTO)
        {
            try
            {
                //Use this API to print the generated manifest of orders at an individual level.

                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;

                var ShipRocketjson = JsonConvert.SerializeObject(orderIdDTO);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "manifests/print", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result1 = response.Content.ReadAsStringAsync().Result;
                        return Ok(result1);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [HttpPost("Seller/{BranchId}/GenerateLabel")]
        public async Task<IActionResult> GenerateLabel(int BranchId, ShipmentIdsDTO shipmentIdsDTO)
        {
            try
            {
                //Generate the label of order by passing the shipment id in the form of an array.
                //This API displays the URL of the generated label.

                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;

                var ShipRocketjson = JsonConvert.SerializeObject(shipmentIdsDTO);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "courier/generate/label", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result1 = response.Content.ReadAsStringAsync().Result;
                        return Ok(result1);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("Seller/{BranchId}/printInvoice")]
        public async Task<IActionResult> printInvoice(int BranchId, ShiprocketOrderIdDTO shiprocketOrderIdDTO)
        {
            try
            {
                //Use this API to generate the invoice for your order by passing the respective Shiprocket order ids.
                //The generated invoice URL is displayed as a response.Multiple ids can be passed together as an array.


                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;

                var ShipRocketjson = JsonConvert.SerializeObject(shiprocketOrderIdDTO);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "orders/print/invoice", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result1 = response.Content.ReadAsStringAsync().Result;
                        return Ok(result1);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("Seller/{BranchId}/TrackAwb/{awb_code}")]
        public IActionResult TrackAwb(int BranchId, int awb_code)
        {
            try
            {
                //Get the tracking details of your shipment by entering the AWB code of the same in the endpoint URL itself.
                //No other body parameters are required to access this API.
                var tokenResult = GetShiprocketToken(BranchId);
                var token = tokenResult.Result as OkObjectResult;
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = null;

                    response = httpClient.GetAsync(_appSettings.ShiprocketHost + "courier/track/awb/" + awb_code).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return Ok(result);
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        //All
        //Update Customer Delivery Address
        [HttpPost("Seller/{BranchId}/UpdateDeliveryAddress")]
        public async Task<IActionResult> UpdateDeliveryAddress(int BranchId, CustomerAddressDTO customerAddressDTO)
        {
            try
            {
                //You can update the customer's name and delivery address through this API by passing the Shiprocket order id and the necessary customer details.
                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;

                var ShipRocketjson = JsonConvert.SerializeObject(customerAddressDTO);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "orders/address/update", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return Ok(result);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        //Update Order
        [HttpPost("Seller/{BranchId}/UpdateOrder")]
        public async Task<IActionResult> UpdateOrder(int BranchId, ShiprocketUpdateOrderDTO shiprocketUpdateOrderDTO)
        {
            try
            {
                //Use this API to update your orders.You have to pass all the required params at the minimum to create a quick custom order.
                //You can add additional parameters as per your preference.
                //You can update only the order_items details before assigning the AWB(before Ready to Ship status).You can only update these key - value pairs
                //i.e increase/ decrease the quantity, update tax / discount, add / remove product items.

                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;

                var ShipRocketjson = JsonConvert.SerializeObject(shiprocketUpdateOrderDTO);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "orders/update/adhoc", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return Ok(result);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        //Cancel an Order
        [HttpPost("Seller/{BranchId}/CancelOrder")]
        public async Task<IActionResult> CancelOrder(int BranchId, OrderIdDTO orderIdDTO)
        {
            try
            {
                //Use this API to cancel a created order. Multiple order_ids can be passed together as an array to cancel them simultaneously.

                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;

                var ShipRocketjson = JsonConvert.SerializeObject(orderIdDTO);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "orders/cancel", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return Ok(result);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpPost("Seller/{BranchId}/ReturnOrder")]
        public async Task<IActionResult> ReturnOrder(int BranchId, ShipmentIdDTO shipmentIdDTO)
        {
            try
            {
                //Use this API to create a new return order in your Shiprocket panel.Return orders are created in case the buyer refuses / rejects / returns a specific order.
                //The parameter specifications are the same as the custom order API, with a few exceptions.

                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;

                var ShipRocketjson = JsonConvert.SerializeObject(shipmentIdDTO);
                var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                ShipRocketcontent.Headers.Clear();
                ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                using (var httpClients = new HttpClient())
                {
                    httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "orders/create/return", ShipRocketcontent);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return Ok(result);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        //Orders

        [HttpGet("Seller/{BranchId}/GetAllOrders")]
        public IActionResult GetAllOrders(int BranchId)
        {
            try
            {
                //This API call will display a list of all created and available orders in your Shiprocket account.The product and shipment details are displayed as sub - arrays within each order detail.
                var tokenResult = GetShiprocketToken(BranchId);
                var token = tokenResult.Result as OkObjectResult;
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = null;

                    response = httpClient.GetAsync(_appSettings.ShiprocketHost + "orders").GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return Ok(result);
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [HttpGet("Seller/{BranchId}/GetSpecificOrderDetails/{id}")]
        public IActionResult GetSpecificOrderDetails(int BranchId, int id)
        {
            try
            {
                //GetSpecificOrderDetails
                //Get the order and shipment details of a particular order through this API by passing the Shiprocket order_id in the endpoint URL itself — type in your order_id in place of {id}.
                var tokenResult = GetShiprocketToken(BranchId);
                var token = tokenResult.Result as OkObjectResult;
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = null;

                    response = httpClient.GetAsync(_appSettings.ShiprocketHost + "orders/show/" + id).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return Ok(result);
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("Seller/{BranchId}/GetAllReturnOrders")]
        public IActionResult GetAllReturnOrders(int BranchId)
        {
            try
            {
                //GetAllReturnOrders
                //Using this API, you can get a list of all created return orders in your Shiprocket account, along with their details.
                var tokenResult = GetShiprocketToken(BranchId);
                var token = tokenResult.Result as OkObjectResult;
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = null;

                    response = httpClient.GetAsync(_appSettings.ShiprocketHost + "orders/processing/return").GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return Ok(result);
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("Seller/{BranchId}/GetCourierServiceabilityForReturn")]
        public IActionResult GetCourierServiceabilityForReturn(int BranchId, int pickup_postcode, int delivery_postcode, string weight, int order_id, int cod, int is_return)
        {
            try
            {
                var tokenResult = GetShiprocketToken(BranchId);
                var token = tokenResult.Result as OkObjectResult;
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                    HttpResponseMessage response = null;
                    if (cod > 0)
                    {
                        response = httpClient.GetAsync(_appSettings.ShiprocketHost + "courier/serviceability?pickup_postcode=" + pickup_postcode + "&delivery_postcode=" + delivery_postcode + "&weight=" + weight + "&cod=" + cod + "&is_return=" + is_return).GetAwaiter().GetResult();
                    }
                    else
                    {
                        response = httpClient.GetAsync(_appSettings.ShiprocketHost + "courier/serviceability?pickup_postcode=" + pickup_postcode + "&delivery_postcode=" + delivery_postcode + "&weight=" + weight + "&order_id=" + order_id + "&is_return=" + is_return).GetAwaiter().GetResult();
                    }
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        return Ok(result);
                    }
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        // get pickup address
        [HttpGet("Seller/{BranchId}/GetAllPickupLocations")]
        public IActionResult GetAllPickupLocations(int BranchId)
        {
            try
            {
                //GetAllPickupLocations
                var tokenResult = GetShiprocketToken(BranchId);
                var token = tokenResult.Result as OkObjectResult;
                if (token != null)
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                        HttpResponseMessage response = null;

                        response = httpClient.GetAsync(_appSettings.ShiprocketHost + "settings/company/pickup").GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            return Ok(result);
                        }
                        return Ok();
                    }
                }
                else
                {
                    return BadRequest("Not a Valid Shiprocket Api User");
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [HttpPost("Seller/{BranchId}/AddNewPickupLocation")]
        public async Task<IActionResult> AddNewPickupLocation(int BranchId, PickupLocationDTO pickupLocationDTO)
        {
            try
            {
                //AddNewPickupLocation
                var tokenResult = await GetShiprocketToken(BranchId);
                var token = tokenResult as OkObjectResult;
                if (token != null)
                {
                    var ShipRocketjson = JsonConvert.SerializeObject(pickupLocationDTO);
                    var ShipRocketcontent = new StringContent(ShipRocketjson, UnicodeEncoding.UTF8, "application/json");
                    ShipRocketcontent.Headers.Clear();
                    ShipRocketcontent.Headers.Add("Content-Type", "application/json");
                    ServicePointManager.Expect100Continue = true;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    using (var httpClients = new HttpClient())
                    {
                        httpClients.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.ToString());
                        HttpResponseMessage response = await httpClients.PostAsync(_appSettings.ShiprocketHost + "settings/company/addpickup", ShipRocketcontent);
                        if (response.IsSuccessStatusCode)
                        {
                            var result = response.Content.ReadAsStringAsync().Result;
                            return Ok(result);
                        }
                    }
                    return Ok();
                }
                else
                {
                    return BadRequest("Not a Valid Shiprocket Api User");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
    }
}
