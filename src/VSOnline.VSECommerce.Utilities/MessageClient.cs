using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Utilities
{
    public class MessageClient
    {
        string messageApiUrl = @"";
        public string GetMessageTemplate(Enums.MailTemplate templateEnum)
        {
            string body = "";
            //Read template file from the App_Data folder
            string eMailTemplateLocation = @"EmailTemplates\";
            string retailerMailTemplateLocation = @"EmailTemplates\";


            switch (templateEnum)
            {
                case Enums.MailTemplate.ProductRequestQuery:
                    using (var sr = new StreamReader(eMailTemplateLocation + "ProductRequestSMS.txt"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.ProductReplyMessage:
                    using (var sr = new StreamReader(retailerMailTemplateLocation + "ProductReplySMS.txt"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.OrderCancellation:
                    using (var sr = new StreamReader(eMailTemplateLocation + "OrderCancellationSMS.txt"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;
                case Enums.MailTemplate.OrderConfirmation:
                    using (var sr = new StreamReader(eMailTemplateLocation + "OrderConfirmationSMS.txt"))
                    {
                        body = sr.ReadToEnd();
                    }
                    break;

            }
            return body;
        }
        public bool SendSMS(string toNumber, string smsMessage, Enums.MailTemplate template)
        {
            try
            {
                if (!string.IsNullOrEmpty(toNumber))
                {
                    string formattedMessageApiUrl = messageApiUrl.FormatWith(new { toNumber, smsMessage });
                    WebRequest request = WebRequest.Create(formattedMessageApiUrl);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream s = (Stream)response.GetResponseStream();
                    StreamReader readStream = new StreamReader(s);
                    string dataString = readStream.ReadToEnd();
                    response.Close();
                    s.Close();
                    readStream.Close();
                }
            }
            catch
            {

            }
            return false;
        }
    }
}
