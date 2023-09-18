using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
    public class LoginController : VSControllerBase
    {
        private readonly UserService _userService;
        private readonly DataContext _context;
        private readonly EfContext _efContext;
        private readonly MailHelper _mailHelper;
        private readonly FileUploadController _fileUploadController;
        private readonly RatingHelper _ratingHelper;
        public LoginController(UserService userService, DataContext context, EfContext efContext, MailHelper mailHelper, IOptions<AppSettings> _appSettings, FileUploadController fileUploadController, RatingHelper ratingHelper) : base(_appSettings)
        {
            _userService = userService;
            _efContext = efContext;
            _context = context;
            _mailHelper = mailHelper;
            _fileUploadController = fileUploadController;
            _ratingHelper = ratingHelper;
        }

        [HttpGet("Seller/{StoreId}/ForgotPassword/{username}")]
        public bool ForgotPassword(int StoreId, string username)
        {
            if (_userService.CheckUserExist(StoreId, username))
            {
                string passwordResetToken = "";
                var execQuery = _userService.GenerateResetPasswordLinkQuery(StoreId, username, out passwordResetToken);
                if (execQuery > 0)
                {
                    var user = _context.User.Where(x => (x.Email == username || x.PhoneNumber1 == username) && x.StoreId == StoreId).FirstOrDefault();
                    var storeName = _context.Seller.Where(x => x.StoreId == StoreId).Select(x => x.StoreName).FirstOrDefault();
                    try
                    {
                        _mailHelper.SendForgetPasswordMail(user.Email, passwordResetToken, username, storeName);
                    }
                    catch
                    {

                    }
                    return true;
                }
            }
            return false;
        }
        
        [HttpPost("Seller/{StoreId}/ResetPassword")]
        public bool ResetPassword(int StoreId, ResetPasswordDTO resetPasswordDTO)
        {
            if (_userService.CheckUserExist(StoreId, resetPasswordDTO.UserName) && _userService.CheckUserDataInPasswordReset(StoreId,resetPasswordDTO.UserName, resetPasswordDTO.UniqueId))
            {
                if (_userService.UpdatePassword(StoreId, resetPasswordDTO.UserName, resetPasswordDTO.Password))
                {
                    _userService.PasswordResetCompeleted(StoreId,resetPasswordDTO.UserName, resetPasswordDTO.UniqueId);
                    return true;
                }
            }
            return false;
        }

        [HttpPost("Seller/{StoreId}/RegisterUser")]
        public IActionResult RegisterUser(int StoreId, [FromBody] UserModelDTO userModel)
        {
            bool checkEmail = _userService.CheckEmailExist(userModel.Email, StoreId);
            if (checkEmail)
            {
                return BadRequest("Email Already Exists");
            }
            bool checkPhoneNumber = _userService.CheckPhonenumberExist(userModel.PhoneNumber1, StoreId);
            if (checkPhoneNumber)
            {
                return BadRequest("Phonenumber Already Exists");
            }
            if (_userService.AddUser(StoreId, userModel) == true)
            {
                var storeName = _context.Seller.Where(x => x.StoreId == StoreId).Select(x => x.StoreName).FirstOrDefault();
                try
                {
                    if (!string.IsNullOrEmpty(userModel.Email))
                    {
                        _mailHelper.SendWelcomeMail(userModel.Email, storeName);
                    }
                }
                catch
                {

                }
                return Ok();
            }
            return BadRequest("User not registered.");
        }

        [Authorize(Roles = "Administrators , StoreAdmin , StoreModerator, Registered, SalesSupport, Support, Marketing")]
        [HttpPost("Seller/{StoreId}/ChangePassword")]
        public bool ChangePassword(int StoreId, ChangePasswordDTO changePasswordDTO)
        {
            if (_userService.CheckUserExist(StoreId, changePasswordDTO.UserName) && _userService.ValidateUser(StoreId, changePasswordDTO.UserName, changePasswordDTO.CurrentPassword))
            {
                return _userService.UpdatePassword(StoreId, changePasswordDTO.UserName, changePasswordDTO.NewPassword);
            }

            return false;
        }

        [HttpGet("Seller/{StoreId}/GetMyDetails/{username}")]
        public UserResult GetMyDetails(int StoreId, string username)
        {
            try
            {
                UserResult userDTO = new UserResult();
                if (_userService.CheckUserExist(StoreId, username))
                {
                    var currentUser = User.Identity.Name;
                    if (username == currentUser)
                    {
                        var user = _context.User.Where(x => (x.Email == username || x.PhoneNumber1 == username) && x.StoreId == StoreId).FirstOrDefault();
                        if(user != null)
                        {
                            userDTO.FirstName = user.FirstName;
                            userDTO.LastName = user.LastName;
                            userDTO.PhoneNumber1 = user.PhoneNumber1;
                            userDTO.Email = user.Email;
                            userDTO.UserId = user.UserId;
                        }
                    }
                    return userDTO;
                }
                return userDTO;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Authorize]
        [HttpPost("Seller/{StoreId}/UpdateUserDetails")]
        public IActionResult UpdateUserDetails(int StoreId, VbuyUserModelDTO vbuyUserModelDTO)
        {
            try
            {
                var userId = User.FindAll("UserId").ToList();
                if (!userId.Where(a => a.Value == vbuyUserModelDTO.UserId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (string.IsNullOrEmpty(vbuyUserModelDTO.Email) && string.IsNullOrEmpty(vbuyUserModelDTO.PhoneNumber1))
                {
                    return BadRequest("Email or PhoneNumber Required");
                }
                var checkPhoneNumber = _context.User.Where(x => x.UserId != vbuyUserModelDTO.UserId && x.PhoneNumber1 == vbuyUserModelDTO.PhoneNumber1 && x.StoreId == StoreId && x.Deleted == false).FirstOrDefault();
                if (checkPhoneNumber != null)
                {
                    return BadRequest("PhoneNumber already exists");
                }
                var checkEmail = _context.User.Where(x => x.UserId != vbuyUserModelDTO.UserId && x.Email == vbuyUserModelDTO.Email && x.StoreId == StoreId && x.Deleted == false).FirstOrDefault();
                if (checkEmail != null)
                {
                    return BadRequest("Email already exists");
                }
                var updateUser = _userService.UpdateUserDetails(StoreId,vbuyUserModelDTO);
                if (updateUser)
                {
                    return Ok("User updated sucessfully");
                }
                return BadRequest("Failed to update user");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        //if email change in myaccount page cp(ui)
        [HttpPost("UpdateMyaccount")]
        public IActionResult UpdateMyaccount([FromBody] MyaccountDTO myaccountDTO)
        {
            try
            {
                var userId = User.FindFirst("UserId")?.Value;
                if (userId != null)
                {
                    var seller = _context.Seller.Where(x => x.PrimaryContact == Convert.ToInt16(userId)).FirstOrDefault();
                    if (seller != null)
                    {
                        var sellerBranchDetails = _context.SellerBranch.Where(x => x.Store == seller.StoreId).ToList();
                        if (sellerBranchDetails.Count > 0)
                        {
                            foreach (var branch in sellerBranchDetails)
                            {
                                branch.Email = myaccountDTO.Email;
                                branch.PhoneNumber = myaccountDTO.PhoneNumber1;
                                _context.Entry(branch).State = EntityState.Modified;
                            }
                            _context.SaveChanges();
                        }
                        var userDetails = _context.User.Where(x => x.UserId == seller.PrimaryContact).FirstOrDefault();
                        if (userDetails != null)
                        {
                            userDetails.FirstName = myaccountDTO.FirstName;
                            userDetails.LastName = myaccountDTO.LastName;
                            userDetails.Email = myaccountDTO.Email;
                            userDetails.PhoneNumber1 = myaccountDTO.PhoneNumber1;
                            _context.User.Update(userDetails);
                            _context.SaveChanges();
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
            }
            return BadRequest("User not registered.");
        }

        [HttpPost("ChangePasswordMerchant")]
        public bool ChangePasswordMerchant(MyAccountChangePasswordDTO myAccountChangePasswordDTO)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (userId != null)
            {
                var userDetails = _context.Seller.Where(x => x.PrimaryContact == Convert.ToInt16(userId)).FirstOrDefault();
                if (userDetails != null)
                {
                    if (_userService.ValidateMyaccount(userDetails.PrimaryContact, myAccountChangePasswordDTO.CurrentPassword))
                    {
                        return _userService.UpdatePasswordMyaccount(userDetails.PrimaryContact, myAccountChangePasswordDTO.NewPassword);
                    }
                }
            }
            return false;
        }

        [HttpPost("EditProfileDetail")]
        public ActionResult<EditProfileResult> EditProfileDetail(EditProfileDTO editProfile)
        {
            var currentUserId = User.FindFirst("UserId")?.Value;
            try
            {
                var curUser = _context.User.Where(x => x.UserId == editProfile.UserId).FirstOrDefault();
                if (curUser != null)
                {
                    curUser.Email = editProfile.Email;
                    curUser.FirstName = editProfile.FirstName;
                    curUser.LastName = editProfile.LastName;
                    curUser.PhoneNumber1 = editProfile.PhoneNumber1;
                    _context.User.Update(curUser);
                    var changes = _context.SaveChanges();
                    if (changes > 0)
                    {
                        return Ok("User updated sucessfully");
                    }
                    return StatusCode(STATUSCODE_FAILURE, "Error while updtating");
                }
                return StatusCode(STATUSCODE_FAILURE, "User Not Found");
            }
            catch
            {
                return StatusCode(STATUSCODE_ERROR, "There is some error");
            }
        }

        [HttpGet("GetProfileDetail")]
        public ActionResult<EditProfileResult> GetProfileDetail()
        {
            var userId = Convert.ToInt64(User.FindFirst("UserId")?.Value);
            EditProfileResult editProfile = new EditProfileResult();
            try
            {
                var result = _context.User.Where(x => x.UserId == userId).FirstOrDefault();
                if (result != null)
                {
                    editProfile.UserId = result.UserId;
                    editProfile.PhoneNumber1 = result.PhoneNumber1;
                    editProfile.Email = result.Email;
                    editProfile.FirstName = result.FirstName;
                    editProfile.LastName = result.LastName;
                    return Ok(editProfile);
                }
                return StatusCode(STATUSCODE_FAILURE, editProfile);
            }
            catch (Exception ex)
            {
                return StatusCode(STATUSCODE_ERROR, "There is some error");
            }
        }

        [HttpGet("GetUser")]
        public IActionResult GetUser()
        {
            try
            {
                UserResult userResult = new UserResult();
                var userId = User.FindFirst("UserId")?.Value;
                if (userId != null)
                {
                    var user = _context.User.Where(x => x.UserId == Convert.ToInt32(userId)).FirstOrDefault();
                    if (user != null)
                    {
                        userResult.UserId = user.UserId;
                        userResult.Email = user.Email;
                        userResult.FirstName = user.FirstName;
                        userResult.LastName = user.LastName;
                        userResult.PhoneNumber1 = user.PhoneNumber1;
                        userResult.IsMerchant = user.IsMerchant;
                    }
                    return Ok(userResult);
                }
                return BadRequest("User Not Found");
            }
            catch (Exception ex)
            {
                return BadRequest("User Not Found");
            }
        }

        //For vbuy
        [HttpPost("RegisterVbuyUser")]
        public IActionResult RegisterVbuyUser([FromBody] VbuyUserModelDTO vbuyUserModelDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bool checkEmail = _userService.CheckEmailExistForVbuy(vbuyUserModelDTO.Email);
                if (checkEmail)
                {
                    return BadRequest("Email Already Exists");
                }
                bool checkPhoneNumber = _userService.CheckPhonenumberExistForVbuy(vbuyUserModelDTO.PhoneNumber1);
                if (checkPhoneNumber)
                {
                    return BadRequest("Phonenumber Already Exists");
                }

                if (_userService.AddVbuyUser(vbuyUserModelDTO) == true)
                {
                    var storeName = "Vbuy";

                    if (!string.IsNullOrEmpty(vbuyUserModelDTO.Email))
                    {
                        _mailHelper.SendWelcomeMail(vbuyUserModelDTO.Email, storeName);
                    }

                    return Ok();
                }
                return BadRequest("User not registered.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("GetMyDetailsForVbuy/{username}")]
        public UserResult GetMyDetailsForVbuy(string username)
        {
            try
            {
                UserResult userDTO = new UserResult();
                var userId = User.FindFirst("UserId")?.Value;
                if (_userService.CheckUserExistForVbuy(username, Convert.ToInt32(userId)))
                {
                    var currentUser = User.Identity.Name;
                    if (username == currentUser)
                    {
                        var user = _context.User.Where(x => (x.Email == username || x.PhoneNumber1 == username) && x.UserId == Convert.ToInt32(userId)).FirstOrDefault();
                        userDTO.FirstName = user.FirstName;
                        userDTO.LastName = user.LastName;
                        userDTO.PhoneNumber1 = user.PhoneNumber1;
                        userDTO.Email = user.Email;
                        userDTO.UserId = user.UserId;
                        if (user.UserProfileImage != null)
                        {
                            if (!user.UserProfileImage.Contains("http"))
                            {
                                userDTO.UserProfileImage = _appSettings.ImageUrlBase + user.UserProfileImage;
                            }
                            else
                            {
                                userDTO.UserProfileImage = user.UserProfileImage;
                            }
                        }
                    }
                    return userDTO;
                }
                return userDTO;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Authorize(Roles = "Administrators , StoreAdmin , StoreModerator, Registered, SalesSupport, Support, Marketing")]
        [HttpPost("ChangePassword")]
        public bool ChangePasswordForVbuy(ChangePasswordDTO changePasswordDTO)
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (_userService.CheckUserExistForVbuy(changePasswordDTO.UserName, Convert.ToInt32(userId)) && _userService.ValidateVbuyUser(changePasswordDTO.UserName, changePasswordDTO.CurrentPassword))
            {
                return _userService.UpdatePasswordForVbuy(changePasswordDTO.UserName, changePasswordDTO.NewPassword);
            }
            return false;
        }

        [HttpGet("ForgotPasswordForVbuy/{username}")]
        public bool ForgotPasswordForVbuy(string username)
        {
            if (_userService.CheckUserExistForVbuy(username))
            {
                string passwordResetToken = "";
                var execQuery = _userService.GenerateResetPasswordLinkQueryForVbuy(username, out passwordResetToken);
                if (execQuery > 0)
                {
                    var user = _context.User.Where(x => (x.Email == username || x.PhoneNumber1 == username) && x.IsVbuyUser == true).FirstOrDefault();
                    try
                    {
                        _mailHelper.SendForgetPasswordMailForVbuy(user.Email, passwordResetToken, username);
                    }
                    catch
                    {

                    }
                    return true;
                }
            }
            return false;
        }

        [HttpPost("ResetPasswordForVbuy")]
        public bool ResetPasswordForVbuy(ResetPasswordDTO resetPasswordDTO)
        {
            if (_userService.CheckUserExistForVbuy(resetPasswordDTO.UserName) && _userService.CheckUserDataInPasswordResetForVbuy(resetPasswordDTO.UserName, resetPasswordDTO.UniqueId))
            {
                if (_userService.UpdatePasswordForVbuy(resetPasswordDTO.UserName, resetPasswordDTO.Password))
                {
                    _userService.PasswordResetCompeletedForVbuy(resetPasswordDTO.UserName, resetPasswordDTO.UniqueId);
                    return true;
                }
            }
            return false;
        }

        [Authorize]
        [HttpPost("UpdateVbuyUser")]
        public IActionResult UpdateVbuyUser(VbuyUserModelDTO vbuyUserModelDTO)
        {
            try
            {
                var userId = User.FindAll("UserId").ToList();
                if (!userId.Where(a => a.Value == vbuyUserModelDTO.UserId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if(string.IsNullOrEmpty(vbuyUserModelDTO.Email) && string.IsNullOrEmpty(vbuyUserModelDTO.PhoneNumber1))
                {
                    return BadRequest("Email or PhoneNumber Required");
                }
                var checkPhoneNumber = _context.User.Where(x => x.UserId != vbuyUserModelDTO.UserId && x.PhoneNumber1 == vbuyUserModelDTO.PhoneNumber1 && x.IsVbuyUser == true && x.Deleted == false).FirstOrDefault();
                if(checkPhoneNumber != null)
                {
                    return BadRequest("PhoneNumber already exists");
                }
                var checkEmail  = _context.User.Where(x => x.UserId != vbuyUserModelDTO.UserId && x.Email == vbuyUserModelDTO.Email && x.IsVbuyUser == true && x.Deleted == false).FirstOrDefault();
                if (checkEmail != null)
                {
                    return BadRequest("Email already exists");
                }
                var updateUser =  _userService.UpdateVbuyUser(vbuyUserModelDTO);
                if (updateUser)
                {
                    return Ok("User updated sucessfully");
                }
                return BadRequest("Failed to update user");
            }
            catch(Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize]
        [HttpDelete("DeleteVbuyUser/{userID}")]
        public IActionResult DeleteVbuyUser(int userID)
        {
            try
            {
                var userId = User.FindAll("UserId").ToList();
                if (!userId.Where(a => a.Value == userID.ToString()).Any())
                {
                    return Unauthorized();
                }
                var updateUser = _userService.DeleteVbuyUser(userID);
                if (updateUser)
                {
                    return Ok("User deleted sucessfully");
                }
                return BadRequest("Failed to delete user");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

    }
    public class UserDetails
    {
        public int userId { get; set; }
        public string UserName { get; set; }
        public bool? Deleted { get; set; }
        public string? Email { get; set; }
        public bool IsMerchant { get; set; }
        public string? UserProfileImage { get; set; }   
    }
    public class UserDetailDTO
    {
        public List<int> userId { get; set; }
    }
    public class ProductDetailDTO
    {
        public List<int> ProductId { get; set; }
    }
    public class productDetails
    {
        public int productId { get; set; }
        public string ProductName { get; set; }
    }
}
