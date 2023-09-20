using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Utilities
{
    public static class Enums
    {
        public enum Role
        {
            Administrators = 1,
            ForumModerators = 2,
            Registered = 3,
            Guests = 4,
            StoreAdmin = 5,
            StoreModerator = 6,
            SalesSupport = 7,
            Support = 8,
            Marketing = 9,
            Reserved = 10,
            Reserved1 = 11
        }

        public enum ProductType
        {
            Store = 1,
            Internal = 2,
            Gift = 3
        }

        public enum UpdateStatus
        {
            Success = 1,
            Failure = 2,
            Error = 3,
            AlreadyExist = 4
        }

        public enum MailTemplate
        {
            RetailerAccountCreated = 1,
            WelcomeEmail = 2,
            ForgotPassword = 3,
            UserAccountCreated = 4,
            UserWelcomeEmail = 5,
            ProductRequestQuery = 6,
            ProductReplyMessage = 7,
            OrderConfirmation = 21,
            OrderCancellation = 22,
            OrderRelated = 23,
            SiteAdminInformation = 100,
            ForgotPasswordForVbuy = 101,
            DeleteInformationMail = 102,
            ForgotPasswordMerchant = 103
        }

        public enum SortBy
        {
            StoresCount = 1,
            LowPrice = 2,
            HighPrice = 3,
            NameAsc = 4,
            NameDesc = 5
        }

        public enum OrderStatus
        {
            [Description("Order Created")]
            Created = 1,
            [Description("Under Verification")]
            VerificationInProgress = 4,
            [Description("Verified")]
            Verified = 5,
            [Description("Processing")]
            Processing = 6,
            [Description("Shipping in Progress")]
            ShippingInProgress = 7,
            Delivered = 20,
            Hold = 100,
            Cancelled = 1000,
            Refund = 998,
            Returned = 990
        }
        public enum DeliveryOption
        {
            HomeDelivery = 1,
            StorePickup = 2
        }

        public enum PaymentOption
        {
            [Description("Cash On Delivery")]
            CashOnDelivery = 1,
            [Description("Card On Delivery")]
            CardOnDelivery = 2,
            [Description("PayUMoney")]
            PaymentGateway1 = 3,
            [Description("PayPal")]
            PaymentGateway2 = 4,
            [Description("RazorPay")]
            PaymentGateway3 = 5,
            [Description("Others")]
            OtherGateway = 9,
            [Description("Apple")]
            PaymentGateway4 = 10,
            [Description("Google")]
            PaymentGateway5 = 11,
            [Description("CCAvenue")]
            PaymentGateway6 = 6,
        }

        public enum PaymentStatus
        {
            [Description("Awaiting Payment")]
            PaymentInProgress = 2,
            [Description("Paid")]
            PaymentCompleted = 3,
            [Description("PartialPayment")]
            PartialPayment = 20,
            RefundInitiated = 30,
            RefundCompleted = 40,
            [Description("Payment Failed")]
            PaymentFailed = 50
        }

        public enum DiscountType
        {
            OrderDiscount = 10,
            CategoryDiscount = 20,
            ProductDiscount = 30,
            Others = 100
        }

        public enum SiteSettings
        {
            TopSellingProductCategory,
            TopSellingProductCategory2
        }

        public const string Home_TopProductList1_CACHE_KEY = "VBuy.in_Home_TopProductList1_CACHE_KEY";
        public const string Home_TopProductList2_CACHE_KEY = "VBuy.in_Home_TopProductList2_CACHE_KEY";

        public enum ResultStatus
        {
            None = 0,
            Success = 200,
            Failed = 417,
            Error = 500
        }

        public enum PermissionTypes
        {
            UserCreate,
            UserEdit,
            UserDelete,
        }
    }
}
