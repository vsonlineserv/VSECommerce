using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Helper
{
    public class MessageHelper
    {
        private readonly MessageClient _messageClient;
        public MessageHelper(MessageClient messageClient)
        {
            _messageClient = messageClient;
        }

        public void SendProductReplyMessage(string toNumber, string? productName, string store, string message)
        {
            var messageBody = _messageClient.GetMessageTemplate(Enums.MailTemplate.ProductReplyMessage)
                .FormatWith(new { productName, store });
            _messageClient.SendSMS(toNumber, messageBody, Enums.MailTemplate.ProductReplyMessage);
        }
        public void SendProductRequestMessage(string toNumber, string productName, string smsMessage, string user, string mobile)
        {
            var messageClient = new MessageClient();
            var messageBody = messageClient.GetMessageTemplate(Enums.MailTemplate.ProductRequestQuery)
                .FormatWith(new { user, productName, mobile });
            messageClient.SendSMS(toNumber, messageBody, Enums.MailTemplate.ProductRequestQuery);
        }
        public void SendOrderConfirmationSMS(string orderId, string toNumber)
        {
            var messageClient = new MessageClient();
            var messageBody = messageClient.GetMessageTemplate(Enums.MailTemplate.OrderConfirmation)
                .FormatWith(new { orderNumber = orderId.ToString() });
            messageClient.SendSMS(toNumber, messageBody, Enums.MailTemplate.OrderConfirmation);
        }
        public void SendOrderCancellationSMS(string orderId, string toNumber)
        {
            var messageClient = new MessageClient();
            var messageBody = messageClient.GetMessageTemplate(Enums.MailTemplate.OrderCancellation)
                .FormatWith(new { orderNumber = orderId.ToString() });
            messageClient.SendSMS(toNumber, messageBody, Enums.MailTemplate.OrderCancellation);
        }
    }
}
