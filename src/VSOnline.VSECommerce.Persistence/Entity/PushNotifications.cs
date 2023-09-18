using System;
namespace VSOnline.VSECommerce.Persistence.Entity
{
	public class PushNotification
    {
        public int Id { get; set; }
        public string? AuthId { get; set; }
        public string? DeviceToken { get; set; }
        public bool FlagNotification { get; set; }
        public int BranchId { get; set; }
        public Nullable<System.DateTime> CreatedOnUtc { get; set; }
        public Nullable<System.DateTime> UpdatedOnUtc { get; set; }

    }
}

