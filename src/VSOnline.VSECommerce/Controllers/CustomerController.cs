using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data;
using VSOnline.VSECommerce.Domain;
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
    public class CustomerController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly DataContext _context;
        private readonly AppSettings _appSettings;
        public CustomerController(UserService userService, DataContext context, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _context = context;
            _appSettings = appSettings.Value;
        }

        [Authorize]
        [HttpGet("Seller/{StoreId}/Customer")]
        public IActionResult GetAllCustomerDetails(int StoreId)
        {
            try
            {
                var storeIds = User.FindAll("StoreId").ToList();
                if (!storeIds.Where(a => a.Value == StoreId.ToString()).Any())
                {
                    return Unauthorized();
                }
                List<CustomerResult> customeDetailsList = new List<CustomerResult>();
                var customerDetails = _context.User.Where(x => x.StoreId == StoreId && x.Deleted == false && x.IsMerchant == false).ToList();
                if (customerDetails.Count > 0)
                {
                    foreach (var eachData in customerDetails)
                    {
                        CustomerResult customerResult = new CustomerResult();
                        customerResult.UserId = eachData.UserId;
                        customerResult.FirstName = eachData.FirstName;
                        customerResult.LastName = eachData.LastName;
                        customerResult.StoreId = (int)eachData.StoreId;
                        customerResult.PhoneNumber1 = eachData.PhoneNumber1;
                        customerResult.Email = eachData.Email;
                        customeDetailsList.Add(customerResult);
                    }
                }
                return Ok(customeDetailsList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpGet("Seller/{StoreId}/Customer/{UserId}")]
        public IActionResult GetCustomerDetails(int StoreId, int UserId)

        {
            try
            {
                var storeIds = User.FindAll("StoreId").ToList();
                if (!storeIds.Where(a => a.Value == StoreId.ToString()).Any())
                {
                    return Unauthorized();
                }
                CustomerResult customerResult = new CustomerResult();

                var user = _context.User.Where(x => x.StoreId == StoreId && x.UserId == UserId).FirstOrDefault();
                if (user != null)
                {
                    customerResult.FirstName = user.FirstName;
                    customerResult.LastName = user.LastName;
                    customerResult.PhoneNumber1 = user.PhoneNumber1;
                    customerResult.Email = user.Email;
                    customerResult.UserId = user.UserId;
                    customerResult.StoreId = (int)user.StoreId;
                }
                return Ok(customerResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpPost("Seller/{StoreId}/Customer")]
        public IActionResult AddCustomer(int StoreId, CustomerDTO customerDTO)
        {
            BaseUpdateResultSet result = new BaseUpdateResultSet();
            result.Status = Enums.UpdateStatus.Failure;
            try
            {
                var storeIds = User.FindAll("StoreId").ToList();
                if (!storeIds.Where(a => a.Value == StoreId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if ((!string.IsNullOrEmpty(customerDTO.Email)) || (!string.IsNullOrEmpty(customerDTO.PhoneNumber1)))
                {
                    bool checkEmail = _userService.CheckEmailExist(customerDTO.Email, StoreId);
                    if (checkEmail)
                    {
                        return BadRequest("Email Already Exists");
                    }
                    bool checkPhoneNumber = _userService.CheckPhonenumberExist(customerDTO.PhoneNumber1, StoreId);
                    if (checkPhoneNumber)
                    {
                        return BadRequest("Phonenumber Already Exists");
                    }
                    User user = new User();
                    user.Password = _userService.GeneratePassword(customerDTO.Password, user.PasswordSalt);
                    user.Email = customerDTO.Email;
                    user.UserGuid = Guid.NewGuid();
                    user.FirstName = customerDTO.FirstName;
                    user.LastName = customerDTO.LastName;
                    user.PhoneNumber1 = customerDTO.PhoneNumber1;
                    user.PasswordFormatId = 1;
                    user.PasswordSalt = DateTime.Now.Year + "_VBuy.in";
                    user.Password = _userService.GeneratePassword(customerDTO.Password, user.PasswordSalt);
                    user.CreatedOnUtc = DateTime.UtcNow;
                    user.IsMerchant = false;
                    user.Deleted = false;
                    user.StoreId = StoreId;
                    _context.User.Add(user);
                    _context.SaveChanges();
                    return Ok("Success");
                }
                else
                {
                    return BadRequest("Email or phoneNumber required");
                }
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpPut("Seller/{StoreId}/Customer")]
        public IActionResult UpdateCustomer(int StoreId, CustomerDTO customerDTO)
        {
            BaseUpdateResultSet result = new BaseUpdateResultSet();
            result.Status = Enums.UpdateStatus.Failure;
            try
            {
                var storeIds = User.FindAll("StoreId").ToList();
                if (!storeIds.Where(a => a.Value == StoreId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var customerExist = _context.User.Where(x => x.Email == customerDTO.Email && x.UserId != customerDTO.UserId && x.StoreId == customerDTO.StoreId).ToList();
                if (customerExist.Count > 0)
                {
                    result.Status = Enums.UpdateStatus.AlreadyExist;
                }
                else
                {
                    var customerDetails = _context.User.Where(x => x.UserId == customerDTO.UserId && x.StoreId == customerDTO.StoreId).FirstOrDefault();
                    if (customerDetails != null)
                    {
                        customerDetails.FirstName = customerDTO.FirstName;
                        customerDetails.LastName = customerDTO.LastName;
                        customerDetails.Email = customerDTO.Email;
                        customerDetails.PhoneNumber1 = customerDTO.PhoneNumber1;
                        customerDetails.UpdatedOnUtc = DateTime.UtcNow;
                        _context.User.Update(customerDetails);
                        _context.SaveChanges();
                    }
                    result.Status = Enums.UpdateStatus.Success;
                }
                return Ok(result);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpDelete("Seller/{StoreId}/Customer/{UserId}")]
        public IActionResult DeleteCustomer(int StoreId, int UserId)
        {
            try
            {
                var storeIds = User.FindAll("StoreId").ToList();
                if (!storeIds.Where(a => a.Value == StoreId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var customer = _context.User.Where(x => x.StoreId == StoreId && x.UserId == UserId).FirstOrDefault();
                if (customer != null)
                {
                    customer.Deleted = true;
                    customer.UpdatedOnUtc = DateTime.UtcNow;
                    _context.User.Update(customer);
                    _context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpGet("Seller/{StoreId}/SearchCustomer/{searchString}")]
        public IActionResult SearchCustomer(int StoreId, string searchString)
        {
            try
            {
                var storeIds = User.FindAll("StoreId").ToList();
                if (!storeIds.Where(a => a.Value == StoreId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (!string.IsNullOrEmpty(searchString))
                {
                    List<CustomerResult> customeDetailsList = new List<CustomerResult>();
                    var customer = _context.User.Where(x => (x.Email.Contains(searchString) || x.PhoneNumber1.Contains(searchString) || x.FirstName.Contains(searchString)) && x.StoreId == StoreId && (x.Deleted == false || x.Deleted == null)).ToList();
                    if (customer != null)
                    {
                        foreach (var eachData in customer)
                        {
                            CustomerResult customerResult = new CustomerResult();
                            customerResult.UserId = eachData.UserId;
                            customerResult.FirstName = eachData.FirstName;
                            customerResult.LastName = eachData.LastName;
                            customerResult.Email = eachData.Email;
                            customerResult.PhoneNumber1 = eachData.PhoneNumber1;
                            customerResult.StoreId = (int)eachData.StoreId;
                            customeDetailsList.Add(customerResult);
                        }
                        return Ok(customeDetailsList);
                    }
                }
                return Ok(new List<CustomerResult>());
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

    }
}
