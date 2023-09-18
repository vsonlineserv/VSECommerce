using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Permission;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly AppSettings _appSettings;
        public SettingsController(DataContext context, IOptions<AppSettings> appSettings)
        {
            _context = context;
            _appSettings = appSettings.Value;
        }

        [HttpGet("Seller/{BranchId}/Currency")]
        public IActionResult GetCurrencyDetails(int BranchId)
        {
            try
            {
                VSEcomCurrencyResult vSEcomCurrencyResult = new VSEcomCurrencyResult();
                var currency = _context.Currency.Where(x=> x.BranchId == BranchId).FirstOrDefault();
                if (currency != null)
                {
                    vSEcomCurrencyResult.Currency = currency.Code;
                    vSEcomCurrencyResult.Symbol = currency.Symbol;
                }
                var taxMaster = _context.TaxMaster.Where(x=> x.BranchId == BranchId).FirstOrDefault();
                if (taxMaster != null)
                {
                    vSEcomCurrencyResult.TaxType = taxMaster.TaxType;
                    vSEcomCurrencyResult.Value = taxMaster.PrimaryOption;
                }
                return Ok(vSEcomCurrencyResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Payment_Read)]
        [HttpGet("Seller/{BranchId}/GetProviderDetails")]
        public IActionResult GetProviderDetails(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                VSEcomProviderResult vSEcomProviderResult = new VSEcomProviderResult();
                var subscriptionProviders = _context.SubscriptionProvider.Where(x=> x.BranchId == BranchId).ToList();
                foreach (var eachProviders in subscriptionProviders)
                {
                    if (eachProviders.ProviderName == (int)Enums.PaymentOption.CashOnDelivery)
                    {
                        vSEcomProviderResult.CashOnDeliveryEnabled = eachProviders.FlagEnable;
                    }
                    if (eachProviders.ProviderName == (int)Enums.PaymentOption.CardOnDelivery)
                    {
                        vSEcomProviderResult.CardOnDeliveryEnbled = eachProviders.FlagEnable;
                    }
                    if (eachProviders.ProviderName == (int)Enums.PaymentOption.PaymentGateway1)
                    {
                        vSEcomProviderResult.PayUEnabled = eachProviders.FlagEnable;
                    }
                    if (eachProviders.ProviderName == (int)Enums.PaymentOption.PaymentGateway2)
                    {
                        vSEcomProviderResult.PayPalSecretId = string.IsNullOrEmpty(eachProviders.SecretId) ? null : eachProviders.SecretId.Trim();
                        vSEcomProviderResult.PayPalSecretKey = string.IsNullOrEmpty(eachProviders.SecretKey) ? null : eachProviders.SecretKey.Trim();
                        vSEcomProviderResult.PayPalFlagEnabled = eachProviders.FlagEnable;
                    }
                    if (eachProviders.ProviderName == (int)Enums.PaymentOption.PaymentGateway3)
                    {
                        vSEcomProviderResult.RazorSecretId = string.IsNullOrEmpty(eachProviders.SecretId) ? null : eachProviders.SecretId.Trim();
                        vSEcomProviderResult.RazorSecretKey = string.IsNullOrEmpty(eachProviders.SecretKey) ? null : eachProviders.SecretKey.Trim();
                        vSEcomProviderResult.RazorFlagEnabled = eachProviders.FlagEnable;
                    }
                    if (eachProviders.ProviderName == (int)Enums.PaymentOption.OtherGateway)
                    {
                        vSEcomProviderResult.Provider = eachProviders.Provider;
                        vSEcomProviderResult.OtherSecretId = string.IsNullOrEmpty(eachProviders.SecretId) ? null : eachProviders.SecretId.Trim();
                        vSEcomProviderResult.OtherSecretKey = string.IsNullOrEmpty(eachProviders.SecretKey) ? null : eachProviders.SecretKey.Trim();
                        vSEcomProviderResult.OtherFlagEnabled = eachProviders.FlagEnable;
                    }
                    if (eachProviders.ProviderName == (int)Enums.PaymentOption.PaymentGateway4)
                    {
                        vSEcomProviderResult.AppleSecretId = string.IsNullOrEmpty(eachProviders.SecretId) ? null : eachProviders.SecretId.Trim();
                        vSEcomProviderResult.AppleSecretKey = string.IsNullOrEmpty(eachProviders.SecretKey) ? null : eachProviders.SecretKey.Trim();
                        vSEcomProviderResult.AppleFlagEnabled = eachProviders.FlagEnable;
                    }
                    if (eachProviders.ProviderName == (int)Enums.PaymentOption.PaymentGateway5)
                    {
                        vSEcomProviderResult.GoogleSecretId = string.IsNullOrEmpty(eachProviders.SecretId) ? null : eachProviders.SecretId.Trim();
                        vSEcomProviderResult.GoogleSecretKey = string.IsNullOrEmpty(eachProviders.SecretKey) ? null : eachProviders.SecretKey.Trim();
                        vSEcomProviderResult.GoogleFlagEnabled = eachProviders.FlagEnable;
                    }
                }
                return Ok(vSEcomProviderResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Payment_Write)]
        [HttpPost("Seller/{BranchId}/Currency")]
        public IActionResult EditCurrencyDetails(int BranchId, VSEcomCurrencyDTO vSEcomCurrencyDTO)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var orderProduct = _context.OrderProduct.Where(x=> x.BranchId == BranchId).Count();
                if (orderProduct > 0)
                {
                    return BadRequest("Currency can't be changed once an order is placed");
                }
                if (vSEcomCurrencyDTO.Currency != null && vSEcomCurrencyDTO.Code != null)
                {
                    var currencyDetail = _context.Currency.Where(e => e.Id > 0 && e.BranchId == BranchId).FirstOrDefault();
                    if (currencyDetail != null)
                    {
                        currencyDetail.CurrencyName = vSEcomCurrencyDTO.Currency;
                        currencyDetail.Code = vSEcomCurrencyDTO.Code;
                        currencyDetail.Symbol = vSEcomCurrencyDTO.Symbol;
                        currencyDetail.UpdatedDate = DateTime.UtcNow;
                        _context.Currency.Update(currencyDetail);
                        _context.SaveChanges();
                    }
                    else
                    {
                        Currency currency = new Currency();
                        currency.CurrencyName = vSEcomCurrencyDTO.Currency;
                        currency.Code = vSEcomCurrencyDTO.Code;
                        currency.Symbol = vSEcomCurrencyDTO.Symbol;
                        currency.CreatedDate = DateTime.UtcNow;
                        currency.BranchId = BranchId;
                        _context.Currency.Add(currency);
                        _context.SaveChanges();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Payment_Write)]
        [HttpPost("Seller/{BranchId}/Tax")]
        public IActionResult EditTaxDetails(int BranchId, VSEcomCurrencyDTO vSEcomCurrencyDTO)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (vSEcomCurrencyDTO.Value != null)
                {
                    var taxDetails = _context.TaxMaster.Where(e => e.Id > 0 && e.BranchId == BranchId).FirstOrDefault();
                    if (taxDetails != null)
                    {
                        taxDetails.UpdatedDate = DateTime.UtcNow;
                        taxDetails.TaxType = ((string?)(string.IsNullOrEmpty(vSEcomCurrencyDTO.TaxType) ? (object)DBNull.Value : vSEcomCurrencyDTO.TaxType));
                        taxDetails.PrimaryOption = vSEcomCurrencyDTO.Value;
                        _context.TaxMaster.Update(taxDetails);
                        _context.SaveChanges();
                    }
                    else
                    {
                        TaxMaster taxMaster = new TaxMaster();
                        taxMaster.TaxType = ((string?)(string.IsNullOrEmpty(vSEcomCurrencyDTO.TaxType) ? (object)DBNull.Value : vSEcomCurrencyDTO.TaxType));
                        taxMaster.PrimaryOption = vSEcomCurrencyDTO.Value;
                        taxMaster.CreatedDate = DateTime.UtcNow;
                        taxMaster.BranchId = BranchId;
                        _context.TaxMaster.Add(taxMaster);
                        _context.SaveChanges();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Payment_Write)]
        [HttpPost("Seller/{BranchId}/EditPaymentDetails")]
        public IActionResult EditPaymentDetails(int BranchId, VSEcomProviderDTO vSEcomProviderDTO)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                SubscriptionProvider subscriptionProvider = new SubscriptionProvider();
                if (!string.IsNullOrEmpty(vSEcomProviderDTO.PayPalSecretId) && !string.IsNullOrEmpty(vSEcomProviderDTO.PayPalSecretKey))
                {
                    var PaypalToken = VerifyPayPalToken(vSEcomProviderDTO.PayPalSecretKey, vSEcomProviderDTO.PayPalSecretId);
                    if (!string.IsNullOrEmpty(PaypalToken))
                    {
                        var paypalSecretId = _context.SubscriptionProvider.Where(x => x.Provider == "PayPal" && x.BranchId == BranchId).FirstOrDefault();
                        if (paypalSecretId != null)
                        {
                            paypalSecretId.SecretKey = vSEcomProviderDTO.PayPalSecretKey.Trim();
                            paypalSecretId.SecretId = vSEcomProviderDTO.PayPalSecretId.Trim();
                            paypalSecretId.FlagEnable = vSEcomProviderDTO.PayPalFlagEnabled;
                            paypalSecretId.UpdatedDate = DateTime.UtcNow;
                            _context.Update(paypalSecretId);
                            _context.SaveChanges();
                        }
                        else
                        {
                            subscriptionProvider.Provider = "PayPal";
                            subscriptionProvider.SecretKey = vSEcomProviderDTO.PayPalSecretKey.Trim();
                            subscriptionProvider.SecretId = vSEcomProviderDTO.PayPalSecretId.Trim();
                            subscriptionProvider.FlagEnable = vSEcomProviderDTO.PayPalFlagEnabled;
                            subscriptionProvider.CreatedDate = DateTime.UtcNow;
                            subscriptionProvider.ProviderName = (int)Enums.PaymentOption.PaymentGateway2;
                            subscriptionProvider.BranchId = BranchId;
                            _context.SubscriptionProvider.Add(subscriptionProvider);
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        return BadRequest("Client Id or Secret Key is Invalid");
                    }
                }
                //Razor
                var RazorId = vSEcomProviderDTO.RazorSecretId;
                if (!string.IsNullOrEmpty(RazorId) && !string.IsNullOrEmpty(vSEcomProviderDTO.RazorSecretKey) && !RazorId.Contains("test"))
                {
                    var RazorToken = VerifyRazorToken(RazorId, vSEcomProviderDTO.RazorSecretKey);
                    if (!String.IsNullOrEmpty(RazorToken))
                    {
                        var razorSecretId = _context.SubscriptionProvider.Where(x => x.Provider == "Razor" && x.BranchId == BranchId).FirstOrDefault();
                        if (razorSecretId != null)
                        {
                            razorSecretId.SecretKey = vSEcomProviderDTO.RazorSecretKey.Trim();
                            razorSecretId.SecretId = RazorId.Trim();
                            razorSecretId.FlagEnable = vSEcomProviderDTO.RazorFlagEnabled;
                            razorSecretId.UpdatedDate = DateTime.UtcNow;
                            _context.Update(razorSecretId);
                            _context.SaveChanges();
                        }
                        else
                        {
                            subscriptionProvider.Provider = "Razor";
                            subscriptionProvider.SecretId = RazorId.Trim();
                            subscriptionProvider.SecretKey = vSEcomProviderDTO.RazorSecretKey.Trim();
                            subscriptionProvider.FlagEnable = vSEcomProviderDTO.RazorFlagEnabled;
                            subscriptionProvider.CreatedDate = DateTime.UtcNow;
                            subscriptionProvider.ProviderName = (int)Enums.PaymentOption.PaymentGateway3;
                            subscriptionProvider.BranchId = BranchId;
                            _context.SubscriptionProvider.Add(subscriptionProvider);
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        return BadRequest("Client Id or Secret Key is Invalid");
                    }
                }
                else
                {
                    return BadRequest("Client Id or Secret Key is Invalid");
                }
                //Apple
                if (!string.IsNullOrEmpty(vSEcomProviderDTO.AppleSecretId) && !string.IsNullOrEmpty(vSEcomProviderDTO.AppleSecretKey))
                {
                    var appleSecretId = _context.SubscriptionProvider.Where(x => x.Provider == "Apple" && x.BranchId == BranchId).FirstOrDefault();
                    if (appleSecretId != null)
                    {
                        appleSecretId.SecretKey = vSEcomProviderDTO.AppleSecretKey.Trim();
                        appleSecretId.SecretId = vSEcomProviderDTO.AppleSecretId.Trim();
                        appleSecretId.FlagEnable = vSEcomProviderDTO.AppleFlagEnabled;
                        appleSecretId.UpdatedDate = DateTime.UtcNow;
                        _context.Update(appleSecretId);
                        _context.SaveChanges();
                    }
                    else
                    {
                        subscriptionProvider.Provider = "Apple";
                        subscriptionProvider.SecretKey = vSEcomProviderDTO.AppleSecretKey.Trim();
                        subscriptionProvider.SecretId = vSEcomProviderDTO.AppleSecretId.Trim();
                        subscriptionProvider.FlagEnable = vSEcomProviderDTO.AppleFlagEnabled;
                        subscriptionProvider.CreatedDate = DateTime.UtcNow;
                        subscriptionProvider.ProviderName = (int)Enums.PaymentOption.PaymentGateway4;
                        subscriptionProvider.BranchId = BranchId;
                        _context.SubscriptionProvider.Add(subscriptionProvider);
                        _context.SaveChanges();
                    }
                }
                //Google
                if (!string.IsNullOrEmpty(vSEcomProviderDTO.GoogleSecretId) && !string.IsNullOrEmpty(vSEcomProviderDTO.GoogleSecretKey))
                {
                    var googlesecretId = _context.SubscriptionProvider.Where(x => x.Provider == "Google" && x.BranchId == BranchId).FirstOrDefault();
                    if (googlesecretId != null)
                    {
                        googlesecretId.SecretKey = vSEcomProviderDTO.GoogleSecretKey.Trim();
                        googlesecretId.SecretId = vSEcomProviderDTO.GoogleSecretId.Trim();
                        googlesecretId.FlagEnable = vSEcomProviderDTO.GoogleFlagEnabled;
                        googlesecretId.UpdatedDate = DateTime.UtcNow;
                        _context.Update(googlesecretId);
                        _context.SaveChanges();
                    }
                    else
                    {
                        subscriptionProvider.Provider = "Google";
                        subscriptionProvider.SecretKey = vSEcomProviderDTO.GoogleSecretKey.Trim();
                        subscriptionProvider.SecretId = vSEcomProviderDTO.GoogleSecretId.Trim();
                        subscriptionProvider.FlagEnable = vSEcomProviderDTO.GoogleFlagEnabled;
                        subscriptionProvider.CreatedDate = DateTime.UtcNow;
                        subscriptionProvider.ProviderName = (int)Enums.PaymentOption.PaymentGateway5;
                        subscriptionProvider.BranchId = BranchId;
                        _context.SubscriptionProvider.Add(subscriptionProvider);
                        _context.SaveChanges();
                    }
                }
                //other
                if (!string.IsNullOrEmpty(vSEcomProviderDTO.OtherSecretId) && !string.IsNullOrEmpty(vSEcomProviderDTO.OtherSecretKey))
                {
                    var otherSecretId = _context.SubscriptionProvider.Where(x => x.ProviderName == 9 && x.BranchId == BranchId).FirstOrDefault();
                    if (otherSecretId != null)
                    {
                        otherSecretId.Provider = (string?)(string.IsNullOrEmpty(vSEcomProviderDTO.Provider) ? (object)DBNull.Value : vSEcomProviderDTO.Provider.Trim());
                        otherSecretId.SecretKey = vSEcomProviderDTO.OtherSecretKey.Trim();
                        otherSecretId.SecretId = vSEcomProviderDTO.OtherSecretId.Trim();
                        otherSecretId.FlagEnable = vSEcomProviderDTO.OtherFlagEnabled;
                        otherSecretId.UpdatedDate = DateTime.UtcNow;
                        _context.Update(otherSecretId);
                        _context.SaveChanges();
                    }
                    else
                    {
                        subscriptionProvider.Provider = (string?)(string.IsNullOrEmpty(vSEcomProviderDTO.Provider) ? (object)DBNull.Value : vSEcomProviderDTO.Provider.Trim());
                        subscriptionProvider.SecretKey = vSEcomProviderDTO.OtherSecretKey.Trim();
                        subscriptionProvider.SecretId = vSEcomProviderDTO.OtherSecretId.Trim();
                        subscriptionProvider.FlagEnable = vSEcomProviderDTO.OtherFlagEnabled;
                        subscriptionProvider.CreatedDate = DateTime.UtcNow;
                        subscriptionProvider.ProviderName = (int)Enums.PaymentOption.OtherGateway;
                        subscriptionProvider.BranchId = BranchId;
                        _context.SubscriptionProvider.Add(subscriptionProvider);
                        _context.SaveChanges();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Payment_Write)]
        [HttpPost("Seller/{BranchId}/CashOptionDetails")]
        public async Task<IActionResult> CashOptionDetails(int BranchId,CashOptionDTO cashOptionDTO)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                SubscriptionProvider subscriptionProvider = new SubscriptionProvider();

                var cashOnDelivery = _context.SubscriptionProvider.Where(x => x.ProviderName == 1 && x.BranchId == BranchId).FirstOrDefault();
                if (cashOnDelivery != null)
                {
                    cashOnDelivery.FlagEnable = cashOptionDTO.CashOnDeliveryEnabled;
                    cashOnDelivery.UpdatedDate = DateTime.UtcNow;
                    _context.SubscriptionProvider.Update(cashOnDelivery);
                    _context.SaveChanges();
                }
                else
                {
                    subscriptionProvider.Provider = "CashOnDelivery";
                    subscriptionProvider.FlagEnable = cashOptionDTO.CashOnDeliveryEnabled;
                    subscriptionProvider.CreatedDate = DateTime.UtcNow;
                    subscriptionProvider.ProviderName = (int)Enums.PaymentOption.CashOnDelivery;
                    subscriptionProvider.BranchId = BranchId;
                    _context.SubscriptionProvider.Add(subscriptionProvider);
                    _context.SaveChanges();
                }
              
                var CardOnDelivery = _context.SubscriptionProvider.Where(x => x.ProviderName == 2 && x.BranchId == BranchId).FirstOrDefault();
                if (CardOnDelivery != null)
                {
                    CardOnDelivery.FlagEnable = cashOptionDTO.CardOnDeliveryEnbled;
                    CardOnDelivery.UpdatedDate = DateTime.UtcNow;
                    _context.SubscriptionProvider.Update(CardOnDelivery);
                    _context.SaveChanges();
                }
                else
                {
                    SubscriptionProvider subscriptionProvider1 = new SubscriptionProvider();
                    subscriptionProvider1.Provider = "CardOnDelivery";
                    subscriptionProvider1.FlagEnable = cashOptionDTO.CardOnDeliveryEnbled;
                    subscriptionProvider1.CreatedDate = DateTime.UtcNow;
                    subscriptionProvider1.ProviderName = (int)Enums.PaymentOption.CardOnDelivery;
                    subscriptionProvider1.BranchId = BranchId;
                    _context.SubscriptionProvider.Add(subscriptionProvider1);
                    _context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        //used in template
        [HttpGet("Seller/{BranchId}/GetEnabledProviderDetails")]
        public IActionResult GetEnabledProviderDetails(int BranchId)
        {
            try
            {
                List<EnabledProvidersResult> enabledProviders = new List<EnabledProvidersResult>();
                var subscriptionProviders = _context.SubscriptionProvider.Where(p => p.FlagEnable == true && p.BranchId == BranchId);
                foreach (var eachProvider in subscriptionProviders)
                {
                    EnabledProvidersResult enabledSubscription = new EnabledProvidersResult();
                    enabledSubscription.ProviderId = eachProvider.ProviderName;
                    enabledSubscription.ProviderName = eachProvider.Provider;
                    enabledProviders.Add(enabledSubscription);
                }

                return Ok(enabledProviders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        private string VerifyPayPalToken(string clientId, string clientSecret)
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
        private string VerifyRazorToken(string clientId, string clientSecret)
        {
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_appSettings.RazorToken.ToString());
                request.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(clientId + ":" + clientSecret));
                request.Accept = "application/json";
                request.Headers.Add("Accept-Language", "en_US");
                request.Method = "GET";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Timeout = 10000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse);
                return "verified successfully";
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
