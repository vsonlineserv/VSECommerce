using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.DTO;
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
        public AccountController(DataContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
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
                        if (_userService.AddSellerStaffMapping(userModel.StoreId, userModel.BranchId, user.UserId, userModel.StoreReferenceId) == true)
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

        [HttpDelete("Seller/{BranchId}/DeleteStaffAccount/{StaffIdentifier}")]
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
    }
}
