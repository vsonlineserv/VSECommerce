using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Permission;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class InventoryController : VSControllerBase
    {
        private readonly DataContext _context;
        public InventoryController(DataContext context, IOptions<AppSettings> _appSettings) : base(_appSettings)
        {
            _context = context;
        }

        [Authorize(Policy = PolicyTypes.Inventory_Read)]
        [HttpGet("Seller/{BranchId}/GetInventoryDetails/{productId}")]
        public IActionResult GetInventoryDetails(int BranchId, int productId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                InventoryResult inventoryResult = new InventoryResult();
                var InventoryDetails = _context.NewInventory.Where(x => x.ProductId == productId && x.BranchId == BranchId).FirstOrDefault();
                if (InventoryDetails != null)
                {
                    inventoryResult.SKU = InventoryDetails.SKU;
                    inventoryResult.ProductCode = InventoryDetails.BarCode;
                    inventoryResult.AvailableQuantity = InventoryDetails.AvailableQuantity;
                    inventoryResult.IsTrackQuantity = InventoryDetails.FlagTrackQuantity;
                    inventoryResult.IsOutOfStock = InventoryDetails.FlagAllowSellOutOfStock;
                }
                return Ok(inventoryResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Inventory_Write)]
        [HttpPost("Seller/{BranchId}/UpdateQuantity")]
        public IActionResult UpdateQuantity(int BranchId, InventoryDTO inventoryDTO)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var inventory = _context.NewInventory.Where(x => x.ProductId == inventoryDTO.ProductId).ToList();
                if (inventory.Count == 0)
                {
                    NewInventory newInventory = new NewInventory();
                    newInventory.ProductId = inventoryDTO.ProductId;
                    newInventory.SellerBranchId = (int)(inventoryDTO.BranchId != null ? inventoryDTO.BranchId : 0);
                    newInventory.BranchId = (int)(inventoryDTO.BranchId != null ? inventoryDTO.BranchId : 0);
                    newInventory.SKU = inventoryDTO.SKU;
                    newInventory.BarCode = inventoryDTO.ProductCode;
                    newInventory.AvailableQuantity = inventoryDTO.AvailableQuantity;
                    newInventory.FlagTrackQuantity = inventoryDTO.IsTrackQuantity;
                    newInventory.FlagAllowSellOutOfStock = inventoryDTO.IsOutOfStock;
                    newInventory.CreatedDate = DateTime.UtcNow;
                    _context.NewInventory.Add(newInventory);
                    _context.SaveChanges();
                }
                else
                {
                    var inventoryDetails = _context.NewInventory.Where(x => x.ProductId == inventoryDTO.ProductId).FirstOrDefault();
                    if (inventoryDetails != null)
                    {
                        inventoryDetails.SKU = inventoryDTO.SKU;
                        inventoryDetails.BarCode = inventoryDTO.ProductCode;
                        inventoryDetails.AvailableQuantity = inventoryDTO.AvailableQuantity;
                        inventoryDetails.FlagTrackQuantity = inventoryDTO.IsTrackQuantity;
                        inventoryDetails.FlagAllowSellOutOfStock = inventoryDTO.IsOutOfStock;
                        inventoryDetails.UpdatedDate = DateTime.UtcNow;
                        _context.Update(inventoryDetails);
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

        [Authorize(Policy = PolicyTypes.Inventory_Read)]
        [HttpGet("Seller/{BranchId}/GetAllInventoryproduct")]
        public IActionResult GetAllInventoryproduct(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var result = _context.NewInventory.Join(_context.ProductStoreMapping,
                      inventory => new { inventory.ProductId, inventory.BranchId },
                       product => new { product.ProductId, product.BranchId },
                       (inventory, product) => new { inventory, product }).Where(x => x.inventory.BranchId == BranchId).Select(y => new InventoryResult
                       {
                           SKU = y.inventory.SKU,
                           ProductId = y.product.ProductId,
                           BranchId = y.inventory.SellerBranchId,
                           AvailableQuantity = y.inventory.AvailableQuantity,
                           IsOutOfStock = y.inventory.FlagAllowSellOutOfStock,
                           IsTrackQuantity = y.inventory.FlagTrackQuantity,
                           ProductName = y.product.Name,
                           PictureName = _context.ProductImage.Where(x => x.ProductId == y.product.ProductId).FirstOrDefault().PictureName,
                       }).ToList();
                foreach (var eachData in result)
                {
                    if (eachData.PictureName != null)
                    {
                        if (!eachData.PictureName.Contains("http"))
                        {
                            eachData.PictureName = _appSettings.ImageUrlBase + eachData.PictureName;
                        }
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Inventory_Edit)]
        [HttpPut("Seller/{BranchId}/AddToExistingQuantity")]
        public IActionResult AddToExistingQuantity(int BranchId, InventoryDTO inventoryDTO)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var inventory = _context.NewInventory.Where(x => x.ProductId == inventoryDTO.ProductId).FirstOrDefault();
                if (inventory != null)
                {
                    int newAvailableQuantity = Convert.ToInt32(inventory.AvailableQuantity) + inventoryDTO.AvailableQuantity;
                    var inventoryDetails = _context.NewInventory.Where(x => x.ProductId == inventoryDTO.ProductId).FirstOrDefault();
                    if (inventoryDetails != null)
                    {
                        inventoryDetails.AvailableQuantity = newAvailableQuantity;
                        inventoryDetails.UpdatedDate = DateTime.UtcNow;
                        _context.Update(inventoryDetails);
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
