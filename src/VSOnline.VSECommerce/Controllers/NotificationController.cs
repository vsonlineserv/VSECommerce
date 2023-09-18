using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nest;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Notifications;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Permission;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly EfContext _efContext;
        private readonly DataContext _context;
        private readonly NotificationServices _notificationServices;

        public NotificationController(EfContext efContext, DataContext context, NotificationServices notificationServices)
        {
            _efContext = efContext;
            _context = context;
            _notificationServices = notificationServices;
        }

        [HttpPost("Seller/{BranchId}/AddNotification")]
        public IActionResult AddNotification(int BranchId, NotificationDTO notificationDTO)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (!string.IsNullOrEmpty(notificationDTO.AuthId) && !string.IsNullOrEmpty(notificationDTO.DeviceToken))
                {
                    var deviceExist = _context.PushNotification.Where(x => x.AuthId == notificationDTO.AuthId && x.BranchId == BranchId && x.DeviceToken == notificationDTO.DeviceToken).FirstOrDefault();
                    if (deviceExist != null)
                    {
                        return Ok(deviceExist);
                    }
                    else
                    {
                        PushNotification pushNotification = new PushNotification();
                        pushNotification.AuthId = notificationDTO.AuthId;
                        pushNotification.DeviceToken = notificationDTO.DeviceToken;
                        pushNotification.BranchId = BranchId;
                        pushNotification.FlagNotification = true;
                        pushNotification.CreatedOnUtc = DateTime.UtcNow;
                        _context.PushNotification.Add(pushNotification);
                        _context.SaveChanges();
                        var notificationDetails = _context.PushNotification.Where(x => x.Id == pushNotification.Id).FirstOrDefault();
                        return Ok(notificationDetails);
                    }
                }
                return BadRequest("Device Token and Auth Id required");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("Seller/{BranchId}/EnableOrDisableNotification/{Status}/{DeviceToken}")]
        public IActionResult EnableOrDisableNotification(int BranchId, bool Status, string DeviceToken)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var deviceDetails = _context.PushNotification.Where(x => x.BranchId == BranchId && x.DeviceToken == DeviceToken).FirstOrDefault();
                if (deviceDetails != null)
                {
                    deviceDetails.FlagNotification = Status;
                    deviceDetails.UpdatedOnUtc = DateTime.UtcNow;
                    _context.PushNotification.Update(deviceDetails);
                    _context.SaveChanges();
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("Seller/{BranchId}/GetNotificationsSettings/{DeviceToken}")]
        public IActionResult GetNotificationsSettings(int BranchId, string DeviceToken)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var deviceDetails = _context.PushNotification.Where(x => x.BranchId == BranchId && x.DeviceToken == DeviceToken).FirstOrDefault();
                if (deviceDetails != null)
                {
                    return Ok(deviceDetails);
                }
                return BadRequest("Details not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [HttpGet("Seller/{BranchId}/SendNotifiacation")]
        public async Task<IActionResult> SendNotifiacation(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                string title = "Regarding leave";
                string body = "Time to leave";
                string deviceToken = "cQzfeH86hkVtjy0mNm2Liu:APA91bFBDEW-PPZPXmk3k3Xpgdy2dvayDb8uJGsI1W3-zhJh9w5Bf_ngjwT6xyrWLiY64WUxY5zUJEqdDGoi9xaoUdPCzQZIcM4uiBSPGitt81EegzIdFcV8gZ_9Ar4BmkEljHK7yP8n";
                await _notificationServices.SendTotificationBYToKen(title, body, deviceToken);
                //var deviceDetails = _context.PushNotification.Where(x => x.BranchId == BranchId).ToList();
                //if (deviceDetails.Count > 0)
                //{
                //    foreach(var device in deviceDetails)
                //    {
                //        string title = "Regarding leave";
                //        string body = "Time to leave";
                //        _notificationServices.SendTotificationBYToKen(title, body, 'crK2MKs_EEjLgo7OINZtLv:APA91bGQ8VI2MQvBzd3KqdtKkO901rwBQeaoEVU8T1TDvxZv5y95IbsorElNqBNlH0Aw__3Y4lDxlV0Z5krlDf2dlDtlW10DdsUwj0E8X2MvR7eB8AyjgKMzeoMRcwBXbFQtwHvHrhip');
                //    }

                //    return Ok();
                //}
                return BadRequest("Details not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

    }
}
