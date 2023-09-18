using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Helper
{
    public class SellerContactHelper
    {
        private readonly DataContext _context;

        public SellerContactHelper(DataContext context)
        {
            _context = context;
        }
        public List<SellerContactResult> GetSellerInbox(int BranchId)
        {
            var result = (from ProductContact in _context.ProductContact
                          join Product in _context.ProductStoreMapping
                          on new
                          {
                            Key1 = ProductContact.ProductId,
                            Key2 = ProductContact.BranchId,
                          }
                          equals
                          new
                          {
                            Key1 = Product.ProductId,
                            Key2 = Product.BranchId,
                          }
                          join SellerBranch in _context.SellerBranch
                          on ProductContact.BranchId equals SellerBranch.BranchId
                          where ProductContact.BranchId == BranchId
                          select new SellerContactResult
                          {
                              Id = ProductContact.Id,
                              ContactName = ProductContact.ContactName,
                              Mobile = ProductContact.Mobile,
                              Email = ProductContact.Email,
                              ProductId = ProductContact.ProductId,
                              StoreId = ProductContact.BranchId,
                              Subject = ProductContact.Subject,
                              Reply = ProductContact.Reply,
                              UpdatedOnIST = ProductContact.UpdatedOnUtc,
                              ProductName = Product.Name,
                              BranchName = SellerBranch.BranchName,
                          }).OrderByDescending(x=> x.UpdatedOnIST).ToList();

            return result;
        }
        public string GetBranchEnquirySummaryQuery(int branchId)
        {
            string query = @"select 'Pending' Status, Count(Id) EnquiryCount from ProductContact
                                Where BranchId = {branchId} AND Reply is NULL and ReplyDate is NULL 
                                Union
                                select 'Replied' Status, Count(Id) EnquiryCount from ProductContact
                                Where BranchId = {branchId} AND Reply is NOT NULL and ReplyDate is NOT NULL 
                             ".FormatWith(new { branchId });
            return query;

        }
        public string GetSellerInboxByFilter(int branchId, int? days, int? month, string startTime, string endTime, bool notReplied, string searchString)
        {
            var query = @"SELECT Id,ContactName,Mobile,ProductContact.Email,ProductContact.ProductId,ProductContact.BranchId, 
                        [Subject], Reply, DATEADD(mi, 330, ProductContact.UpdatedOnUtc)  UpdatedOnIST
                        , DATEADD(mi, 330, ProductContact.ReplyDate) ReplyDateIST
                        ,Product.Name ProductName, SellerBranch.BranchName
                        from ProductContact
                        Inner Join Product ON ProductContact.ProductId = Product.ProductId
                        Inner Join SellerBranch ON ProductContact.BranchId = SellerBranch.BranchId
                        WHERE ProductContact.BranchId = {branchId}
                        {filterBydays}
                        {filterByMonths}
                        {filterByCustomDays}
                        {notRepliedOnly}
                        {searchString}
                        Order By ProductContact.UpdatedOnUtc DESC"
                    .FormatWith(new
                    {
                        branchId,
                        filterBydays = (days != null) ? "AND ProductContact.UpdatedOnUtc > convert(varchar(8), dateadd(day, -" + days + ", getutcdate()), 112)" : "",
                        filterByMonths = (month != null && month > 0) ? "AND ProductContact.UpdatedOnUtc > GETUTCDATE() -" + month : "",
                        notRepliedOnly = (notReplied) ? "AND ProductContact.Reply is NULL or ProductContact.Reply = '' " : "",
                        searchString = (searchString != null && searchString != "") ? "And (ProductContact.ContactName like '%" + searchString + "%' OR ProductContact.Email like '%" + searchString + "%' OR Product.Name like '%" + searchString + "%'  OR ProductContact.Mobile like '%" + searchString + "%')" : "",
                        filterByCustomDays = (startTime != null && endTime != null && startTime != "" && endTime != "") ? "And ProductContact.UpdatedOnUtc between " + "'" + startTime + "'" + "and" + "'" + endTime + "'" : ""
                    });
            return query.ToString();

        }
        public void UpdateReply(int mailId, string reply)
        {
            var productContactDetail = _context.ProductContact.Where(a => a.Id == mailId).FirstOrDefault();
            if (productContactDetail != null)
            {
                productContactDetail.Reply = reply;
                productContactDetail.ReplyDate = DateTime.UtcNow;
                _context.ProductContact.Update(productContactDetail);
                _context.SaveChanges();
            }
        }

        public ProductContact? GetContactInformation(int mailId)
        {
            var productContactDetail = _context.ProductContact.Where(a => a.Id == mailId).FirstOrDefault();
            if (productContactDetail != null)
            {
                return productContactDetail;
            }
            return null;
        }
        public string VerifyDuplicateInbox(int? productId, int? branchId, string name, string email, string mobileNumber)
        {

            var query = @"select count(*) from  ProductContact
                WHERE mobile='{mobileNumber}' AND Email = '{email}' AND ProductId= {productId} AND BranchId = {branchId}
                AND UpdatedOnUtc > '{allowedDate}'"
                .FormatWith(new { mobileNumber, email, productId, branchId, allowedDate = DateTime.UtcNow.AddHours(-2).ToString("yyyy-MM-dd HH:mm") });
            return query;
        }
        public string InsertSellerContactQuery(int productId, int BranchId, string name, string email, string mobileNumber, string subject)
        {
            try
            {
                ProductContact productContact = new ProductContact();
                productContact.ContactName = name;
                productContact.Mobile = mobileNumber;
                productContact.Email = email;
                productContact.Subject = subject;
                productContact.ProductId = productId;
                productContact.BranchId = BranchId;
                productContact.UpdatedOnUtc = DateTime.UtcNow;
                _context.Add(productContact);
                _context.SaveChanges();
                return "Ok";
            }
            catch (Exception ex)
            {
                return "";
            }
        }

    }
}
