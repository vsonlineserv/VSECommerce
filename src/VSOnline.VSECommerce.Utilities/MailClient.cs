using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Utilities
{
    public class MailClient
    {
        IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        private readonly IConfigurationSection _appSettings;

        public MailClient()
        {
            _appSettings = _configuration.GetSection("AppSettings");
        }
        private SmtpClient SmtpServer
        {
            get
            {
                SmtpClient SmtpServer = new SmtpClient(_appSettings.GetValue<string>("emailHost").ToString());
                SmtpServer.Port = Convert.ToInt32(_appSettings.GetValue<string>("emailPort"));
                SmtpServer.Credentials = new System.Net.NetworkCredential(_appSettings.GetValue<string>("emailUsername").ToString(), _appSettings.GetValue<string>("emailPassword").ToString());
                SmtpServer.EnableSsl = true;
                return SmtpServer;
            }

        }
        public string GetMailBody(Enums.MailTemplate templateEnum)
        {
            string body = "";
            //Read template file from the App_Data folder
            string eMailTemplateLocation =  @"EmailTemplates/";
            string retailerMailTemplateLocation = @"EmailTemplates/";

            switch (templateEnum)
            {
                case Enums.MailTemplate.RetailerAccountCreated:
                    using (var sr = new StreamReader(retailerMailTemplateLocation + "RetailerRegistration.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.WelcomeEmail:
                    using (var sr = new StreamReader(eMailTemplateLocation + "WelcomeMail.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.UserAccountCreated:
                    using (var sr = new StreamReader(eMailTemplateLocation + "WelcomeMail.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.ForgotPassword:
                    using (var sr = new StreamReader(eMailTemplateLocation + "ForgotPassword.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.ProductRequestQuery:
                    using (var sr = new StreamReader(eMailTemplateLocation + "ProductRequestMail.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.ProductReplyMessage:
                    using (var sr = new StreamReader(retailerMailTemplateLocation + "ProductReplyMail.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.OrderConfirmation:
                    using (var sr = new StreamReader(eMailTemplateLocation + "OrderConfirmation.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.OrderCancellation:
                    using (var sr = new StreamReader(eMailTemplateLocation + "OrderCancellation.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.SiteAdminInformation:
                    using (var sr = new StreamReader(eMailTemplateLocation + "SiteAdminInformation.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;       
                case Enums.MailTemplate.ForgotPasswordForVbuy:
                    using (var sr = new StreamReader(eMailTemplateLocation + "ForgotPasswordForVbuy.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.DeleteInformationMail:
                    using (var sr = new StreamReader(eMailTemplateLocation + "DeleteInformationMail.html"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
            }
            return body;
        }
        public bool SendMail(string toMail, string mailBody, Enums.MailTemplate templateEnum)
        {
            try
            {
                if (!string.IsNullOrEmpty(toMail) && !toMail.ToLower().Contains("test.com"))
                {
                    MailMessage mail = new MailMessage();
                    mail.IsBodyHtml = true;
                    mail.From = new MailAddress(GetFromEmailId(templateEnum), GetFromEmailDisplayName(templateEnum));
                    mail.To.Add(toMail);
                    mail.Subject = GetSubject(templateEnum);
                    mail.Body = mailBody;
                    SmtpServer.Send(mail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }
        public string GetFromEmailId(Enums.MailTemplate templateEnum)
        {
            switch (templateEnum)
            {
                case Enums.MailTemplate.UserAccountCreated:
                case Enums.MailTemplate.RetailerAccountCreated:
                    return "support@vsonlineservices.com";
                case Enums.MailTemplate.WelcomeEmail:
                    return "support@vsonlineservices.com";
            }
            return "support@vsonlineservices.com";
        }
        public string GetFromEmailDisplayName(Enums.MailTemplate templateEnum)
        {
            switch (templateEnum)
            {
                case Enums.MailTemplate.RetailerAccountCreated:
                    return "Seller CRM";
                case Enums.MailTemplate.UserAccountCreated:
                    return "User Verification";
                case Enums.MailTemplate.ForgotPassword:
                    return "Support";
                case Enums.MailTemplate.WelcomeEmail:
                    return "Sivakumar Anirudhan";
                case Enums.MailTemplate.OrderConfirmation:
                    return "Order Confirmation";
                case Enums.MailTemplate.OrderCancellation:
                    return "Order Cancellation";
                case Enums.MailTemplate.ForgotPasswordForVbuy:
                    return "Support";
                case Enums.MailTemplate.DeleteInformationMail:
                    return "Support";
            }
            return "VS Online Services";
        }
        public string GetSubject(Enums.MailTemplate templateEnum)
        {
            switch (templateEnum)
            {
                case Enums.MailTemplate.RetailerAccountCreated:
                    return "Retailer Account Created - Explore your new account now!";
                case Enums.MailTemplate.UserAccountCreated:
                    return "User Account Created - Explore your new account now!";
                case Enums.MailTemplate.WelcomeEmail:
                    return "Welcome to VS Online Services. Online Shopping Mall within your reach";
                case Enums.MailTemplate.ForgotPassword:
                    return "Password Reset instruction";
                case Enums.MailTemplate.ProductRequestQuery:
                    return "Product Enquiry";
                case Enums.MailTemplate.ProductReplyMessage:
                    return "Product Enquiry - Response";
                case Enums.MailTemplate.OrderConfirmation:
                    return "Order Confirmation";
                case Enums.MailTemplate.OrderCancellation:
                    return "Order Cancellation";
                case Enums.MailTemplate.SiteAdminInformation:
                    return "Information from Site";
                case Enums.MailTemplate.ForgotPasswordForVbuy:
                    return "Password Reset instruction";
                case Enums.MailTemplate.DeleteInformationMail:
                    return "Account Deletion";
            }
            return "Thanks for being a part of vsEcommerce";
        }
    }
}
