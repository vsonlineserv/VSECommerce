using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Permission;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class ShippingController : ControllerBase
    {
        private readonly DataContext _context;
        public ShippingController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("Seller/{BranchId}/GetShippingType")]
        public IActionResult GetShippingType(int BranchId)
        {
            try
            {
                var CurrentSelection = _context.NewMasterSettingsSelections.Where(x => x.BranchId == BranchId).Select(x => x.CurrentSelection).FirstOrDefault();
                if (!string.IsNullOrEmpty(CurrentSelection))
                {
                    return Ok(CurrentSelection.ToString());
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Shipping_Write)]
        [HttpPost("Seller/{BranchId}/AddMasterSettingsSelections")]
        public IActionResult AddMasterSettingsSelections(int BranchId, MasterSettingsSelectionDTO selectionDetails)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (!string.IsNullOrEmpty(selectionDetails.CurrentSelection))
                {
                    var masterSettingDetails = _context.NewMasterSettingsSelections.Where(x => x.MasterSettings == selectionDetails.MasterSettings && x.BranchId == BranchId).FirstOrDefault();
                    if (masterSettingDetails != null)
                    {
                        masterSettingDetails.MasterSettings = selectionDetails.MasterSettings;
                        masterSettingDetails.CurrentSelection = selectionDetails.CurrentSelection;
                        masterSettingDetails.UpdatedDate = DateTime.UtcNow;
                        _context.NewMasterSettingsSelections.Update(masterSettingDetails);
                        _context.SaveChanges();
                    }
                    else
                    {
                        NewMasterSettingsSelections newMasterSettingsSelections = new NewMasterSettingsSelections();
                        newMasterSettingsSelections.MasterSettings = selectionDetails.MasterSettings;
                        newMasterSettingsSelections.CurrentSelection = selectionDetails.CurrentSelection;
                        newMasterSettingsSelections.CreatedDate = DateTime.UtcNow;
                        newMasterSettingsSelections.BranchId = BranchId;
                        _context.NewMasterSettingsSelections.Add(newMasterSettingsSelections);
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

        [HttpGet("Seller/{BranchId}/GetShippingDetails/{type}")]
        public IActionResult GetShippingDetails(int BranchId, string type)
        {
            try
            {
                var shippingDetails = _context.NewMasterShippingCalculation.Where(x => x.Type == type && x.BranchId == BranchId).FirstOrDefault();
                return Ok(shippingDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Shipping_Write)]
        [HttpPost("Seller/{BranchId}/AddShippingDetails")]
        public IActionResult AddShippingDetails(int BranchId, ShippingCalculationDTO shippingCalculation)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (!string.IsNullOrEmpty(shippingCalculation.Type))
                {
                    var masterShippingDetails = _context.NewMasterShippingCalculation.Where(x => x.Type == shippingCalculation.Type && x.BranchId == BranchId).FirstOrDefault();
                    if (masterShippingDetails != null)
                    {
                        masterShippingDetails.Type = shippingCalculation.Type;
                        masterShippingDetails.DisplayName = shippingCalculation.DisplayName;
                        masterShippingDetails.DeliveryTime = shippingCalculation.DeliveryTime;
                        masterShippingDetails.RangeStart = shippingCalculation.RangeStart;
                        masterShippingDetails.RangeEnd = shippingCalculation.RangeEnd;
                        masterShippingDetails.CreatedBy = shippingCalculation.CreatedBy;
                        masterShippingDetails.UpdatedDate = DateTime.UtcNow;
                        _context.NewMasterShippingCalculation.Update(masterShippingDetails);
                        _context.SaveChanges();
                    }
                    else
                    {
                        NewMasterShippingCalculation newMasterShippingCalculation = new NewMasterShippingCalculation();
                        newMasterShippingCalculation.Type = shippingCalculation.Type;
                        newMasterShippingCalculation.DisplayName = shippingCalculation.DisplayName;
                        newMasterShippingCalculation.DeliveryTime = shippingCalculation.DeliveryTime;
                        newMasterShippingCalculation.Rate = shippingCalculation.Rate;
                        newMasterShippingCalculation.RangeStart = shippingCalculation.RangeStart;
                        newMasterShippingCalculation.RangeEnd = shippingCalculation.RangeEnd;
                        newMasterShippingCalculation.CreatedBy = shippingCalculation.CreatedBy;
                        newMasterShippingCalculation.CreatedDate = DateTime.UtcNow;
                        newMasterShippingCalculation.IsDeleted = false;
                        newMasterShippingCalculation.BranchId = BranchId;
                        _context.NewMasterShippingCalculation.Add(newMasterShippingCalculation);
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

        //delivery
        [Authorize(Policy = PolicyTypes.Shipping_Read)]
        [HttpGet("Seller/{BranchId}/GetCarrierDetails")]
        public IActionResult GetCarrierDetails(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var carrierDetailsList = _context.NewMasterParcelService.Where(x => x.BranchId == BranchId).Select(x => x.CarrierName).ToList();
                return Ok(carrierDetailsList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Shipping_Write)]
        [HttpPost("Seller/{BranchId}/AddCarrier")]
        public IActionResult AddCarrier(int BranchId, AddCarrierDTO carrierDetails)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (!string.IsNullOrEmpty(carrierDetails.CarrierName))
                {
                    var parcelServiceDetails = _context.NewMasterParcelService.Where(x => x.CarrierName == carrierDetails.CarrierName && x.BranchId == BranchId).FirstOrDefault();
                    if (parcelServiceDetails != null)
                    {
                        return BadRequest("CarrierName Already Exists");
                    }
                    else
                    {
                        NewMasterParcelService newMasterParcelService = new NewMasterParcelService();
                        newMasterParcelService.CarrierName = carrierDetails.CarrierName;
                        newMasterParcelService.CreatedBy = carrierDetails.CreatedBy;
                        newMasterParcelService.CreatedDate = DateTime.UtcNow;
                        newMasterParcelService.IsDeleted = false;
                        newMasterParcelService.BranchId = BranchId;
                        _context.NewMasterParcelService.Add(newMasterParcelService);
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
    }
}
