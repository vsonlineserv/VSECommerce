using AuthPermissions.AspNetCore;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web.Resource;
using System.Data;
using System.Globalization;
using System.Net;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.Order;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Permission;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class OrdersController : VSControllerBase
    {
        private readonly OrderHelper _orderHelper;
        private readonly EfContext _efContext;
        private readonly DataContext _context;
        private readonly UserService _userService;
        private readonly ShoppingCartRepository _shoppingCartRepository;
        private readonly SellerRepository _sellerRepository;
        public OrdersController(OrderHelper orderHelper, EfContext efContext, DataContext context, UserService userService, ShoppingCartRepository shoppingCartRepository, SellerRepository sellerRepository, IOptions<AppSettings> _appSettings) : base(_appSettings)
        {
            _orderHelper = orderHelper;
            _efContext = efContext;
            _context = context;
            _userService = userService;
            _shoppingCartRepository = shoppingCartRepository;
            _sellerRepository = sellerRepository;
        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("GetOrderStatus")]
        public IActionResult GetOrderStatus()
        {
            try
            {
                var sffs = Request.Headers.Referer;
                var adas = Request.Headers.Location;
                var adasd = Request.Headers.Origin;
                var adasadasd = Request.Headers.UserAgent;
                var currentUser = User.FindFirst("SubcriptionExpired")?.Value;
                var orderStatus = new List<object>();

                foreach (var item in Enum.GetValues(typeof(Enums.OrderStatus)))
                {
                    orderStatus.Add(new
                    {
                        id = (int)item,
                        name = item.ToString()
                    });
                }
                return Ok(orderStatus);
            }
            catch (Exception ex)
            {
                return BadRequest("There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/GetPendingOrderHistory/{PageSize}/{PageNo}")]
        public IActionResult GetPendingOrderHistory(int BranchId, int PageSize, int PageNo)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var query = _orderHelper.getOrderDetails(BranchId, PageSize, PageNo);
                var result = _efContext.Database.SqlQuery<OrderResult>(query).ToList();

                var branchDetails = _context.SellerBranch.Where(x => x.BranchId == BranchId).FirstOrDefault();
                var orderIdPrefix = "";
                if (branchDetails != null)
                {
                    orderIdPrefix = branchDetails.OrderIdPrefix == null ? branchDetails.BranchName : branchDetails.OrderIdPrefix;
                }
                if (result.Count > 0)
                {
                    foreach (var orderresult in result)
                    {
                        orderresult.BranchOrderIdWithPrefix = orderIdPrefix.ToUpper() + "-" + orderresult.BranchOrderId;
                        orderresult.OrderStatus = _orderHelper.GetEnumDescription((Enums.OrderStatus)orderresult.OrderStatusId);
                        orderresult.PaymentStatus = _orderHelper.GetEnumDescription((Enums.PaymentStatus)orderresult.PaymentStatusId);
                        orderresult.PaymentMethodString = _orderHelper.GetEnumDescription((Enums.PaymentOption)orderresult.PaymentMethod);
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/GetExcelReport/{Status?}/{searchString?}/{days?}/{startTime?}/{endTime?}")]
        public IActionResult GetExcelReport(int BranchId, int Status, string? searchString, int? days, string? startTime, string? endTime)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var report = GetReportDetails(BranchId, Status, searchString, days, startTime, endTime);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Reports");
                    var currentRow = 1;
                    worksheet.Cell(currentRow, 1).Value = "SUBORDER ID";
                    worksheet.Cell(currentRow, 2).Value = "ORDER ID";
                    worksheet.Cell(currentRow, 3).Value = "ORDER DATE";
                    worksheet.Cell(currentRow, 4).Value = "NAME";
                    worksheet.Cell(currentRow, 5).Value = "QTY";
                    worksheet.Cell(currentRow, 6).Value = "CUSTOMER";
                    worksheet.Cell(currentRow, 7).Value = "UNIT PRIC";
                    worksheet.Cell(currentRow, 8).Value = "PRICE INCL";
                    worksheet.Cell(currentRow, 9).Value = "STATUS";
                    worksheet.Cell(currentRow, 10).Value = "PAYMENT";

                    for (int i = 0; i < report.Rows.Count; i++)
                    {
                        {
                            currentRow++;
                            worksheet.Cell(currentRow, 1).Value = report.Rows[i]["SUBORDER ID"];
                            worksheet.Cell(currentRow, 2).Value = report.Rows[i]["ORDER ID"];
                            worksheet.Cell(currentRow, 3).Value = report.Rows[i]["ORDER DATE"];
                            worksheet.Cell(currentRow, 4).Value = report.Rows[i]["NAME"];
                            worksheet.Cell(currentRow, 5).Value = report.Rows[i]["QTY"];
                            worksheet.Cell(currentRow, 6).Value = report.Rows[i]["CUSTOMER"];
                            worksheet.Cell(currentRow, 7).Value = report.Rows[i]["UNIT PRIC"];
                            worksheet.Cell(currentRow, 8).Value = report.Rows[i]["PRICE INCL"];
                            worksheet.Cell(currentRow, 9).Value = report.Rows[i]["STATUS"];
                            worksheet.Cell(currentRow, 10).Value = report.Rows[i]["PAYMENT"];
                        }
                    }
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalesReport.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        private DataTable GetReportDetails(int branchId, int Status, string? searchString, int? days, string? startTime, string? endTime)
        {
            try
            {
                var ReportsDetails = GetOrdersByFiltersForCsv(branchId, Status, searchString, days, startTime, endTime);

                DataTable dtProduct = new DataTable("OrderDetails");
                dtProduct.Columns.AddRange(new DataColumn[10] {
                                            new DataColumn("SUBORDER ID"),
                                            new DataColumn("ORDER ID"),
                                            new DataColumn("ORDER DATE"),
                                            new DataColumn("NAME"),
                                             new DataColumn("QTY"),
                                            new DataColumn("CUSTOMER"),
                                            new DataColumn("UNIT PRIC"),
                                            new DataColumn("PRICE INCL"),
                                             new DataColumn("STATUS"),
                                            new DataColumn("PAYMENT"),
                                            });
                foreach (var report in ReportsDetails)
                {
                    dtProduct.Rows.Add(report.Id, report.OrderId, report.OrderDateUtc, report.Name, report.Quantity, report.FirstName, report.UnitPriceInclTax, report.PriceInclTax, report.OrderItemStatus, report.PaymentMethodString);
                }

                return dtProduct;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        private List<OrderResult> GetOrdersByFiltersForCsv(int branchId, int Status, string? searchString, int? days, string? startTime, string? endTime)
        {
            try
            {
                var query = _orderHelper.getOrderDetailsbyFiltersForCsv(branchId, Status, searchString, days, startTime, endTime);
                var result = _efContext.Database.SqlQuery<OrderResult>(query).ToList();
                foreach (var orderresult in result)
                {
                    orderresult.OrderItemStatus = _orderHelper.GetEnumDescription((Enums.OrderStatus)orderresult.OrderItemStatusId);
                    orderresult.PaymentStatus = _orderHelper.GetEnumDescription((Enums.PaymentStatus)orderresult.PaymentStatusId);
                    orderresult.PaymentMethodString = _orderHelper.GetEnumDescription((Enums.PaymentOption)orderresult.PaymentMethod);
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/GetOrdersSearch")]
        public IActionResult GetOrdersSearch(int BranchId, string? searchString = null, int? Status = null, int? days = null, string? startTime = null, string? endTime = null)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var branchDetails = _context.SellerBranch.Where(x => x.BranchId == BranchId).FirstOrDefault();
                var orderIdPrefix = "";
                if (branchDetails != null)
                {
                    orderIdPrefix = branchDetails.OrderIdPrefix == null ? branchDetails.BranchName : branchDetails.OrderIdPrefix;
                }
                var query = _orderHelper.getSearchOrdersWithExtraParams(BranchId, searchString, Status, days, startTime, endTime);
                var result = _efContext.Database.SqlQuery<OrderResult>(query).ToList();
                foreach (var orderresult in result)
                {
                    orderresult.BranchOrderIdWithPrefix = orderIdPrefix.ToUpper() + "-" + orderresult.BranchOrderId;
                    orderresult.OrderStatus = _orderHelper.GetEnumDescription((Enums.OrderStatus)orderresult.OrderStatusId);
                    orderresult.PaymentStatus = _orderHelper.GetEnumDescription((Enums.PaymentStatus)orderresult.PaymentStatusId);
                    orderresult.PaymentMethodString = _orderHelper.GetEnumDescription((Enums.PaymentOption)orderresult.PaymentMethod);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/GetOrderDetails/{orderId}")]
        public IActionResult GetOrderDetails(int BranchId, int orderId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var query = _orderHelper.getEachOrderDetails(BranchId, orderId);
                var result = _efContext.Database.SqlQuery<OrderResult>(query).ToList();
                var branchDetails = _context.SellerBranch.Where(x => x.BranchId == BranchId).FirstOrDefault();
                var orderIdPrefix = "";
                if (branchDetails != null)
                {
                    orderIdPrefix = branchDetails.OrderIdPrefix == null ? branchDetails.BranchName : branchDetails.OrderIdPrefix;
                }
                foreach (var orderresult in result)
                {
                    if (orderresult.PictureName != null)
                    {
                        if (!orderresult.PictureName.Contains("http"))
                        {
                            orderresult.PictureName = _appSettings.ImageUrlBase + orderresult.PictureName;
                        }
                    }
                    orderresult.BranchOrderIdWithPrefix = orderIdPrefix.ToUpper() + "-" + orderresult.BranchOrderId;
                    orderresult.OrderStatus = _orderHelper.GetEnumDescription((Enums.OrderStatus)orderresult.OrderStatusId);
                    orderresult.OrderItemStatus = _orderHelper.GetEnumDescription((Enums.OrderStatus)orderresult.OrderItemStatusId);
                    orderresult.PaymentStatus = _orderHelper.GetEnumDescription((Enums.PaymentStatus)orderresult.PaymentStatusId);
                    orderresult.PaymentMethodString = _orderHelper.GetEnumDescription((Enums.PaymentOption)orderresult.PaymentMethod);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Orders_Write)]
        [HttpPost("Seller/{BranchId}/UpdateOrderStatus")]
        public IActionResult UpdateOrderStatusForAll(int BranchId, OrderStatusUpdateModelNew orderStatusUpdate)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var order = _context.OrderProduct.Where(x => x.Id == orderStatusUpdate.OrderId).Include(y => y.OrderProductItem).FirstOrDefault();
                OrderProductItem subOrder = order.OrderProductItem.FirstOrDefault(x => x.OrderId == orderStatusUpdate.OrderId);
                if (subOrder != null)
                {
                    order.OrderStatusId = orderStatusUpdate.StatusId;
                    foreach (var item in order.OrderProductItem)
                    {
                        if (item.OrderItemStatus < orderStatusUpdate.StatusId)
                        {
                            item.OrderItemStatus = orderStatusUpdate.StatusId;
                        }
                    }
                    _context.Update(order);
                    _context.SaveChanges();
                    return Ok(true);
                }
                return Ok(false);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }

        }

        //used in dashboard page Analytics
        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/GetBranchProductSummary")]
        public IActionResult GetBranchProductSummary(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var query = _orderHelper.GetBranchProductSummaryQuery(BranchId);
                var result = _efContext.Database.SqlQuery<ProductSummaryDashboardResult>(query).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/GetOrdersCountByStatus")]
        public IActionResult GetOrdersCountByStatus(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                int Status;
                List<OrderCountResult> orderCountList = new List<OrderCountResult>();
                for (int i = 0; i < 8; i++)
                {
                    OrderCountResult orderCount = new OrderCountResult();
                    if (i == 0)
                    {
                        Status = 1;
                        orderCount.Count = _context.OrderProduct.Count(p => p.BranchId == BranchId && p.OrderStatusId == Status);
                        orderCount.Status = Status;
                        orderCount.Name = "Pending";
                        orderCountList.Add(orderCount);
                    }
                    if (i == 1)
                    {
                        Status = 7;
                        orderCount.Count = _context.OrderProduct.Count(p => p.BranchId == BranchId && p.OrderStatusId == Status);
                        orderCount.Status = Status;
                        orderCount.Name = "Shipped";
                        orderCountList.Add(orderCount);
                    }
                    if (i == 2)
                    {
                        Status = 1000;
                        orderCount.Count = _context.OrderProduct.Count(p => p.BranchId == BranchId && p.OrderStatusId == Status);
                        orderCount.Status = Status;
                        orderCount.Name = "Cancelled";
                        orderCountList.Add(orderCount);
                    }
                    if (i == 3)
                    {
                        Status = 100;
                        orderCount.Count = _context.OrderProduct.Count(p => p.BranchId == BranchId && p.OrderStatusId == Status);
                        orderCount.Status = Status;
                        orderCount.Name = "Hold";
                        orderCountList.Add(orderCount);
                    }
                    if (i == 4)
                    {
                        Status = 20;
                        orderCount.Count = _context.OrderProduct.Count(p => p.BranchId == BranchId && p.OrderStatusId == Status);
                        orderCount.Status = Status;
                        orderCount.Name = "Delivered";
                        orderCountList.Add(orderCount);
                    }
                    if (i == 5)
                    {
                        Status = 990;
                        orderCount.Count = _context.OrderProduct.Count(p => p.BranchId == BranchId && p.OrderStatusId == Status);
                        orderCount.Status = Status;
                        orderCount.Name = "Return";
                        orderCountList.Add(orderCount);

                    }
                    if (i == 6)
                    {
                        Status = 998;
                        orderCount.Count = _context.OrderProduct.Count(p => p.BranchId == BranchId && p.OrderStatusId == Status);
                        orderCount.Status = Status;
                        orderCount.Name = "Refund";
                        orderCountList.Add(orderCount);
                    }
                }
                return Ok(orderCountList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/TotalCustomersUserCount")]
        public IActionResult TotalCustomersUserCount(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var result = new
                {
                    CustomersCount = _context.User.Count(p => p.IsMerchant == false && p.StoreId == BranchId),
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/TotalOrdersSales")]
        public IActionResult TotalOrdersSales(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var result = _context.OrderProduct.Select(x =>
                    new TotalOrderSalesResult
                    {
                        OrderCount = _context.OrderProduct.Count(x => x.BranchId == BranchId),
                        SalesTotal = _context.OrderProduct.Where(x => x.BranchId == BranchId).Sum(x => x.OrderTotal)
                    }).FirstOrDefault();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/TotalWishlistCount")]
        public IActionResult TotalWishlistCount(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var result = new
                {
                    WishlistCount = _context.UserWishlist.Count(x => x.BranchId == BranchId),
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/TotalCartCount")]
        public IActionResult TotalCartCount(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var result = new
                {
                    CartCount = _context.ShoppingCartItem.Where(x => x.BranchId == BranchId).Select(x => x.CustomerId).Distinct().Count(),
                };
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/GetMostSellingProducts")]
        public IActionResult GetMostSellingProducts(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                var IsValid = branchIds.Where(a => a.Value.Contains(BranchId.ToString())).Any();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }

                var result = _context.OrderProductItem.Join(_context.ProductStoreMapping,
                        x => new { x.ProductId, x.BranchId },
                        y => new { y.ProductId, y.BranchId },
                        (x, y) => new { x, y }).Where(x => x.x.BranchId == BranchId).GroupBy(x => x.y.Name)
                        .Select(y => new SellingProductsResult
                        {
                            ProductName = y.Key,
                            OrderCount = y.Count()
                        }).OrderByDescending(x => x.OrderCount).Take(5);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/GetLeastSellingProducts")]
        public IActionResult GetLeastSellingProducts(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var result = _context.OrderProductItem.Join(_context.ProductStoreMapping,
                        x => new { x.ProductId, x.BranchId },
                        y => new { y.ProductId, y.BranchId },
                        (x, y) => new { x, y }).Where(x => x.x.BranchId == BranchId).GroupBy(x => x.y.Name)
                        .Select(y => new SellingProductsResult
                        {
                            ProductName = y.Key,
                            OrderCount = y.Count()
                        }).OrderBy(x => x.OrderCount).Take(5);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/GetTodayAndYesterdaysOrder")]
        public IActionResult GetTodayAndYesterdaysOrder(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var query = _orderHelper.getTodayAndYesterdaysOrder(BranchId);
                var result = _efContext.Database.SqlQuery<OrderSummaryResult>(query).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.General)]
        [HttpGet("Seller/{BranchId}/GetOrdersSummary")]
        public IActionResult GetOrdersSummary(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var query = _orderHelper.getOrdersSummary(BranchId);
                var result = _efContext.Database.SqlQuery<OrderSummaryResult>(query).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/GetOrderSummaryByCategory")]
        public IActionResult GetOrderSummaryByCategory(int BranchId, int? days, string? startTime, string? endTime)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var CategorySummaryDetails = _context.OrderProductItem
                    .Join(_context.OrderProduct, orderProductItem => orderProductItem.OrderId, orderProduct => orderProduct.Id,
                    (orderProductItem, orderProduct) => new { orderProductItem, orderProduct })
                    .Join(_context.ProductStoreMapping, orderProductItemJoined => orderProductItemJoined.orderProductItem.ProductId, product => product.ProductId,
                    (orderProductItemJoined, product) => new { orderProductItemJoined, product })
                    .Join(_context.Category, categoryDetails => categoryDetails.product.Category, category => category.CategoryId,
                    (categoryDetails, category) => new { categoryDetails, category }).Where(z => z.categoryDetails.orderProductItemJoined.orderProductItem.BranchId == BranchId)
                    .GroupBy(d => d.category.Name)
                    .Select(g => new
                    {
                        CategoryName = g.Key,
                        Quantity = g.Sum(s => s.categoryDetails.orderProductItemJoined.orderProductItem.Quantity),
                        Total = g.Sum(s => s.categoryDetails.orderProductItemJoined.orderProductItem.PriceInclTax),
                        CreatedDate = g.FirstOrDefault().categoryDetails.orderProductItemJoined.orderProduct.OrderDateUtc
                    }).ToList().OrderByDescending(e => e.Total).Take(5);
                if (days != null)
                {
                    CategorySummaryDetails = CategorySummaryDetails.Where(a => a.CreatedDate < DateTime.UtcNow.AddDays(-(double)days)).Select(a => a);
                }
                if (!string.IsNullOrEmpty(startTime) && !string.IsNullOrEmpty(endTime))
                {
                    DateTime startDate = DateTime.ParseExact(startTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    DateTime EndDate = DateTime.ParseExact(endTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    CategorySummaryDetails = CategorySummaryDetails.Where(a => a.CreatedDate >= startDate && a.CreatedDate <= EndDate);
                }

                var reducedCategorySummaryDetails = CategorySummaryDetails.Select(a => new { a.CategoryName, a.Quantity, a.Total }).ToList();

                return Ok(reducedCategorySummaryDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/GetOrderSummaryByProduct")]
        public IActionResult GetOrderSummaryByProduct(int BranchId, int? days, string? startTime, string? endTime)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var query = _orderHelper.GetOrderSummaryByProductQueryByfilter(BranchId, days, startTime, endTime);
                var result = _efContext.Database.SqlQuery<ProductSummaryOrdersResult>(query).ToList();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/GetOrderCountSplitByStatus")]
        public IActionResult GetOrderCountSplitByStatus(int BranchId, int? days, string? startTime, string? endTime)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var query = _orderHelper.GetOrderCountSplitByStatusQueryByFilter(BranchId, days, startTime, endTime);
                var result = _efContext.Database.SqlQuery<StatusSummaryOrdersResult>(query).ToList();
                foreach (var orderresult in result)
                {
                    orderresult.OrderStatus = _orderHelper.GetEnumDescription((Enums.OrderStatus)orderresult.OrderStatusId);
                    orderresult.PaymentStatus = _orderHelper.GetEnumDescription((Enums.PaymentStatus)orderresult.PaymentStatusId);
                    orderresult.PaymentMethodString = _orderHelper.GetEnumDescription((Enums.PaymentOption)orderresult.PaymentMethod);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/TotalCreatedDateCount")]
        public IActionResult TotalCreatedDateCount(int BranchId, int? days, string? startTime, string? endTime)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var count = _context.User.Where(x => x.IsMerchant == false && x.StoreId == BranchId).ToList<User>();
                if (days != null)
                {
                    count = (List<User>)count.Where(x => x.CreatedOnUtc > DateTime.Now.AddDays((double)-days) && x.StoreId == BranchId).ToList();
                }
                if (startTime != null && endTime != null && startTime != "" && endTime != "")
                {
                    DateTime startDate = DateTime.ParseExact(startTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    DateTime EndDate = DateTime.ParseExact(endTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    count = (List<User>)count.Where(x => x.CreatedOnUtc >= startDate && x.CreatedOnUtc < EndDate).ToList();
                }
                return Ok(count.Count());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/GetProductsCountByFilter")]
        public IActionResult GetProductsCountByFilter(int BranchId, int? days, string? startTime, string? endTime)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var count = _context.ProductStoreMapping.Where(x => x.BranchId == BranchId && (x.IsDeleted == false || x.IsDeleted == null)).ToList();
                if (days != null)
                {
                    count = (List<ProductStoreMapping>)count.Where(x => x.CreatedOnUtc > DateTime.Now.AddDays((double)-days) && x.BranchId == BranchId).ToList();
                }
                if (startTime != null && endTime != null && startTime != "" && endTime != "")
                {
                    DateTime startDate = DateTime.ParseExact(startTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    DateTime EndDate = DateTime.ParseExact(endTime, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    count = (List<ProductStoreMapping>)count.Where(x => x.CreatedOnUtc >= startDate && x.CreatedOnUtc < EndDate).ToList();
                }
                return Ok(count.Count());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/GetOrdersCountByFilter")]
        public IActionResult GetOrdersCountByFilter(int BranchId, int? days, string? startTime, string? endTime)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var count = _context.OrderProduct.ToList();
                if (days != null)
                {
                    count = (List<OrderProduct>)count.Where(x => x.OrderDateUtc > DateTime.Now.AddDays((double)-days) && x.BranchId == BranchId).ToList();
                }
                if (startTime != null && endTime != null && startTime != "" && endTime != "")
                {
                    DateTime startDate = DateTime.ParseExact(startTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    DateTime EndDate = DateTime.ParseExact(endTime, "MM/dd/yyyy", CultureInfo.InvariantCulture);
                    count = (List<OrderProduct>)count.Where(x => x.OrderDateUtc >= startDate && x.OrderDateUtc < EndDate).ToList();
                }
                return Ok(count.Count());
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }


        [Authorize(Policy = PolicyTypes.Orders_Read)]
        [HttpGet("Seller/{BranchId}/PrintOrderDetails/{orderId}")]
        public IActionResult PrintOrderDetails(int BranchId, int orderId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var orderIdPrefix = "";
                var branchDetails = _context.SellerBranch.Where(x => x.BranchId == BranchId).FirstOrDefault();
                if (branchDetails != null)
                {
                    orderIdPrefix = branchDetails.OrderIdPrefix == null ? branchDetails.BranchName : branchDetails.OrderIdPrefix;
                }
                var currentUser = User.FindFirst("UserId")?.Value;
                if (currentUser != null)
                {
                    var user = _userService.GetUser(Convert.ToInt32(currentUser));
                    OrderConfirmationResult orderDetail = new OrderConfirmationResult();

                    string userName = user.Email;
                    if (string.IsNullOrEmpty(user.Email))
                    {
                        userName = user.PhoneNumber1;
                    }

                    bool vBuyHighlevelusers = _userService.VBuyHighLevelUsers(userName);
                    var orderDTOObj = _shoppingCartRepository.GetOrder(orderId);
                    orderDTOObj.BranchOrderIdWithPrefix = orderIdPrefix.ToUpper() + "-" + orderDTOObj.BranchOrderId;
                    var retailerInfo = _sellerRepository.GetRetailerInfo(currentUser);
                    if (vBuyHighlevelusers)
                    {
                        orderDetail.OrderDetails = orderDTOObj;
                        orderDetail.OrderItemDetails = _shoppingCartRepository.GetOrderedItemlist(orderId);
                        orderDetail.ShippingAddress = _shoppingCartRepository.GetAddress(orderDTOObj.ShippingAddressId);
                        User buyer = _userService.GetUser(orderDTOObj.CustomerId);
                        orderDetail.CustomerName = buyer.FirstName;
                        orderDetail.CustomerEmail = buyer.Email;
                        //send order confirmation email 
                    }
                    else
                    {
                        if (retailerInfo.Branches != null && retailerInfo.Branches.Count > 0)
                        {
                            var branchId = retailerInfo.Branches[0].BranchId;

                            orderDetail.OrderDetails = orderDTOObj;
                            orderDetail.OrderItemDetails = _shoppingCartRepository.GetOrderedItemlist(orderId, branchId);
                            orderDetail.ShippingAddress = _shoppingCartRepository.GetAddress(orderDTOObj.ShippingAddressId);

                            User buyer = _userService.GetUser(orderDTOObj.CustomerId);
                            orderDetail.CustomerName = buyer.FirstName;
                            orderDetail.CustomerEmail = buyer.Email;
                        }
                    }
                    if (vBuyHighlevelusers || (retailerInfo.Branches != null && retailerInfo.Branches.Count > 0))
                    {
                        decimal subOrderTotal = 0;
                        decimal subShippingCharges = 0;
                        foreach (OrderItemResult result in orderDetail.OrderItemDetails)
                        {
                            subOrderTotal = subOrderTotal + result.Price;
                            subShippingCharges = subShippingCharges + result.ShippingCharges;
                        }
                        orderDetail.OrderDetails.OrderSubtotalInclTax = subOrderTotal;
                    }

                    orderDetail.OrderDetails.OrderStatus = _orderHelper.GetEnumDescription((Enums.OrderStatus)orderDetail.OrderDetails.OrderStatusId);

                    return Ok(orderDetail);
                }
                return null;
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }


        // if paid change status
        [Authorize(Policy = PolicyTypes.Orders_Write)]
        [HttpGet("Seller/{BranchId}/ChangePaymentStatus/{orderId}")]
        public IActionResult ChangePaymentStatus(int BranchId, int orderId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (orderId > 0)
                {
                    // change in OrderProductItem table
                    var orderProductItem = _shoppingCartRepository.UpdateOrderProductItemStatus(orderId, (int)Enums.OrderStatus.Verified);

                    // change in OrderProduct table
                    var orderDetails = _context.OrderProduct.Where(x => x.Id == orderId).FirstOrDefault();
                    if (orderDetails != null)
                    {
                        orderDetails.PaymentStatusId = (int)Enums.PaymentStatus.PaymentCompleted;
                        orderDetails.OrderStatusId = (int)Enums.OrderStatus.Verified;
                        _context.OrderProduct.Update(orderDetails);
                        _context.SaveChanges();
                    }
                    return Ok();
                }
                return BadRequest("Order Id Required");

            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [Authorize(Policy = PolicyTypes.Orders_Write)]
        [HttpGet("Seller/{BranchId}/ChangeDeliveredStatus/{orderId}")]
        public IActionResult ChangeDeliveredStatus(int BranchId, int orderId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (orderId > 0)
                {
                    // change in OrderProductItem table
                    var orderProductItem = _shoppingCartRepository.UpdateOrderProductItemStatus(orderId, (int)Enums.OrderStatus.Delivered);

                    // change in OrderProduct table
                    var orderDetails = _context.OrderProduct.Where(x => x.Id == orderId).FirstOrDefault();
                    if (orderDetails != null)
                    {
                        orderDetails.OrderStatusId = (int)Enums.OrderStatus.Delivered;
                        _context.OrderProduct.Update(orderDetails);
                        _context.SaveChanges();
                    }
                    return Ok();
                }
                return BadRequest("Order Id Required");

            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }
        [Authorize(Policy = PolicyTypes.Orders_Write)]
        [HttpGet("Seller/{BranchId}/UpdateOrderConfirmationStatus/{orderId}/{FlagConfirmStatus}")]
        public IActionResult UpdateOrderConfirmationStatus(int BranchId, int orderId, bool FlagConfirmStatus)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                if (orderId > 0)
                {
                    // change in OrderProductItem table
                    var orderProductItem = _shoppingCartRepository.UpdateOrderProductItemStatus(orderId, (int)Enums.OrderStatus.Processing);

                    // change in OrderProduct table
                    var orderDetails = _context.OrderProduct.Where(x => x.Id == orderId).FirstOrDefault();
                    if (orderDetails != null)
                    {
                        orderDetails.OrderStatusId = (int)Enums.OrderStatus.Processing;
                        orderDetails.FlagConfirmStatus = FlagConfirmStatus;
                        _context.OrderProduct.Update(orderDetails);
                        _context.SaveChanges();
                    }
                    return Ok();
                }
                return BadRequest("Order Id Required");

            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        // change orderid prefix
        [HttpGet("Seller/{BranchId}/ChangeOrderIdPrefix")]
        public IActionResult ChangeOrderIdPrefix(int BranchId, string OrderPrefix)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var sellerBranch = _context.SellerBranch.Where(x => x.BranchId == BranchId).FirstOrDefault();
                if (sellerBranch != null)
                {
                    sellerBranch.OrderIdPrefix = OrderPrefix;
                    _context.SellerBranch.Update(sellerBranch);
                    _context.SaveChanges();
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
