using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Linq;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Permission;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class DiscountController : VSControllerBase
    {
        private readonly DataContext _context;
        private readonly EfContext _efContext;
        private readonly OrderHelper _orderHelper;
        public DiscountController(DataContext context, EfContext efContext, IOptions<AppSettings> _appSettings, OrderHelper orderHelper) : base(_appSettings)
        {
            _context = context;
            _efContext = efContext;
            _orderHelper = orderHelper;
        }

        [Authorize(Policy = PolicyTypes.Discount_Read)]
        [HttpGet("Seller/{BranchId}/Discount/{id}")]
        public IActionResult GetDiscountDetailsById(int BranchId, int id)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var discountDetail = _context.Discount.Where(e => e.Id == id).FirstOrDefault();
                return Ok(discountDetail);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Discount_Delete)]
        [HttpDelete("Seller/{BranchId}/Discount/{discountId}")]
        public IActionResult DeleteCoupon(int BranchId, int discountId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var discountDetail = _context.Discount.Where(x => x.Id == discountId).FirstOrDefault();
                if (discountDetail != null)
                {
                    discountDetail.IsDeleted = true;
                    discountDetail.UpdatedDateUtc = DateTime.UtcNow;
                    _context.Discount.Update(discountDetail);
                    _context.SaveChanges();
                    return Ok("Success");
                }
                return BadRequest("Discount Detail Not Found");
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Discount_Edit)]
        [HttpPut("Seller/{BranchId}/UpdateDiscountDetails")]
        public IActionResult UpdateDiscountDetails(int BranchId, DiscountCouponDetailsDTO discountCouponDetails)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var couponCode = _context.Discount.Where(x => x.CouponCode == discountCouponDetails.CouponCode && x.Id != discountCouponDetails.Id).FirstOrDefault();
                if (couponCode == null)
                {
                    var discountDetail = _context.Discount.Where(x => x.Id == discountCouponDetails.Id).FirstOrDefault();
                    if (discountDetail != null)
                    {
                        discountDetail.Name = discountCouponDetails.Name;
                        discountDetail.DiscountTypeId = (int)Utilities.Enums.DiscountType.OrderDiscount;
                        discountDetail.UsePercentage = (bool)(discountCouponDetails.UsePercentage != null ? discountCouponDetails.UsePercentage : false);
                        discountDetail.DiscountPercentage = discountCouponDetails.DiscountPercentage != null ? discountCouponDetails.DiscountPercentage : 0;
                        discountDetail.DiscountAmount = discountCouponDetails.DiscountAmount != null ? discountCouponDetails.DiscountAmount : 0;
                        discountDetail.StartDateUtc = DateTime.ParseExact(discountCouponDetails.StartDateUtc, "MM/dd/yyyy", null);
                        discountDetail.EndDateUtc = DateTime.ParseExact(discountCouponDetails.EndDateUtc, "MM/dd/yyyy", null);
                        discountDetail.RequiresCouponCode = discountCouponDetails.RequiresCouponCode;
                        discountDetail.CouponCode = discountCouponDetails.CouponCode;
                        discountDetail.MinOrderValue = discountCouponDetails.MinOrderValue;
                        discountDetail.MaxDiscountAmount = discountCouponDetails.MaxDiscountAmount != null ? discountCouponDetails.MaxDiscountAmount : 0;
                        discountDetail.UpdatedDateUtc = DateTime.UtcNow;
                        _context.Discount.Update(discountDetail);
                        _context.SaveChanges();
                        return Ok("Success");
                    }
                }
                return BadRequest("Already Exists");
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Discount_Read)]
        [HttpGet("Seller/{BranchId}/Discount")]
        public IActionResult GetAllDiscountDetails(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var discountDetails = _context.Discount.Where(x => x.BranchId == BranchId).OrderByDescending(x => x.Id).ToList();
                return Ok(discountDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Discount_Write)]
        [HttpPost("Seller/{BranchId}/AddDiscount")]
        public IActionResult AddDiscountCoupon(int BranchId, DiscountCouponDetailsDTO discountCouponDetails)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                DateTime startDate = DateTime.ParseExact(discountCouponDetails.StartDateUtc, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                DateTime EndDate = DateTime.ParseExact(discountCouponDetails.EndDateUtc, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                var couponCode = _context.Discount.Where(x => x.CouponCode == discountCouponDetails.CouponCode).FirstOrDefault();
                if (couponCode == null)
                {
                    Discount discount = new Discount();
                    discount.Name = discountCouponDetails.Name;
                    discount.DiscountTypeId = (int)Utilities.Enums.DiscountType.OrderDiscount;
                    discount.UsePercentage = (bool)discountCouponDetails.UsePercentage;
                    discount.DiscountPercentage = discountCouponDetails.DiscountPercentage != null ? discountCouponDetails.DiscountPercentage : 0;
                    discount.DiscountAmount = discountCouponDetails.DiscountAmount != null ? discountCouponDetails.DiscountAmount : 0;
                    discount.StartDateUtc = startDate;
                    discount.EndDateUtc = EndDate;
                    discount.RequiresCouponCode = discountCouponDetails.RequiresCouponCode;
                    discount.CouponCode = discountCouponDetails.CouponCode;
                    discount.MinOrderValue = discountCouponDetails.MinOrderValue;
                    discount.MaxDiscountAmount = discountCouponDetails.MaxDiscountAmount != null ? discountCouponDetails.MaxDiscountAmount : 0;
                    discount.CreatedDateUtc = DateTime.UtcNow;
                    discount.BranchId = BranchId;
                    discount.IsDeleted = false;
                    _context.Discount.Add(discount);
                    _context.SaveChanges();
                    return Ok("Success");
                }
                return BadRequest("Already Exists");
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Discount_Read)]
        [HttpPost("Seller/{BranchId}/GetDiscountsByFilter")]
        public IActionResult GetDiscountsByFilter(int BranchId, DiscountFilterDTO discountFilterDTO)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var query = _orderHelper.GetDisountsByFilter_(discountFilterDTO.days, discountFilterDTO.month, discountFilterDTO.startTime,
                    discountFilterDTO.endTime, discountFilterDTO.searchString, discountFilterDTO.activeCoupons, BranchId);
                var result = _efContext.Database.SqlQuery<DiscountResult>(query).ToList();
                return Ok(result);
            }
            catch (Exception Ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
    }
}
