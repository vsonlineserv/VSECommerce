using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Order
{
    public class CartRepository
    {
        private readonly DataContext _context;
        public CartRepository(DataContext dataContext)
        {
            _context = dataContext;
        }
        public bool CheckQuantityExists(int productId)
        {
            try
            {
                if (productId > 0)
                {
                    int Id = 0;
                    bool IsFlagTrackQuantity = false;
                    bool IsAllowOutOfStockSales = false;
                    int availableQuantity = 0;
                    var inventoryDetails = _context.NewInventory.Where(x => x.ProductId == productId).FirstOrDefault();
                    if (inventoryDetails != null)
                    {
                        Id = inventoryDetails.Id;
                        IsFlagTrackQuantity = inventoryDetails.FlagTrackQuantity;
                        IsAllowOutOfStockSales = inventoryDetails.FlagAllowSellOutOfStock;
                        availableQuantity = inventoryDetails.AvailableQuantity;
                    }
                    if (Id > 0 && IsFlagTrackQuantity && !IsAllowOutOfStockSales)
                    {
                        if (availableQuantity == 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public int? getAvailableQuantity(int productId)
        {
            try
            {
                if (productId > 0)
                {
                    int availableQuantity = 0;
                    var productAvailable = _context.NewInventory.Where(x => x.ProductId == productId).FirstOrDefault();
                    if (productAvailable != null)
                    {
                        availableQuantity = productAvailable.AvailableQuantity;
                    }
                    if (availableQuantity > 0)
                    {
                        return availableQuantity;
                    }
                    else
                    {
                        return null;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string GetShoppingCartForUserQuery(int userId)
        {
            var query = @"SELECT 
            [ShoppingCartItem].[Id]
            ,[ShoppingCartItem].[BranchId]
            ,[ShoppingCartItem].[CustomerId]
            ,[ShoppingCartItem].[ProductId]
            ,[ShoppingCartItem].[Quantity]
            ,[ShoppingCartItem].[UnitPrice]
            ,[ShoppingCartItem].[ShippingCharges] AdditionalShippingCharge
            ,SelectedSize
            ,Product.Name
            ,productimage.PictureName
            ,Pricing.SpecialPrice
            ,Pricing.Price
            ,Pricing.DeliveryTime
            ,SellerBranch.BranchId
            ,SellerBranch.BranchName Branch
            FROM [dbo].[ShoppingCartItem] 
            INNER JOIN Product ON [ShoppingCartItem].ProductId = Product.ProductId
            OUTER apply
            (
            select top 1 PictureName from ProductImage where ProductId = Product.ProductId
            )
            productimage
            OUTER apply
            (
            select top 1 SpecialPrice,Price,DeliveryTime,Branch from Pricing where Pricing.Branch = ShoppingCartItem.BranchId AND
            Product = ShoppingCartItem.ProductId 
            And (IsDeleted is null  or IsDeleted= 'false' or IsDeleted= 0)
            {ProductVariant}    
            )
            Pricing
            INNER JOIN SellerBranch ON SellerBranch.BranchId = Pricing.Branch 
            Where CustomerId = {UserId}".FormatWith(new
            {
                UserId = userId,
                ProductVariant = ""
            });
            return query;
        }
        public bool CheckAvailableQuantityInInventory(int productId, int quantity)
        {
            try
            {
                if (productId > 0)
                {
                    int Id = 0;
                    bool IsFlagTrackQuantity = false;
                    bool IsAllowOutOfStockSales = false;
                    int availableQuantity = 0;
                    var productAvailable = _context.NewInventory.Where(x => x.ProductId == productId).FirstOrDefault();
                    if (productAvailable != null)
                    {
                        Id = productAvailable.Id;
                        IsFlagTrackQuantity = productAvailable.FlagTrackQuantity;
                        IsAllowOutOfStockSales = productAvailable.FlagAllowSellOutOfStock;
                        availableQuantity = productAvailable.AvailableQuantity;
                    }
                    if (Id > 0 && IsFlagTrackQuantity && quantity > 0 && !IsAllowOutOfStockSales)
                    {
                        if (availableQuantity < quantity)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public string ReduceQuantityInInventory(int productId, int quantity)
        {
            try
            {
                if (productId > 0)
                {
                    int Id = 0;
                    bool IsFlagTrackQuantity = false;
                    int availableQuantity = 0;
                    var productDetails = _context.NewInventory.Where(x => x.ProductId == productId).FirstOrDefault();
                    if (productDetails != null)
                    {
                        Id = productDetails.Id;
                        IsFlagTrackQuantity = productDetails.FlagTrackQuantity;
                        availableQuantity = productDetails.AvailableQuantity;
                    }
                    if (Id > 0 && IsFlagTrackQuantity && quantity > 0)
                    {
                        var inventoryUpdate = _context.NewInventory.Where(x => x.ProductId == productId).FirstOrDefault();
                        if (inventoryUpdate != null)
                        {
                            int newAvailableQuantity = availableQuantity - quantity;
                            inventoryUpdate.AvailableQuantity = newAvailableQuantity;
                            inventoryUpdate.UpdatedDate = DateTime.UtcNow;
                            _context.NewInventory.Update(inventoryUpdate);
                            _context.SaveChanges();
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        public string AddQuantityInInventory(int productId, int quantity)
        {
            try
            {
                if (productId > 0)
                {
                    int Id = 0;
                    bool IsFlagTrackQuantity = false;
                    int availableQuantity = 0;

                    var inventoryDetails = _context.NewInventory.Where(x => x.ProductId == productId).FirstOrDefault();
                    if (inventoryDetails != null)
                    {
                        Id = inventoryDetails.Id;
                        IsFlagTrackQuantity = inventoryDetails.FlagTrackQuantity;
                        availableQuantity = inventoryDetails.AvailableQuantity;
                    }
                    if (Id > 0 && IsFlagTrackQuantity && quantity > 0)
                    {
                        int newAvailableQuantity = availableQuantity + quantity;
                        if (inventoryDetails != null)
                        {
                            inventoryDetails.AvailableQuantity = newAvailableQuantity;
                            inventoryDetails.UpdatedDate = DateTime.UtcNow;
                            _context.NewInventory.Update(inventoryDetails);
                            _context.SaveChanges();
                        }
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
