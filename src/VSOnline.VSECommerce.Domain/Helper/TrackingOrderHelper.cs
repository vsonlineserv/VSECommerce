using System;
using System.Security.Claims;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Helper
{
    public class TrackingOrderHelper
    {
        public string GetOrdersList(int curUser)
        {
            var currentUser = curUser;
            var query = @"select count(Id) 'Orders' from OrderProduct where OrderDateUtc >= Dateadd(Month, Datediff(Month, 0, DATEADD(m, -6, current_timestamp)), 0)
          AND CustomerId = {currentUser}".FormatWith(new { currentUser });
            return query;

        }
        public string GetProductList(int OrderId)
        {
            var query = @" WITH OrdersCte AS 
                  (
                  select *from  OrderProduct where Id={OrderId}
                  ),
                  productCte AS
                  (
                    select orders.OrderDateUtc,product.OrderId,product.ProductId,product.BranchId,product.Quantity,product.PriceInclTax,
                product.OrderItemStatus,product.SelectedSize,product.ShippingCharges,
                orders.CustomerId,orders.OrderTotal,orders.OrderStatusId, orders.PaymentStatusId, orders.PaymentMethod,product.Id from 
                OrdersCte orders inner join OrderProductItem product on orders.Id=product.OrderId 
                  ),
                  ProductNameCte AS
                  (
                     select product.Id,product.OrderId,product.OrderDateUtc,product.ProductId,orderName.Name,product.BranchId,product.Quantity,
                product.PriceInclTax,product.ShippingCharges, product.OrderItemStatus,product.CustomerId, product.OrderTotal, product.SelectedSize,
                product.OrderStatusId,product.PaymentStatusId, product.PaymentMethod from productCte product inner join Product  
                orderName on product.ProductId=orderName.ProductId
                  ),
                 BranchCte AS
                 (
                 select product.Id,product.OrderId,product.ProductId,product.OrderDateUtc,product.Name,product.BranchId,
                Branch.BranchName,product.Quantity,product.PriceInclTax, product.ShippingCharges, product.OrderItemStatus,product.CustomerId, product.SelectedSize,
                product.OrderTotal,product.OrderStatusId, product.PaymentStatusId, product.PaymentMethod from 
                ProductNameCte product inner join  SellerBranch Branch on product.BranchId=Branch.BranchId
                 )
                  select Id,OrderId, Name,BranchId,BranchName,Quantity,PriceInclTax, ShippingCharges, OrderItemStatus AS OrderItemStatusId,  SelectedSize from BranchCte
                               ".FormatWith(new { OrderId });
            return query;
        }
        public string SearchOrders(string OrderId,int currentUser)
        {
            var query = @"  WITH OrdersCte AS
               (

                  select count(Id)as 'Orders' from OrderProduct where CustomerId = {currentUser}
                     )
                    , OrdersProductCte AS
                  (
                   select  orders.Id,orders.BranchOrderId,orders.OrderDateUtc,product.OrderId,product.ProductId,product.BranchId,product.Quantity,product.PriceInclTax,product.ShippingCharges,
                    product.OrderItemStatus, orders.CustomerId,orders.OrderTotal,
                orders.OrderStatusId, orders.PaymentStatusId, orders.PaymentMethod from OrderProduct orders inner join 
                OrderProductItem product on orders .Id=product .OrderId where 
            orders.CustomerId = {currentUser} and (orders.Id='{OrderId}' or orders.BranchOrderId ='{OrderId}')
                     )
                   select  count(ProductId)'NumberOfProducts',OrderTotal,Id,BranchOrderId,OrderDateUtc,PaymentMethod,OrderStatusId, PaymentStatusId
            from  OrdersProductCte where OrderDateUtc >= Dateadd(Month, Datediff(Month, 0, DATEADD(m, -6, current_timestamp)), 0) group by 
        Id,OrderDateUtc,PaymentMethod,OrderTotal,OrderStatusId,PaymentStatusId,BranchOrderId
                         
       ".FormatWith(new { OrderId, currentUser });
            return query;

        }

        public string GetTrackingOrder(int curUser)
        {
            var currentUser = curUser;
            var query = @" WITH OrdersCte AS
               (

                  select count(Id)as 'Orders' from OrderProduct where CustomerId= {currentUser}
                     )
                    , OrdersProductCte AS
                  (
                   select  orders.Id,orders.OrderDateUtc,product.OrderId,product.ProductId,product.BranchId,product.Quantity,orders.BranchOrderId,
                product.PriceInclTax,product.OrderItemStatus,orders.CustomerId,orders.OrderTotal,orders.OrderStatusId,orders.PaymentStatusId,orders.PaymentMethod 
                from OrderProduct orders inner join OrderProductItem product on orders.Id=product.OrderId where orders.CustomerId= {currentUser}
                     )
                   select  count(ProductId)'NumberOfProducts',OrderTotal,Id,BranchOrderId,OrderDateUtc,PaymentMethod,OrderStatusId,PaymentStatusId from  OrdersProductCte 
                   where OrderDateUtc >= Dateadd(Month, Datediff(Month, 0, DATEADD(m, -6, current_timestamp)), 0) 
                   group by Id,OrderDateUtc,PaymentMethod,OrderTotal,OrderStatusId,PaymentStatusId,BranchOrderId order by Id desc"
                .FormatWith(new { currentUser });
            return query;
        }
    }
}

