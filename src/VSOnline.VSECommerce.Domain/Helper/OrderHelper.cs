using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Helper
{
    public class OrderHelper
    {
        public string GetEnumDescription(Enum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());

                DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attributes != null && attributes.Length > 0)
                    return attributes[0].Description;
                else
                    return value.ToString();
            }
            catch
            {

            }
            return string.Empty;
        }
        public string getOrderDetails(int branchId, int PageSize, int PageNo)
        {
            var query = @"
                           WITH CTE AS
                         (
                          select   OrderProduct.Id OrderId , count (OrderProductItem.OrderId) as OrderCount,OrderProduct.BranchOrderId,
						  OrderProduct.OrderDateUtc , OrderProduct.OrderStatusId
						  ,OrderProduct.PaymentStatusId
                            ,OrderProduct.PaymentMethod
						  ,[User].FirstName, [User].Email,
						  BuyerAddress.Address1, BuyerAddress.Address2, BuyerAddress.[State], 
						  BuyerAddress.City, BuyerAddress.PostalCode, BuyerAddress.PhoneNumber
						  ,SellerBranch.BranchName,OrderTotal
						  From OrderProductItem
						  Inner Join OrderProduct ON OrderProduct.Id = OrderProductItem.OrderId
						   AND OrderProductItem.BranchId={branchId}
						  Inner Join SellerBranch ON SellerBranch.BranchId = OrderProductItem.BranchId
						  Inner Join [User] ON [User].UserId = OrderProduct.CustomerId
						  INNER JOIN BuyerAddress ON OrderProduct.ShippingAddressId = BuyerAddress.BuyerAddressId
						  group by OrderProduct.Id, OrderProduct.OrderDateUtc , OrderProduct.OrderStatusId
						  ,OrderProduct.PaymentStatusId
                            ,OrderProduct.PaymentMethod,[User].FirstName, [User].Email,
						  BuyerAddress.Address1, BuyerAddress.Address2, BuyerAddress.[State], 
						  BuyerAddress.City, BuyerAddress.PostalCode, BuyerAddress.PhoneNumber
						  ,SellerBranch.BranchName,OrderTotal,BranchOrderId
						   
                        )
                             SELECT * FROM (
                             SELECT ROW_NUMBER() OVER(ORDER BY OrderId desc) AS orders,
                                    OrderId, OrderCount,BranchOrderId, OrderDateUtc,
                                FirstName,Email,BranchName,OrderTotal,
                                Address1,PhoneNumber,
                                PaymentStatusId,PaymentMethod, OrderStatusId
                                FROM CTE
                               ) AS TBL
                WHERE orders BETWEEN (({PageNo} - 1) * {PageSize} + 1) AND ({PageNo} * {PageSize})
                        ".FormatWith(new { branchId, PageNo, PageSize });
            return query;
        }
        public string getOrderDetailsbyFiltersForCsv(int branchId, int Status, string searchString, int? days, string startTime, string endTime)
        {
            var query = @"
                          WITH CTE AS
                         (
                          select OrderProductItem.Id Id, OrderProduct.Id OrderId, Product.ProductId,
						  OrderProduct.OrderDateUtc , OrderProductItem.OrderItemStatus, OrderProduct.OrderStatusId
						  ,OrderProduct.PaymentStatusId
                            ,OrderProduct.PaymentMethod
						  ,Product.Name,
						  OrderProductItem.UnitPriceInclTax,OrderProductItem.PriceInclTax,
						  OrderProductItem.Quantity, OrderProductItem.SelectedSize
						  ,OrderProductItem.ShippingCharges
						  ,[User].FirstName, 
						  BuyerAddress.Address1, BuyerAddress.Address2, BuyerAddress.[State], 
						  BuyerAddress.City, BuyerAddress.PostalCode, BuyerAddress.PhoneNumber
						  ,SellerBranch.BranchName
						           
						  From OrderProductItem
						  Inner Join OrderProduct ON OrderProduct.Id = OrderProductItem.OrderId
						  INNER Join Product ON Product.ProductId = OrderProductItem.ProductId
						  Inner Join SellerBranch ON SellerBranch.BranchId = OrderProductItem.BranchId
						  Inner Join [User] ON [User].UserId = OrderProduct.CustomerId
						  INNER JOIN BuyerAddress ON OrderProduct.ShippingAddressId = BuyerAddress.BuyerAddressId
                          AND OrderProductItem.BranchId={branchId} 
                            {OrderIDDetails}
                            {filterBydays}
                            {StatusDetails}
                            {filterByCustomDays}
						   
                        )
                             SELECT * FROM (
                             SELECT ROW_NUMBER() OVER(ORDER BY OrderId desc) AS orders,
                                   Id, OrderId, OrderDateUtc,Name, ProductId,
                                Quantity,FirstName,BranchName,
                                UnitPriceInclTax,PriceInclTax,
                                Address1,PhoneNumber,
                                PaymentStatusId,PaymentMethod, SelectedSize
                                ,OrderItemStatus AS OrderItemStatusId   FROM CTE
                               ) AS TBL".FormatWith(new
            {
                branchId,
                OrderIDDetails = (!string.IsNullOrEmpty(searchString)) ? "AND ( [User].PhoneNumber1 like '%{searchString}%' OR OrderId like '%{searchString}%' OR FirstName like '%{searchString}%' OR [User].Email like '%{searchString}%')".FormatWith(new { searchString }) : "",
                StatusDetails = Status > 0 ? "AND OrderProductItem.OrderItemStatus = {Status}".FormatWith(new { Status }) : "",
                filterBydays = (days != null) ? "AND OrderProduct.OrderDateUtc > convert(varchar(8), dateadd(day, -" + days + ", getutcdate()), 112)" : "",
                filterByCustomDays = (startTime != null && endTime != null && startTime != "" && endTime != "") ? "AND OrderProduct.OrderDateUtc between " + "'" + startTime + "'" + "and" + "'" + endTime + "'" : ""
            });
            return query;
        }
        public string getSearchOrdersWithExtraParams(int branchId, string searchString, int? Status, int? days, string startTime, string endTime)
        {
            var query = "";

            query = @"WITH CTE AS
                         (
                          select   OrderProduct.Id OrderId , count (OrderProductItem.OrderId) as OrderCount,OrderProduct.BranchOrderId,
						  OrderProduct.OrderDateUtc , OrderProduct.OrderStatusId
						  ,OrderProduct.PaymentStatusId
                            ,OrderProduct.PaymentMethod
						  ,[User].FirstName, [User].Email,
						  BuyerAddress.Address1, BuyerAddress.Address2, BuyerAddress.[State], 
						  BuyerAddress.City, BuyerAddress.PostalCode, BuyerAddress.PhoneNumber
						  ,SellerBranch.BranchName,OrderTotal
						  From OrderProductItem
						  Inner Join OrderProduct ON OrderProduct.Id = OrderProductItem.OrderId
						  Inner Join SellerBranch ON SellerBranch.BranchId = OrderProductItem.BranchId
						  Inner Join [User] ON [User].UserId = OrderProduct.CustomerId
						  INNER JOIN BuyerAddress ON OrderProduct.ShippingAddressId = BuyerAddress.BuyerAddressId
                          AND SellerBranch.BranchId= {branchId} {SearchString} {StatusDetails} {filterBydays} {filterByCustomDays}
						  group by OrderProduct.Id, OrderProduct.OrderDateUtc , OrderProduct.OrderStatusId
						  ,OrderProduct.PaymentStatusId
                            ,OrderProduct.PaymentMethod,[User].FirstName, [User].Email,
						  BuyerAddress.Address1, BuyerAddress.Address2, BuyerAddress.[State], 
						  BuyerAddress.City, BuyerAddress.PostalCode, BuyerAddress.PhoneNumber
						  ,SellerBranch.BranchName,OrderTotal,BranchOrderId
						 
                        )
                             SELECT * FROM (
                             SELECT ROW_NUMBER() OVER(ORDER BY OrderId desc) AS orders,BranchOrderId,
                                    OrderId, OrderDateUtc,OrderCount,
                                FirstName,Email,BranchName,OrderTotal,
                                Address1,PhoneNumber,
                                PaymentStatusId,PaymentMethod, OrderStatusId
                                FROM CTE
                               ) AS TBL".FormatWith(new
            {
                branchId,
                SearchString = (!string.IsNullOrEmpty(searchString)) ? "AND ([User].PhoneNumber1 like '%{searchString}%' OR OrderId like '%{searchString}%' OR FirstName like '%{searchString}%' OR [User].Email like '%{searchString}%' OR BranchOrderId like '%{searchString}%')".FormatWith(new { searchString }) : "",
                StatusDetails = Status > 0 ? "AND OrderProductItem.OrderItemStatus = {Status}".FormatWith(new { Status }) : "",
                filterBydays = (days != null) ? "AND OrderProduct.OrderDateUtc > convert(varchar(8), dateadd(day, -" + days + ", getutcdate()), 112)" : "",
                filterByCustomDays = (startTime != null && endTime != null && startTime != "" && endTime != "") ? "AND OrderProduct.OrderDateUtc between " + "'" + startTime + "'" + "and" + "'" + endTime + "'" : "",
            });

            return query;
        }
        public string getEachOrderDetails(int branchId, int orderId)
        {
            var query = @"
                        WITH CTE AS
                         (
                          select OrderProductItem.Id Id, OrderProduct.Id OrderId,OrderProduct.OrderTaxTotal OrderTaxTotal,OrderProduct.BranchOrderId,
                            OrderProduct.FlagConfirmStatus FlagConfirmStatus,OrderProduct.OrderShippingTotal OrderShippingTotal,
                            OrderProduct.OrderDiscount OrderDiscount,
                            Product.ProductId,
						  OrderProduct.OrderDateUtc , OrderProductItem.OrderItemStatus, OrderProduct.OrderStatusId
						  ,OrderProduct.PaymentStatusId
                            ,OrderProduct.PaymentMethod
						  ,Product.Name,
						  OrderProductItem.UnitPriceInclTax,OrderProductItem.PriceInclTax,
						  OrderProductItem.Quantity, OrderProductItem.SelectedSize
						  ,OrderProductItem.ShippingCharges
						  ,[User].FirstName,[User].LastName, [User].Email,
						  BuyerAddress.Address1, BuyerAddress.Address2, BuyerAddress.State, BuyerAddress.Country,
						  BuyerAddress.City, BuyerAddress.PostalCode, BuyerAddress.PhoneNumber
						  ,SellerBranch.BranchName, productimage.PictureName, OrderProduct.OrderTotal
						  From OrderProductItem
						  Inner Join OrderProduct ON OrderProduct.Id = OrderProductItem.OrderId
						   AND OrderProductItem.BranchId={branchId} and OrderId={orderId}
						  INNER Join Product ON Product.ProductId = OrderProductItem.ProductId
						  Inner Join SellerBranch ON SellerBranch.BranchId = OrderProductItem.BranchId
						  Inner Join [User] ON [User].UserId = OrderProduct.CustomerId
						  INNER JOIN BuyerAddress ON OrderProduct.ShippingAddressId = BuyerAddress.BuyerAddressId
						  OUTER apply(select top 1 PictureName from ProductImage where ProductId = OrderProductItem.ProductId)productimage)
                             SELECT * FROM (
                             SELECT ROW_NUMBER() OVER(ORDER BY OrderId desc) AS orders,
                                   Id, OrderId, OrderDateUtc,Name, ProductId,BranchOrderId,
                                Quantity,FirstName,LastName, Email, BranchName,
                                UnitPriceInclTax,PriceInclTax,
                                Address1, Address2, State, City, Country, PostalCode, PhoneNumber, PictureName, 
                                PaymentStatusId,PaymentMethod, SelectedSize, OrderTotal
                                ,OrderItemStatus AS OrderItemStatusId, OrderStatusId,
                                OrderTaxTotal,FlagConfirmStatus,OrderShippingTotal,OrderDiscount
                                FROM CTE
                               ) AS TBL
                        ".FormatWith(new
            {
                branchId,
                orderId
            });
            return query;
        }

        public string GetBranchProductSummaryQuery(int branchId)
        {
            string query = @"With cte as (Select CASE WHEN  Product.Published = 1  THEN 'Active'
	                            ELSE 'Inactive'
	                            END ProductStatus,Count(Product.ProductId) TotalProducts from ProductStoreMapping as Product
	                            Where (Product.IsDeleted IS NULL OR Product.IsDeleted =0) and BranchId = {branchId}
	                            GROUP BY Product.IsDeleted,Product.Published
	                            Union
	                            select 'Pending', Count(Product.ProductId) TotalProducts from ProductStoreMapping as Product 
	                            Inner Join NewInventory ON Product.ProductId = NewInventory.ProductId and Product.BranchId = NewInventory.BranchId
	                            Where Product.BranchId = {branchId}
	                            AND (Product.IsDeleted is NULL OR Product.IsDeleted =0) And NewInventory.AvailableQuantity = 0)
	                            Select ProductStatus, Sum(TotalProducts) TotalProducts from cte 
	                            Group By ProductStatus
                             ".FormatWith(new { branchId });
            return query;

        }

        public string GetMostSellingProductsCount()
        {
            string query = @"select TOP 5 Product.Name as ProductName, COUNT(OrderProductItem.ProductId) as OrderCount from OrderProductItem 
                  Inner Join Product ON OrderProductItem.ProductId = Product.ProductId 
                  Group By Product.Name ORDER BY COUNT(OrderProductItem.ProductId) DESC";
            return query;
        }

        public string GetLeastSellingProductsCount()
        {
            string query = @"select TOP 5 Product.Name as ProductName, COUNT(OrderProductItem.ProductId) as OrderCount from OrderProductItem 
                  Inner Join Product ON OrderProductItem.ProductId = Product.ProductId 
                  Group By Product.Name ORDER BY COUNT(OrderProductItem.ProductId)";
            return query;
        }
        public string getTodayAndYesterdaysOrder(int branchId)
        {
            var query = @"
        with OrderCte As
            (
           select  OrderDateUtc, BranchId, OrderTotal SalesValue from OrderProduct
                Where OrderProduct.BranchId  = {branchId}
            ),
            cteOrder AS
            (
             select 1 slNo, 'Today'  as days, sum(SalesValue)as SalesValue from OrderCte where OrderCte.BranchId= {branchId} and
                    (OrderCte.OrderDateUtc > dateadd(day,datediff(day,0,GETDATE()),0))
               union
                select 2 slNo, 'Yesterday'  as days,sum(SalesValue)as SalesValue from OrderCte where OrderCte.BranchId = {branchId} 
                    and (OrderCte.OrderDateUtc >= dateadd(day,datediff(day,1,GETDATE()),0) AND OrderCte.OrderDateUtc < dateadd(day,datediff(day,0,GETDATE()),0))
            )
            select days, SalesValue from cteOrder order by slNo".FormatWith(new { branchId });
            return query;
        }
        //Discount
        public string GetDisountsByFilter(int? days, int? month, string? startTime, string? endTime, string? searchString, bool activeCoupons)
        {
            DateTime currentDate = DateTime.UtcNow;
            string currentFormattedDate = currentDate.ToString("yyyy-MM-dd");
            var query = @"SELECT Id,Name,DiscountTypeId,UsePercentage,DiscountPercentage,DiscountAmount,StartDateUtc,EndDateUtc,RequiresCouponCode,CouponCode,MinOrderValue,MaxDiscountAmount,IsDeleted from Discount
                        WHERE Discount.Id > 0
                        {filterBydays}
                        {filterByMonths}
                        {filterByCustomDays}
                        {searchString}
                        {activeCoupons} 
                        order by Id desc"
            .FormatWith(new
            {
                filterBydays = (days != null) ? "AND Discount.CreatedDateUtc >= convert(varchar(8), dateadd(day, -" + days + ", getutcdate()), 112)" : "",
                filterByMonths = (month != null && month > 0) ? "AND Discount.CreatedDateUtc > GETUTCDATE() -" + month : "",
                filterByCustomDays = (startTime != null && endTime != null && startTime != "" && endTime != "") ? "And Discount.CreatedDateUtc between " + "'" + startTime + "'" + "and" + "'" + endTime + "'" : "",
                searchString = (searchString != null && searchString != "") ? "And Discount.Name like '%" + searchString + "%' OR Discount.CouponCode like '%" + searchString + "%'" : "",
                activeCoupons = (activeCoupons) ? "AND Discount.EndDateUtc <='" + currentFormattedDate + "'" : ""
            });
            return query;
        }
        public string GetDisountsByFilter_(int? days, int? month, string? startTime, string? endTime, string? searchString, bool activeCoupons, int BranchId)
        {
            DateTime currentDate = DateTime.UtcNow;
            string currentFormattedDate = currentDate.ToString("yyyy-MM-dd");
            var query = @"SELECT Id,Name,DiscountTypeId,UsePercentage,DiscountPercentage,DiscountAmount,StartDateUtc,EndDateUtc,RequiresCouponCode,CouponCode,MinOrderValue,MaxDiscountAmount,IsDeleted from Discount
                        WHERE  BranchId = {BranchId}
                        {filterBydays}
                        {filterByMonths}
                        {filterByCustomDays}
                        {searchString}
                        {activeCoupons} 
                        order by Id desc"
            .FormatWith(new
            {
                BranchId,
                filterBydays = (days != null) ? "AND Discount.CreatedDateUtc >= convert(varchar(8), dateadd(day, -" + days + ", getutcdate()), 112)" : "",
                filterByMonths = (month != null && month > 0) ? "AND Discount.CreatedDateUtc > GETUTCDATE() -" + month : "",
                filterByCustomDays = (startTime != null && endTime != null && startTime != "" && endTime != "") ? "And Discount.CreatedDateUtc between " + "'" + startTime + "'" + "and" + "'" + endTime + "'" : "",
                searchString = (searchString != null && searchString != "") ? "And (Discount.Name like '%" + searchString + "%' OR Discount.CouponCode like '%" + searchString + "%')" : "",
                activeCoupons = (activeCoupons) ? " AND (Discount.IsDeleted = 0 or Discount.IsDeleted is null or Discount.IsDeleted = 'false') AND Discount.EndDateUtc >='" + currentFormattedDate + "'" : ""
            });
            return query;
        }
        //Reports
        public string getOrdersSummary(int branchId)
        {
            var query = @"
            with OrderCte As
            (
              select OrderId,OrderDateUtc,quantity,OrderProductItem.BranchId,PriceInclTax SalesValue from OrderProductItem 
                inner join OrderProduct on OrderProductItem.OrderId=OrderProduct.Id
                Where OrderProductItem.BranchId = {branchId}
            ),
            cteOrder AS
            (
             select 1 slNo, 'Today'  as days, sum(quantity)as quantity,sum(SalesValue)as SalesValue from OrderCte where OrderCte.BranchId={branchId} and (OrderCte.OrderDateUtc>convert(varchar(8), dateadd(day,0, getutcdate()), 112))
               union
             select 2 slNo, 'This Week'  as days, sum(quantity)as quantity,sum(SalesValue)as SalesValue from OrderCte where OrderCte.BranchId={branchId} and (OrderCte.OrderDateUtc>convert(varchar(8), dateadd(day,-7, getutcdate()), 112))
               union
                select 3 slNo, 'Fortnight'  as days, sum(quantity)as quantity,sum(SalesValue)as SalesValue from OrderCte where OrderCte.BranchId={branchId} and (OrderCte.OrderDateUtc>convert(varchar(8), dateadd(day,-15, getutcdate()), 112))
	            union
	             select 4 slNo, 'This Month'  as days, sum(quantity)as quantity,sum(SalesValue)as SalesValue from OrderCte where OrderCte.BranchId={branchId} and (OrderCte.OrderDateUtc>convert(varchar(8), dateadd(day,-30, getutcdate()), 112))
	            union
	             select 5 slNo, 'This Quarter'  as days, sum(quantity)as quantity,sum(SalesValue)as SalesValue from OrderCte where OrderCte.BranchId={branchId} and (OrderCte.OrderDateUtc>convert(varchar(8), dateadd(day,-183, getutcdate()), 112))
             union
            select 6 slNo, 'This Year'  as days, sum(quantity)as quantity,sum(SalesValue)as SalesValue from OrderCte where OrderCte.BranchId={branchId} and (OrderCte.OrderDateUtc>convert(varchar(8), dateadd(day,-365, getutcdate()), 112))
            )
            select days,quantity,SalesValue from cteOrder order by slNo".FormatWith(new { branchId });
            return query;
        }
        public string GetOrderSummaryByCategoryQueryByfilter(int branchId, int? days, string? startTime, string? endTime)
        {
            string query = @"select Top 5 Category.Name CategoryName, Sum(quantity) Quantity, Sum(OrderTotal) Total from OrderProductItem 
                              inner join OrderProduct on OrderProductItem.OrderId=OrderProduct.Id
                              Inner Join Product ON OrderProductItem.ProductId = Product.ProductId
                              Inner Join Category ON Product.Category = Category.CategoryId
                              Where OrderProductItem.BranchId = {branchId}
                              {filterBydays}
                              {filterByCustomDays}
                              Group By Category.Name
                              Order By Sum(OrderTotal) desc"
                             .FormatWith(new
                             {
                                 branchId,
                                 filterBydays = (days != null) ? "AND OrderProduct.OrderDateUtc > convert(varchar(8), dateadd(day, -" + days + ", getutcdate()), 112)" : "",
                                 filterByCustomDays = (startTime != null && endTime != null && startTime != "" && endTime != "") ? "And OrderProduct.OrderDateUtc between " + "'" + startTime + "'" + "and" + "'" + endTime + "'" : ""
                             });
            return query;
        }
        public string GetOrderSummaryByProductQueryByfilter(int branchId, int? days, string? startTime, string? endTime)
        {
            string query = @"select Top 5 Product.Name ProductName, Sum(quantity) Quantity, Sum(OrderTotal) Total from OrderProductItem 
                inner join OrderProduct on OrderProductItem.OrderId=OrderProduct.Id
                Inner Join Product ON OrderProductItem.ProductId = Product.ProductId
                Where OrderProductItem.BranchId =  {branchId}
                {filterBydays}
                {filterByCustomDays}
                Group By Product.Name
                Order By Sum(Quantity) desc"
                .FormatWith(new
                {
                    branchId,
                    filterBydays = (days != null) ? "AND OrderProduct.OrderDateUtc > convert(varchar(8), dateadd(day, -" + days + ", getutcdate()), 112)" : "",
                    filterByCustomDays = (startTime != null && endTime != null && startTime != "" && endTime != "") ? "And OrderProduct.OrderDateUtc between " + "'" + startTime + "'" + "and" + "'" + endTime + "'" : ""
                });

            return query;
        }
        public string GetOrderCountSplitByStatusQueryByFilter(int branchId, int? days, string? startTime, string? endTime)
        {
            string query = @"Select OrderStatusId , PaymentMethod, PaymentStatusId, Count(OrderProduct.Id) OrderCount from OrderProduct 
            Inner Join OrderProductItem ON OrderProduct.Id = OrderProductItem.OrderId
             Where OrderProductItem.BranchId = {branchId}
            {filterBydays}
            {filterByCustomDays}
            Group By OrderStatusId , PaymentMethod, PaymentStatusId"
               .FormatWith(new
               {
                   branchId,
                   filterBydays = (days != null) ? "AND OrderProduct.OrderDateUtc > convert(varchar(8), dateadd(day, -" + days + ", getutcdate()), 112)" : "",
                   filterByCustomDays = (startTime != null && endTime != null && startTime != "" && endTime != "") ? "And OrderProduct.OrderDateUtc between " + "'" + startTime + "'" + "and" + "'" + endTime + "'" : ""
               });
            return query;
        }
    }
}
