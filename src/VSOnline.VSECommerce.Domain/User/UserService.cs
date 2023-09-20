using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;
using XSystem.Security.Cryptography;

namespace VSOnline.VSECommerce.Domain
{
    public class UserService
    {
        IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        private readonly DataContext _context;
        private readonly IConfigurationSection _appSettings;
        private readonly EfContext _efContext;
        private readonly MailHelper _mailHelper;

        public UserService(DataContext context, EfContext efContext, MailHelper mailHelper)
        {
            _context = context;
            _appSettings = _configuration.GetSection("AppSettings");
            _efContext = efContext;
            _mailHelper = mailHelper;
        }
        public bool ValidateUser(int StoreId, string userName, string password)
        {
            var user = _context.User.Where<User>(x => (x.Email == userName || x.PhoneNumber1 == userName) && x.StoreId == StoreId).FirstOrDefault<User>();
            if (user != null)
            {
                var passwordHash = GeneratePassword(password, user.PasswordSalt);
                var userEnterdPassword = "0x" + BitConverter.ToString(passwordHash).Replace("-", "");
                var dbPassword = "0x" + BitConverter.ToString(user.Password).Replace("-", "");
                return (userEnterdPassword == dbPassword);
            }
            else
            {
                var userDetails = _context.User.Where<User>(x => (x.Email == userName || x.PhoneNumber1 == userName) && x.IsMerchant == true).FirstOrDefault<User>();
                if (userDetails != null)
                {
                    var passwordHash = GeneratePassword(password, userDetails.PasswordSalt);
                    var userEnterdPassword = "0x" + BitConverter.ToString(passwordHash).Replace("-", "");
                    var dbPassword = "0x" + BitConverter.ToString(userDetails.Password).Replace("-", "");
                    return (userEnterdPassword == dbPassword);
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public byte[] GeneratePassword(string password, string passwordSalt)
        {
            SHA256Managed crypt = new SHA256Managed();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password + passwordSalt), 0, Encoding.UTF8.GetByteCount(password + passwordSalt));
            return crypto;
        }

        public TokenResult GetUserToken(int StoreId, string userName)
        {
            try
            {
                var token = GenerateJSONWebToken(StoreId, userName);
                if (token != null)
                {
                    TokenResult tokenResult = new TokenResult();
                    tokenResult.AccessToken = token.Item1;
                    tokenResult.ValidDateUTC = token.Item2;
                    return tokenResult;
                }
            }
            catch (Exception ex)
            {
            }
            return null;

        }

        private Tuple<string, DateTime> GenerateJSONWebToken(int StoreId, string userName)
        {
            try
            {
                var user = _context.User.Where(x => (x.Email == userName || x.PhoneNumber1 == userName) && x.Deleted == false && x.StoreId == StoreId).FirstOrDefault();
                if (user != null)
                {
                    var userRole = GetUserRoleForId(user.UserId);
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.GetValue<string>("Jwtkey")));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    var roles = new List<string>();
                    var claims = new List<Claim>();
                    claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userName.ToString()));
                    claims.Add(new Claim(ClaimTypes.Name, userName.ToString()));
                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                    claims.Add(new Claim("UserId", user.UserId.ToString()));
                    claims.Add(new Claim("StoreId", StoreId.ToString()));
                    var branchDetails = _context.SellerBranch.Where(x => x.Store == StoreId).ToList();
                    foreach (var branch in branchDetails)
                    {
                        claims.Add(new Claim("BranchId", branch.BranchId.ToString()));
                    }
                    var token = new JwtSecurityToken(_appSettings.GetValue<string>("JwtIssuer"), _appSettings.GetValue<string>("JwtIssuer"), claims, expires: DateTime.Now.AddDays(15), signingCredentials: credentials);
                    var ValidTo = Convert.ToDateTime(token.ValidTo);
                    var token1 = new JwtSecurityTokenHandler().WriteToken(token);
                    return new Tuple<string, DateTime>(token1, ValidTo);
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        private string GetUserRole(string userName)
        {
            var user = _context.User.Where<User>(x => (x.Email == userName || x.PhoneNumber1 == userName)).FirstOrDefault<User>();
            if (user.IsSuperAdmin == true)
            {
                return Enums.Role.Administrators.ToString();
            }
            else if (user.IsMerchant)
            {
                return Enums.Role.StoreModerator.ToString();
            }
            else if (user.IsSales == true)
            {
                return Enums.Role.SalesSupport.ToString();
            }
            else if (user.IsMarketing == true)
            {
                return Enums.Role.Marketing.ToString();
            }
            else if (user.IsSupport == true)
            {
                return Enums.Role.Support.ToString();
            }
            else if (user.UserId > 0)
            {
                return Enums.Role.Registered.ToString();
            }
            return Enums.Role.Guests.ToString();
        }

        public Tuple<string, DateTime> GenerateJwtToken(int userId, string userRole)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.GetValue<string>("Jwtkey")));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var userName = userId.ToString();
            var userDetail = _context.User.Where(x => x.UserId == userId).FirstOrDefault();

            if (userDetail != null)
            {
                userName = userDetail.Email;
                if (string.IsNullOrEmpty(userDetail.Email))
                {
                    userName = userDetail.PhoneNumber1;
                }
            }
            var claims = new List<Claim>();
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, userName.ToString()));
            claims.Add(new Claim(ClaimTypes.Role, userRole));
            claims.Add(new Claim("UserId", userId.ToString()));
            var userPermission = new List<string>();
            var sellerDetails = _context.Seller.Where(x => x.PrimaryContact == userId).ToList();
            var StoreIdList = new List<int>();
            if (sellerDetails.Count > 0)
            {
                foreach (var seller in sellerDetails)
                {
                    claims.Add(new Claim("StoreId", seller.StoreId.ToString()));
                    StoreIdList.Add(seller.StoreId);
                }
                foreach (var eachStore in StoreIdList)
                {
                    var branchDetails = _context.SellerBranch.Where(x => x.Store == eachStore).ToList();
                    foreach (var branch in branchDetails)
                    {
                        claims.Add(new Claim("BranchId", branch.BranchId.ToString()));
                    }
                }
                userPermission = GetUserPermission();
            }
            var sellerStaffDetails = _context.SellerStaffMapping.Where(x => x.UserId == userId).ToList();
            var StaffStoreIdList = new List<int>();
            if (sellerStaffDetails.Count > 0)
            {
                foreach (var seller in sellerStaffDetails)
                {
                    claims.Add(new Claim("StoreId", seller.StoreId.ToString()));
                    StaffStoreIdList.Add(seller.StoreId);
                }
                foreach (var eachStore in StaffStoreIdList)
                {
                    var branchDetails = _context.SellerBranch.Where(x => x.Store == eachStore).ToList();
                    foreach (var branch in branchDetails)
                    {
                        claims.Add(new Claim("BranchId", branch.BranchId.ToString()));
                        userPermission = GetUserPermissionListForId(userId, eachStore, branch.BranchId);
                    }
                }

            }
            foreach (var permission in userPermission)
            {
                claims.Add(new Claim(CustomClaimTypes.Permission, permission));
            }

            var token = new JwtSecurityToken(_appSettings.GetValue<string>("JwtIssuer"), _appSettings.GetValue<string>("JwtIssuer"), claims, expires: DateTime.Now.AddDays(15), signingCredentials: credentials);
            var ValidTo = Convert.ToDateTime(token.ValidTo);
            var token1 = new JwtSecurityTokenHandler().WriteToken(token);
            return new Tuple<string, DateTime>(token1, ValidTo);
        }
        public string GetUserRoleForId(int userId)
        {
            var user = _context.User.Where(x => x.UserId == userId).FirstOrDefault();
            if (user != null)
            {
                if (user.IsSuperAdmin == true)
                {
                    return Enums.Role.Administrators.ToString();
                }
                else if (user.IsMerchant)
                {
                    return Enums.Role.StoreModerator.ToString();
                }
                else if (user.IsSales == true)
                {
                    return Enums.Role.SalesSupport.ToString();
                }
                else if (user.IsMarketing == true)
                {
                    return Enums.Role.Marketing.ToString();
                }
                else if (user.IsSupport == true)
                {
                    return Enums.Role.Support.ToString();
                }
                else if (user.UserId > 0)
                {
                    return Enums.Role.Registered.ToString();
                }
            }
            return Enums.Role.Guests.ToString();
        }
        public User GetUser(int userId)
        {
            var user = _context.User.Where(x => x.UserId == userId).FirstOrDefault();
            return user;
        }
        public bool DeleteUser(int UserId)
        {
            try
            {
                var user = _context.User.Where<User>(x => x.UserId == UserId).FirstOrDefault<User>();
                if (user != null)
                {
                    user.Deleted = true;
                    user.UpdatedOnUtc = DateTime.UtcNow;
                    _context.User.Update(user);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Tuple<bool, int> AddStaffAccount(StaffDetailsDTO userModel)
        {
            int userId = 0;
            try
            {
                User user = new User();
                user.Password = GeneratePassword(userModel.Password, user.PasswordSalt);
                user.Email = userModel.Email;
                user.UserGuid = Guid.NewGuid();
                user.FirstName = userModel.FirstName;
                user.LastName = userModel.LastName;
                user.PhoneNumber1 = userModel.PhoneNumber;
                user.PasswordFormatId = 1;
                user.PasswordSalt = DateTime.Now.Year + "_VBuy.in";
                user.Password = GeneratePassword(userModel.Password, user.PasswordSalt);
                user.CreatedOnUtc = DateTime.UtcNow;
                user.IsMerchant = false;
                user.IsSales = true;
                user.StoreId = userModel.StoreId;
                _context.User.Add(user);
                _context.SaveChanges();
                userId = user.UserId;
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, userId);
            }
            return Tuple.Create(true, userId);
        }
        public bool AddSellerStaffMapping(int storeId, int branchId, int user)
        {
            try
            {
                var sellerStaffDetails = _context.SellerStaffMapping.Where(x => x.StoreId == storeId && x.BranchId == branchId && x.UserId == user).FirstOrDefault();
                if (sellerStaffDetails == null)
                {
                    SellerStaffMapping sellerStaffMapping = new SellerStaffMapping();
                    sellerStaffMapping.StoreId = storeId;
                    sellerStaffMapping.BranchId = branchId;
                    sellerStaffMapping.UserId = user;
                    sellerStaffMapping.CreatedOnUtc = DateTime.UtcNow;
                    _context.SellerStaffMapping.Add(sellerStaffMapping);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public User GetUser(string userName)
        {
            User user = new User();
            user = _context.User.Where<User>(x => (x.Email == userName || x.PhoneNumber1 == userName) && (x.Deleted == null || x.Deleted == false)).FirstOrDefault<User>();
            return user;
        }

        public bool CheckUserExist(int StoreId, string userName)
        {
            try
            {
                if (_context.User.Any(e => (e.Email == userName || e.PhoneNumber1 == userName) && e.StoreId == StoreId))
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public int GenerateResetPasswordLinkQuery(int StoreId, string username, out string passwordResetToken)
        {
            try
            {
                int affectedRows = 0;
                passwordResetToken = Convert.ToString(GenerateUniquePasswordResetToken(username));
                DateTime PasswordResetExpiration = DateTime.UtcNow.AddDays(1);

                var userInPasswordReset = _context.PasswordReset.Where(x => x.Username == username && x.StoreId == StoreId).FirstOrDefault();
                if (userInPasswordReset != null)
                {
                    userInPasswordReset.PasswordResetToken = passwordResetToken;
                    userInPasswordReset.PasswordResetExpiration = PasswordResetExpiration;
                    userInPasswordReset.FlagCompleted = false;
                    _context.PasswordReset.Update(userInPasswordReset);
                    affectedRows = _context.SaveChanges();
                }
                else
                {
                    PasswordReset passwordReset = new PasswordReset();
                    passwordReset.Username = username;
                    passwordReset.PasswordResetToken = passwordResetToken;
                    passwordReset.PasswordResetExpiration = PasswordResetExpiration;
                    passwordReset.StoreId = StoreId;
                    _context.PasswordReset.Add(passwordReset);
                    affectedRows = _context.SaveChanges();
                }
                return affectedRows;
            }
            catch (Exception ex)
            {
                passwordResetToken = null;
                return 0;
            }

        }
        private Guid GenerateUniquePasswordResetToken(string username)
        {
            return Guid.NewGuid();
        }
        public bool AddUser(int StoreId, UserModelDTO userModel)
        {
            try
            {
                User user = new User();

                user.Password = GeneratePassword(userModel.Password, user.PasswordSalt);
                user.Email = userModel.Email;
                user.UserGuid = Guid.NewGuid();
                user.FirstName = userModel.FirstName;
                user.LastName = userModel.LastName;

                user.PhoneNumber1 = userModel.PhoneNumber1;
                user.PasswordFormatId = 1;
                user.PasswordSalt = DateTime.Now.Year + "_VBuy.in";
                user.Password = GeneratePassword(userModel.Password, user.PasswordSalt);
                user.CreatedOnUtc = DateTime.UtcNow;
                user.IsMerchant = false;
                user.StoreId = StoreId;
                _context.User.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public bool UpdateUserDetails(int StoreId, VbuyUserModelDTO vbuyUserModelDTO)
        {
            try
            {
                var userDetail = _context.User.Where(x => x.UserId == vbuyUserModelDTO.UserId && x.StoreId == StoreId).FirstOrDefault();
                if (userDetail != null)
                {
                    userDetail.Email = vbuyUserModelDTO.Email;
                    userDetail.PhoneNumber1 = vbuyUserModelDTO.PhoneNumber1;
                    userDetail.FirstName = vbuyUserModelDTO.FirstName;
                    userDetail.LastName = vbuyUserModelDTO.LastName;
                    userDetail.UpdatedOnUtc = DateTime.UtcNow;
                    _context.User.Update(userDetail);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool UpdatePassword(int StoreId, string userName, string password)
        {
            try
            {
                var passwordSalt = DateTime.Now.Year + "_VBuy.in";
                var updatedPassword = GeneratePassword(password, passwordSalt);

                var user = _context.User.Where<User>(x => (x.Email == userName || x.PhoneNumber1 == userName) && x.StoreId == StoreId).FirstOrDefault<User>();
                if (user != null)
                {
                    user.Password = updatedPassword;
                    user.PasswordSalt = passwordSalt;
                    user.UpdatedOnUtc = DateTime.UtcNow;
                    _context.User.Update(user);
                    _context.SaveChanges();
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }
        public bool CheckEmailExist(string email, int StoreId)
        {
            try
            {
                var emailExists = _context.User.Where(x => x.Email == email && x.StoreId == StoreId && x.Deleted == false).Select(a => a.Email).FirstOrDefault();
                if (!string.IsNullOrEmpty(emailExists))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool CheckPhonenumberExist(string phoneNumber, int StoreId)
        {
            try
            {
                var phoneNumberExists = _context.User.Where(x => x.PhoneNumber1 == phoneNumber && x.StoreId == StoreId && x.Deleted == false).Select(a => a.PhoneNumber1).FirstOrDefault();
                if (!string.IsNullOrEmpty(phoneNumberExists))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<string> GetUserPermissionListForId(int userId, int storeId, int branchId)
        {
            var userPermission = _context.UserPermissionMapping.Where(x => x.UserId == userId && x.StoreId == storeId && x.BranchId == branchId).Select(x => x.PermissionId).ToList();
            var userPermissionList = new List<string>();
            foreach (var permission in userPermission)
            {
                var eachPermission = _context.Permissions.Where(x => x.Id == permission).Select(x => x.Permission).FirstOrDefault();
                userPermissionList.Add(eachPermission);
            }
            return userPermissionList;
        }
        public bool VBuyHighLevelUsers(string userName)
        {
            var userRole = GetUserRole(userName);

            if (userRole == Enums.Role.Administrators.ToString() || userRole == Enums.Role.Marketing.ToString() || userRole == Enums.Role.SalesSupport.ToString()
            || userRole == Enums.Role.Support.ToString())
            {
                return true;
            }
            return false;
        }

        public List<string> GetUserPermission()
        {
            var userPermission = _context.Permissions.Select(x => x.Permission).ToList();

            return userPermission;
        }
        public static class CustomClaimTypes
        {
            public const string Permission = "Application.Permission";
        }
        // for Hyperlocal
        public bool CheckEmailExistForVbuy(string email)
        {
            try
            {
                var emailExists = _context.User.Where(x => x.Email == email && x.IsVbuyUser == true && x.Deleted == false).Select(a => a.Email).FirstOrDefault();
                if (!string.IsNullOrEmpty(emailExists))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool CheckPhonenumberExistForVbuy(string phoneNumber)
        {
            try
            {
                var phoneNumberExists = _context.User.Where(x => x.PhoneNumber1 == phoneNumber && x.IsVbuyUser == true && x.Deleted == false).Select(a => a.PhoneNumber1).FirstOrDefault();
                if (!string.IsNullOrEmpty(phoneNumberExists))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool AddVbuyUser(VbuyUserModelDTO vbuyUserModelDTO)
        {
            try
            {
                User user = new User();

                user.Password = GeneratePassword(vbuyUserModelDTO.Password, user.PasswordSalt);
                user.Email = vbuyUserModelDTO.Email;
                user.UserGuid = Guid.NewGuid();
                user.FirstName = vbuyUserModelDTO.FirstName;
                user.PhoneNumber1 = vbuyUserModelDTO.PhoneNumber1;
                user.PasswordFormatId = 1;
                user.PasswordSalt = DateTime.Now.Year + "_VBuy.in";
                user.Password = GeneratePassword(vbuyUserModelDTO.Password, user.PasswordSalt);
                user.CreatedOnUtc = DateTime.UtcNow;
                user.IsMerchant = false;
                user.IsVbuyUser = true;
                user.StoreId = 0;

                _context.User.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        public bool ValidateVbuyUser(string userName, string password)
        {

            var userDetails = _context.User.Where<User>(x => (x.Email == userName || x.PhoneNumber1 == userName) && x.IsVbuyUser == true && x.Deleted == false).FirstOrDefault<User>();
            if (userDetails != null)
            {
                var passwordHash = GeneratePassword(password, userDetails.PasswordSalt);
                var userEnterdPassword = "0x" + BitConverter.ToString(passwordHash).Replace("-", "");
                var dbPassword = "0x" + BitConverter.ToString(userDetails.Password).Replace("-", "");
                return (userEnterdPassword == dbPassword);
            }
            else
            {
                return false;
            }
        }
        public TokenResult GetVbuyUserToken(string userName)
        {
            try
            {
                var token = GenerateTokenForVbuy(userName);
                if (token != null)
                {
                    TokenResult tokenResult = new TokenResult();
                    tokenResult.AccessToken = token.Item1;
                    tokenResult.ValidDateUTC = token.Item2;
                    return tokenResult;
                }
            }
            catch (Exception ex)
            {
            }
            return null;

        }
        private Tuple<string, DateTime> GenerateTokenForVbuy(string userName)
        {
            try
            {
                var user = _context.User.Where(x => (x.Email == userName || x.PhoneNumber1 == userName) && x.Deleted == false && x.IsVbuyUser == true).FirstOrDefault();
                if (user != null)
                {
                    var userRole = GetUserRoleForId(user.UserId);
                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.GetValue<string>("Jwtkey")));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                    var roles = new List<string>();
                    var claims = new List<Claim>();
                    claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userName.ToString()));
                    claims.Add(new Claim(ClaimTypes.Name, userName.ToString()));
                    claims.Add(new Claim(ClaimTypes.Role, userRole));
                    claims.Add(new Claim("UserId", user.UserId.ToString()));
                    var token = new JwtSecurityToken(_appSettings.GetValue<string>("JwtIssuer"), _appSettings.GetValue<string>("JwtIssuer"), claims, expires: DateTime.Now.AddDays(15), signingCredentials: credentials);
                    var ValidTo = Convert.ToDateTime(token.ValidTo);
                    var token1 = new JwtSecurityTokenHandler().WriteToken(token);
                    return new Tuple<string, DateTime>(token1, ValidTo);
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        public bool CheckUserExistForVbuy(string userName, int userId)
        {
            try
            {
                if (_context.User.Any(e => (e.Email == userName || e.PhoneNumber1 == userName) && e.UserId == userId && e.IsVbuyUser == true))
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public bool UpdatePasswordForVbuy(string userName, string password)
        {
            try
            {
                var passwordSalt = DateTime.Now.Year + "_VBuy.in";
                var updatedPassword = GeneratePassword(password, passwordSalt);

                var user = _context.User.Where<User>(x => (x.Email == userName || x.PhoneNumber1 == userName) && x.IsVbuyUser == true).FirstOrDefault<User>();
                if (user != null)
                {
                    user.Password = updatedPassword;
                    user.PasswordSalt = passwordSalt;
                    user.UpdatedOnUtc = DateTime.UtcNow;
                    _context.User.Update(user);
                    _context.SaveChanges();
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }
        public bool CheckUserExistForVbuy(string userName)
        {
            try
            {
                if (_context.User.Any(e => (e.Email == userName || e.PhoneNumber1 == userName) && e.IsVbuyUser == true))
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
        public int GenerateResetPasswordLinkQueryForVbuy(string username, out string passwordResetToken)
        {
            try
            {
                //Check if the user already exist in password reset .

                //else insert 
                int affectedRows = 0;
                passwordResetToken = Convert.ToString(GenerateUniquePasswordResetToken(username));
                DateTime PasswordResetExpiration = DateTime.UtcNow.AddDays(1);

                var userInPasswordReset = _context.PasswordReset.Where(x => x.Username == username && x.IsVbuyUser == true).FirstOrDefault();
                if (userInPasswordReset != null)
                {
                    userInPasswordReset.PasswordResetToken = passwordResetToken;
                    userInPasswordReset.PasswordResetExpiration = PasswordResetExpiration;
                    userInPasswordReset.FlagCompleted = false;
                    _context.PasswordReset.Update(userInPasswordReset);
                    affectedRows = _context.SaveChanges();
                }
                else
                {
                    PasswordReset passwordReset = new PasswordReset();
                    passwordReset.Username = username;
                    passwordReset.PasswordResetToken = passwordResetToken;
                    passwordReset.PasswordResetExpiration = PasswordResetExpiration;
                    passwordReset.IsVbuyUser = true;
                    _context.PasswordReset.Add(passwordReset);
                    affectedRows = _context.SaveChanges();
                }
                return affectedRows;
            }
            catch (Exception ex)
            {
                passwordResetToken = null;
                return 0;
            }

        }
        // for myaccount in ui
        public bool ValidateMyaccount(int userId, string password)
        {

            var userDetails = _context.User.Where<User>(x => x.UserId == userId).FirstOrDefault<User>();
            if (userDetails != null)
            {
                var passwordHash = GeneratePassword(password, userDetails.PasswordSalt);
                var userEnterdPassword = "0x" + BitConverter.ToString(passwordHash).Replace("-", "");
                var dbPassword = "0x" + BitConverter.ToString(userDetails.Password).Replace("-", "");
                return (userEnterdPassword == dbPassword);
            }
            else
            {
                return false;
            }
        }
        public bool UpdatePasswordMyaccount(int userId, string password)
        {
            try
            {
                var passwordSalt = DateTime.Now.Year + "_VBuy.in";
                var updatedPassword = GeneratePassword(password, passwordSalt);

                var user = _context.User.Where<User>(x => x.UserId == userId && x.IsMerchant == true).FirstOrDefault<User>();
                if (user != null)
                {
                    user.Password = updatedPassword;
                    user.PasswordSalt = passwordSalt;
                    user.UpdatedOnUtc = DateTime.UtcNow;
                    _context.User.Update(user);
                    _context.SaveChanges();
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }
        public bool CheckUserDataInPasswordReset(int StoreId, string username, string uniqueId)
        {
            try
            {
                var userInPasswordReset = _context.PasswordReset.Where(x => x.Username == username && x.PasswordResetToken == uniqueId && x.FlagCompleted == false && x.StoreId == StoreId).Select(x => x.Username).FirstOrDefault();
                return !string.IsNullOrEmpty(userInPasswordReset);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void PasswordResetCompeleted(int StoreId, string username, string uniqueId)
        {
            try
            {
                var updateFlagQuery = _context.PasswordReset.Where(x => x.Username == username && x.PasswordResetToken == uniqueId && x.StoreId == StoreId).FirstOrDefault();
                if (updateFlagQuery != null)
                {
                    updateFlagQuery.FlagCompleted = true;
                    _context.PasswordReset.Update(updateFlagQuery);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public bool CheckUserDataInPasswordResetForVbuy(string username, string uniqueId)
        {
            try
            {
                var userInPasswordReset = _context.PasswordReset.Where(x => x.Username == username && x.PasswordResetToken == uniqueId && x.FlagCompleted == false && x.IsVbuyUser == true).Select(x => x.Username).FirstOrDefault();
                return !string.IsNullOrEmpty(userInPasswordReset);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public void PasswordResetCompeletedForVbuy(string username, string uniqueId)
        {
            try
            {
                var updateFlagQuery = _context.PasswordReset.Where(x => x.Username == username && x.PasswordResetToken == uniqueId && x.IsVbuyUser == true).FirstOrDefault();
                if (updateFlagQuery != null)
                {
                    updateFlagQuery.FlagCompleted = true;
                    _context.PasswordReset.Update(updateFlagQuery);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public bool UpdateVbuyUser(VbuyUserModelDTO vbuyUserModelDTO)
        {
            try
            {
                var vbuyUser = _context.User.Where(x => x.UserId == vbuyUserModelDTO.UserId && x.IsVbuyUser == true).FirstOrDefault();
                if (vbuyUser != null)
                {
                    vbuyUser.Email = vbuyUserModelDTO.Email;
                    vbuyUser.PhoneNumber1 = vbuyUserModelDTO.PhoneNumber1;
                    vbuyUser.FirstName = vbuyUserModelDTO.FirstName;
                    vbuyUser.UpdatedOnUtc = DateTime.UtcNow;
                    vbuyUser.IsVbuyUser = true;
                    _context.User.Update(vbuyUser);
                    _context.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public bool DeleteVbuyUser(int userId)
        {
            try
            {
                var vbuyUser = _context.User.Where(x => x.UserId == userId && x.IsVbuyUser == true).FirstOrDefault();
                if (vbuyUser != null)
                {
                    vbuyUser.Deleted = true;
                    vbuyUser.UpdatedOnUtc = DateTime.UtcNow;
                    _context.User.Update(vbuyUser);
                    _context.SaveChanges();
                    if (!string.IsNullOrEmpty(vbuyUser.Email))
                    {
                        var userName = vbuyUser.FirstName + " " + vbuyUser.LastName;
                        _mailHelper.SendDeleteInformationMail(vbuyUser.Email, userName);
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public int ValidateVSUsers(string userName, string password)
        {
            var userDetails = _context.User.Where<User>(x => (x.Email == userName || x.PhoneNumber1 == userName) && x.IsMerchant == true && (x.Deleted == null || x.Deleted == false)).FirstOrDefault<User>();
            if (userDetails != null)
            {
                var passwordHash = GeneratePassword(password, userDetails.PasswordSalt);
                var userEnterdPassword = "0x" + BitConverter.ToString(passwordHash).Replace("-", "");
                var dbPassword = "0x" + BitConverter.ToString(userDetails.Password).Replace("-", "");
                if (userEnterdPassword == dbPassword)
                {
                    return userDetails.UserId;
                }
            }
            else
            {
                var staffDetails = _context.User.Where<User>(x => (x.Email == userName || x.PhoneNumber1 == userName) && x.IsSales == true && (x.Deleted == null || x.Deleted == false)).FirstOrDefault<User>();
                if (staffDetails != null)
                {
                    var passwordHash = GeneratePassword(password, staffDetails.PasswordSalt);
                    var userEnterdPassword = "0x" + BitConverter.ToString(passwordHash).Replace("-", "");
                    var dbPassword = "0x" + BitConverter.ToString(staffDetails.Password).Replace("-", "");
                    if (userEnterdPassword == dbPassword)
                    {
                        return staffDetails.UserId;
                    }
                }
            }
            return 0;

        }
        public bool UpdatePasswordMerchant(int userId, string password)
        {
            try
            {
                var passwordSalt = DateTime.Now.Year + "_VBuy.in";
                var updatedPassword = GeneratePassword(password, passwordSalt);

                var user = _context.User.Where<User>(x => x.UserId == userId).FirstOrDefault<User>();
                if (user != null)
                {
                    user.Password = updatedPassword;
                    user.PasswordSalt = passwordSalt;
                    user.UpdatedOnUtc = DateTime.UtcNow;
                    _context.User.Update(user);
                    _context.SaveChanges();
                    return true;
                }
            }
            catch
            {

            }
            return false;
        }
        public void PasswordResetCompeleted(string username, string uniqueId)
        {
            try
            {
                var updateFlagQuery = _context.PasswordReset.Where(x => x.Username == username && x.PasswordResetToken == uniqueId).FirstOrDefault();
                if (updateFlagQuery != null)
                {
                    updateFlagQuery.FlagCompleted = true;
                    _context.PasswordReset.Update(updateFlagQuery);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public int GenerateResetPasswordLinkQuery(string username, out string passwordResetToken)
        {
            try
            {
                int affectedRows = 0;
                passwordResetToken = Convert.ToString(GenerateUniquePasswordResetToken(username));
                DateTime PasswordResetExpiration = DateTime.UtcNow.AddDays(1);

                var userInPasswordReset = _context.PasswordReset.Where(x => x.Username == username).FirstOrDefault();
                if (userInPasswordReset != null)
                {
                    userInPasswordReset.PasswordResetToken = passwordResetToken;
                    userInPasswordReset.PasswordResetExpiration = PasswordResetExpiration;
                    userInPasswordReset.FlagCompleted = false;
                    _context.PasswordReset.Update(userInPasswordReset);
                    affectedRows = _context.SaveChanges();
                }
                else
                {
                    PasswordReset passwordReset = new PasswordReset();
                    passwordReset.Username = username;
                    passwordReset.PasswordResetToken = passwordResetToken;
                    passwordReset.PasswordResetExpiration = PasswordResetExpiration;
                    _context.PasswordReset.Add(passwordReset);
                    affectedRows = _context.SaveChanges();
                }
                return affectedRows;
            }
            catch (Exception ex)
            {
                passwordResetToken = null;
                return 0;
            }

        }
        public bool CheckUserDataInPasswordReset(string username, string uniqueId)
        {
            try
            {
                var userInPasswordReset = _context.PasswordReset.Where(x => x.Username == username && x.PasswordResetToken == uniqueId && x.FlagCompleted == false).Select(x => x.Username).FirstOrDefault();
                return !string.IsNullOrEmpty(userInPasswordReset);
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
