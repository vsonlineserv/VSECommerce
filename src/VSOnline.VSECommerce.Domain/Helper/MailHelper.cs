using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Helper
{
    public class MailHelper
    {
        private readonly MailClient _mailClient;
        public MailHelper(MailClient mailClient)
        {
            _mailClient = mailClient;
        }

        public void SendProductReplyMail(string toMail, string productName, string store, string message, string siteAddress)
        {
            var mailBody = _mailClient.GetMailBody(Enums.MailTemplate.ProductReplyMessage)
                .FormatWith(new { productName, store, message, siteAddress });
            _mailClient.SendMail(toMail, mailBody, Enums.MailTemplate.ProductReplyMessage);
        }
        public void SendProductRequestMail(string branch, string toMail, string name, string userMail, string mobile, string productName, string message)
        {
            var mailClient = new MailClient();
            var mailBody = mailClient.GetMailBody(Enums.MailTemplate.ProductRequestQuery)
                .FormatWith(new { store = branch, productName, message, name, number = mobile, email = userMail });
            mailClient.SendMail(toMail, mailBody, Enums.MailTemplate.ProductRequestQuery);
        }
        public void SendForgetPasswordMail(string toMail, string uniqueId, string username,string storeName)
        {
            var mailClient = new MailClient();
            var mailBody = mailClient.GetMailBody(Enums.MailTemplate.ForgotPassword).FormatWith
                (new { uid = uniqueId, username, storeName });
            mailClient.SendMail(toMail, mailBody, Enums.MailTemplate.ForgotPassword);
        }
        public void SendWelcomeMail(string toMail, string storeName)
        {
            var mailClient = new MailClient();
            var mailBody = mailClient.GetMailBody(Enums.MailTemplate.WelcomeEmail).FormatWith(new { storeName });
            mailClient.SendMail(toMail, mailBody, Enums.MailTemplate.WelcomeEmail);
        }
        public void SendOrderConfirmationMail(string customerEmail, string orderNumber, string trOrderConfirmation,
        decimal orderTotalInclTax, decimal? shippingCharges, decimal orderDiscount, decimal netPayable,
        string CustomerName, string Address1, string Address2, string City, string State,
        string PostalCode, string PhoneNumber, string storeName,decimal? OrderTaxTotal)
        {
            var mailClient = new MailClient();
            var mailBody = mailClient.GetMailBody(Enums.MailTemplate.OrderConfirmation)
                .FormatWith(new
                {
                    CustomerEmail = customerEmail,
                    orderNumber,
                    trOrderConfirmation,
                    orderTotalInclTax,
                    shippingCharges,
                    orderDiscount,
                    netPayable,
                    CustomerName,
                    Address1,
                    Address2,
                    State,
                    City,
                    PostalCode,
                    PhoneNumber,
                    storeName,
                    OrderTaxTotal
                });
            mailClient.SendMail(customerEmail, mailBody, Enums.MailTemplate.OrderConfirmation);
        }
        public void SendRegisterRetailerMail(string username, string toMail, string businessname)
        {
            var mailClient = new MailClient();
            var mailBody = mailClient.GetMailBody(Enums.MailTemplate.RetailerAccountCreated)
                .FormatWith(new { businessname, username });
            mailClient.SendMail(toMail, mailBody, Enums.MailTemplate.RetailerAccountCreated);
        }
        public void SendOrderCancellationMail(string toMail, string orderNumber)
        {
            var mailClient = new MailClient();
            var mailBody = mailClient.GetMailBody(Enums.MailTemplate.OrderCancellation)
                .FormatWith(new { orderNumber });
            mailClient.SendMail(toMail, mailBody, Enums.MailTemplate.OrderCancellation);
        }
        //Hyperlocal
        public void SendForgetPasswordMailForVbuy(string toMail, string uniqueId, string username)
        {
            var mailClient = new MailClient();
            var mailBody = mailClient.GetMailBody(Enums.MailTemplate.ForgotPasswordForVbuy).FormatWith
                (new { uid = uniqueId, username });
            mailClient.SendMail(toMail, mailBody, Enums.MailTemplate.ForgotPasswordForVbuy);
        }
        public void SendDeleteInformationMail(string toMail,string username)  
        {
            var mailClient = new MailClient();
            var mailBody = mailClient.GetMailBody(Enums.MailTemplate.DeleteInformationMail).FormatWith (new { userName = username, email = toMail });     
            mailClient.SendMail(toMail, mailBody, Enums.MailTemplate.DeleteInformationMail);
        }
    }

}
