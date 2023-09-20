using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    public class AccountController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly UserService _userService;
        private readonly MailHelper _mailHelper;

        public AccountController(DataContext context, UserService userService, MailHelper mailHelper)
        {
            _context = context;
            _userService = userService;
            _mailHelper = mailHelper;
        }

        [HttpPost("Seller/{BranchId}/RegisterStaff")]
        public IActionResult RegisterStaff(int BranchId, StaffDetailsDTO userModel)
        {
            try
            {
                var addStaff = _userService.AddStaffAccount(userModel);
                if (addStaff.Item1 == true)
                {
                    var user = _context.User.Where(x => x.UserId == addStaff.Item2).FirstOrDefault();
                    if (user != null)
                    {
                        if (_userService.AddSellerStaffMapping(userModel.StoreId, userModel.BranchId, user.UserId) == true)
                        {
                            PermissionDTO permission = new PermissionDTO();
                            permission.UserId = user.UserId;
                            permission.BranchId = userModel.BranchId;
                            permission.StoreId = userModel.StoreId;
                            permission.PermissionIds = userModel.PermissionIds;
                            AddPermissions(permission);
                            return Ok();
                        }
                        return BadRequest("StaffMapping not registered.");
                    }
                }
                return BadRequest("User not registered.");

            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [HttpPost("AddPermissions")]
        public IActionResult AddPermissions(PermissionDTO permission)
        {
            try
            {
                if (permission.PermissionIds != null)
                {
                    foreach (var permissionId in permission.PermissionIds)
                    {
                        var permissionExist = _context.UserPermissionMapping.Where(x => x.UserId == permission.UserId && x.PermissionId == permissionId && x.StoreId == permission.StoreId && x.BranchId == permission.BranchId).FirstOrDefault();
                        if (permissionExist == null)
                        {
                            UserPermissionMapping userPermissionMapping = new UserPermissionMapping();
                            userPermissionMapping.UserId = permission.UserId;
                            userPermissionMapping.PermissionId = permissionId;
                            userPermissionMapping.StoreId = permission.StoreId;
                            userPermissionMapping.BranchId = permission.BranchId;
                            userPermissionMapping.CreatedDate = DateTime.UtcNow;
                            _context.UserPermissionMapping.Add(userPermissionMapping);
                            _context.SaveChanges();
                        }
                        else
                        {
                            permissionExist.PermissionId = permissionId;
                            permissionExist.UpdatedDate = DateTime.UtcNow;
                            _context.UserPermissionMapping.Update(permissionExist);
                            _context.SaveChanges();
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("There is some error");
            }
        }

        [HttpDelete("Seller/{BranchId}/DeleteStaffAccount/{UserId}")]
        public IActionResult DeleteStaffAccount(int BranchId, int UserId)
        {
            try
            {
                var userDetails = _context.User.Where(x => x.UserId == UserId && x.IsSales == true && (x.Deleted == null || x.Deleted == false)).FirstOrDefault();
                if (userDetails != null)
                {
                    _userService.DeleteUser(userDetails.UserId);
                    return Ok();
                }
                return BadRequest("User Not Found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");

            }
        }

        [HttpGet("GetPermissions")]
        public IActionResult GetPermissions()
        {
            try
            {
                var permissions = _context.Permissions.ToList();
                if (permissions.Count > 0)
                {
                    return Ok(permissions);
                }
                return BadRequest("Permissions Not Found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpGet("Seller/{BranchId}/GetAddress")]
        public IActionResult GetAddress(int BranchId)
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
                AddressResult addressResult = new AddressResult();
                var branchDetails = _context.SellerBranch.Where(x => x.BranchId == BranchId).FirstOrDefault();
                if (branchDetails != null)
                {
                    addressResult.phonenumber = branchDetails.PhoneNumber;
                    addressResult.zipcode = branchDetails.PostalCode;
                    addressResult.city = branchDetails.City;
                    addressResult.state = branchDetails.State;
                    addressResult.address1 = branchDetails.Address1;
                    addressResult.country = _context.Country.Where(x => x.CountryId == branchDetails.Country).Select(x => x.Name).FirstOrDefault();
                    addressResult.address2 = branchDetails.Address2;
                }
                return Ok(addressResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpPost("Seller/{BranchId}/UpdateAddress")]
        public IActionResult UpdateAddress(int BranchId, UpdateAddressDTO updateAddressDTO)
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
                var addressDetails = _context.SellerBranch.Where(x => x.BranchId == BranchId).FirstOrDefault();
                if (addressDetails != null)
                {
                    addressDetails.Address1 = updateAddressDTO.Address1;
                    addressDetails.Address2 = updateAddressDTO.Address2;
                    addressDetails.City = updateAddressDTO.City;
                    addressDetails.Country = _context.Country.Where(x => x.Name == updateAddressDTO.Country).Select(x => x.CountryId).FirstOrDefault();
                    addressDetails.PhoneNumber = updateAddressDTO.Phonenumber;
                    addressDetails.State = updateAddressDTO.State;
                    addressDetails.PostalCode = updateAddressDTO.Zipcode;
                    _context.SellerBranch.Update(addressDetails);
                    _context.SaveChanges();
                    return Ok();
                }
                return BadRequest("Error while updating address.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpPost("Seller/{StoreId}/CheckEmailExists")]
        public IActionResult CheckEmailExists(int StoreId, CheckEmailDTO checkEmailDTO)
        {
            try
            {
                if (StoreId > 0)
                {
                    var storeIds = User.FindAll("StoreId").ToList();
                    if (!storeIds.Where(a => a.Value == StoreId.ToString()).Any())
                    {
                        return Unauthorized();
                    }
                }
                AddressResult addressResult = new AddressResult();
                var branchDetails = _context.User.Where(x => x.Email == checkEmailDTO.Email && (x.Deleted == null || x.Deleted == false)).FirstOrDefault();
                if (branchDetails == null)
                {
                    return Ok(false);
                }
                return Ok(true);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpGet("Seller/{BranchId}/GetAllStaffAccount")]
        public IActionResult GetAllStaffAccount(int BranchId)
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

                List<StaffAccountResult> staffAccountResultsList = new List<StaffAccountResult>();

                var staffdetails = _context.SellerStaffMapping.Where(x => x.BranchId == BranchId).ToList();
                if (staffdetails.Count > 0)
                {
                    foreach (var eachDetail in staffdetails)
                    {
                        var userDetails = _context.User.Where(x => x.UserId == eachDetail.UserId && x.Deleted == false).FirstOrDefault();
                        if (userDetails != null)
                        {
                            StaffAccountResult staffAccountResult = new StaffAccountResult();
                            staffAccountResult.email = userDetails.Email;
                            staffAccountResult.phoneNumber = userDetails.PhoneNumber1;
                            staffAccountResult.firstName = userDetails.FirstName;
                            staffAccountResult.lastName = userDetails.LastName;
                            staffAccountResult.id = userDetails.UserId;
                            staffAccountResultsList.Add(staffAccountResult);
                        }
                    }

                }
                return Ok(staffAccountResultsList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpPost("UpdateUserDetails")]
        public IActionResult UpdateUserDetails([FromForm] UpdateUserDTO updateUserDTO)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (userId != null)
                {
                    var user = _context.User.Where(x => x.UserId == Convert.ToInt32(userId)).FirstOrDefault();
                    if (user != null)
                    {
                        user.Email = updateUserDTO.Email;
                        user.FirstName = updateUserDTO.FirstName;
                        user.LastName = updateUserDTO.LastName;
                        user.PhoneNumber1 = updateUserDTO.PhoneNumber;
                        user.UpdatedOnUtc = DateTime.UtcNow;
                        _context.User.Update(user);
                        _context.SaveChanges();
                        return Ok("Details Updated Successfully");
                    }
                }
                return BadRequest("User Not Found");
            }
            catch (Exception ex)
            {
                return BadRequest("User Not Found");
            }
        }

        [HttpGet("ForgotPassword/{username}")]
        public bool ForgotPassword(string username)
        {
            var userDetails = _userService.GetUser(username);
            if (userDetails.UserId > 0)
            {
                string passwordResetToken = "";
                var execQuery = _userService.GenerateResetPasswordLinkQuery(username, out passwordResetToken);
                if (execQuery > 0)
                {
                    var user = _context.User.Where(x => (x.Email == username || x.PhoneNumber1 == username) && (x.Deleted == null || x.Deleted == false)).FirstOrDefault();
                    if(user != null)
                    {
                        var storeName = _context.Seller.Where(x => x.StoreId == user.StoreId).Select(x => x.StoreName).FirstOrDefault();
                        try
                        {
                            _mailHelper.SendForgetPasswordMailMerchant(user.Email, passwordResetToken, username, storeName);
                        }
                        catch
                        {

                        }
                        return true;
                    }
                }
            }
            return false;
        }

        [HttpPost("ResetPassword")]
        public bool ResetPassword([FromForm] ResetPasswordMerchantDTO resetPasswordDTO)
        {
            var userDetails = _userService.GetUser(resetPasswordDTO.UserName);

            if (userDetails.UserId > 0 && _userService.CheckUserDataInPasswordReset(resetPasswordDTO.UserName, resetPasswordDTO.PasswordResetToken))
            {
                if (_userService.UpdatePasswordMerchant(userDetails.UserId, resetPasswordDTO.NewPassword))
                {
                    _userService.PasswordResetCompeleted(resetPasswordDTO.UserName, resetPasswordDTO.PasswordResetToken);
                    return true;
                }
            }
            return false;
        }
    }
}
