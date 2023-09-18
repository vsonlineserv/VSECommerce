using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Permission;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class EnquiriesController : VSControllerBase
    {
        private readonly SellerContactHelper _sellerContactHelper;
        private readonly DataContext _context;
        private readonly MailHelper _mailHelper;
        private readonly MessageHelper _messageHelper;
        private readonly EfContext _efContext;

        public EnquiriesController(SellerContactHelper sellerContactHelper, IOptions<AppSettings> _appSettings, DataContext context, MailHelper mailHelper, MessageHelper messageHelper, EfContext efContext) : base(_appSettings)
        {
            _sellerContactHelper = sellerContactHelper;
            _context = context;
            _mailHelper = mailHelper;
            _messageHelper = messageHelper;
            _efContext = efContext;
        }

        [Authorize(Policy = PolicyTypes.Enquiries_Read)]
        [HttpGet("Seller/{BranchId}/GetSellerInbox")]
        public IActionResult GetSellerInbox(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var result = _sellerContactHelper.GetSellerInbox(BranchId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Enquiries_Read)]
        [HttpGet("Seller/{BranchId}/GetBranchEnquirySummary")]
        public IActionResult GetBranchEnquirySummary(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var query = _sellerContactHelper.GetBranchEnquirySummaryQuery(BranchId);
                var result = _efContext.Database.SqlQuery<EnquirySummaryDashboardResult>(query).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Enquiries_Read)]
        [HttpGet("Seller/{BranchId}/GetSellerInboxByFilter")]
        public IActionResult GetSellerInboxByFilter(int BranchId, int? days, int? month, string? startTime, string? endTime, bool notReplied, string? searchString)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var query = _sellerContactHelper.GetSellerInboxByFilter(BranchId, days, month, startTime, endTime, notReplied, searchString);
                var result = _efContext.Database.SqlQuery<SellerContactResult>(query).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Enquiries_Write)]
        [HttpGet("Seller/{BranchId}/InboxReply/{mailId}")]
        public IActionResult InboxReply(int BranchId, int mailId, string reply, string domainURL)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                _sellerContactHelper.UpdateReply(mailId, reply);

                var contactResult = _sellerContactHelper.GetContactInformation(mailId);
                if (contactResult != null)
                {
                    var productDetails = _context.ProductStoreMapping.Where(x => x.ProductId == contactResult.ProductId).FirstOrDefault();
                    var branchinfo = _context.SellerBranch.Where(x => x.BranchId == contactResult.BranchId).FirstOrDefault();

                    if (!string.IsNullOrEmpty(contactResult.Email))
                    {
                        _mailHelper.SendProductReplyMail(contactResult.Email, productDetails.Name, branchinfo.BranchName, reply, domainURL);
                    }
                    if (!string.IsNullOrEmpty(contactResult.Mobile))
                    {
                        _messageHelper.SendProductReplyMessage(contactResult.Mobile, productDetails.Name, branchinfo.BranchName, reply);
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
    }
}
