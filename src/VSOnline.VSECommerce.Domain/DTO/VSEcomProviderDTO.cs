using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class VSEcomProviderDTO
    {
        public string? PayPalSecretKey { get; set; }
        public string? PayPalSecretId { get; set; }
        public bool PayPalFlagEnabled { get; set; }
        public string? AppleSecretId { get; set; }
        public string? AppleSecretKey { get; set; }
        public bool AppleFlagEnabled { get; set; }
        public string? GoogleSecretId { get; set; }
        public string? GoogleSecretKey { get; set; }
        public bool GoogleFlagEnabled { get; set; }
        public string? RazorSecretKey { get; set; }
        public string? RazorSecretId { get; set; }
        public bool RazorFlagEnabled { get; set; }
        public string? Provider { get; set; }
        public string? OtherSecretKey { get; set; }
        public string? OtherSecretId { get; set; }
        public bool OtherFlagEnabled { get; set; }
        public bool CardOnDeliveryEnbled { get; set; }
        public bool CashOnDeliveryEnabled { get; set; }
        public bool PayUEnabled { get; set; }
    }
}
