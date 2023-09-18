using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Persistence.Data
{
    public class DataContext : DbContext
    {
        public DataContext()
        {
        }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Area>().HasNoKey();
            modelBuilder.Entity<Country>().HasNoKey();
            modelBuilder
        .Entity<PasswordReset>()
        .Property(e => e.Username)
        .ValueGeneratedOnAdd();
        }
        public DbSet<User> User { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Pricing> Pricing { get; set; }
        public DbSet<ProductImage> ProductImage { get; set; }
        public DbSet<ProductContact> ProductContact { get; set; }
        public DbSet<SellerBranch> SellerBranch { get; set; }
        public DbSet<OrderProduct> OrderProduct { get; set; }
        public DbSet<OrderProductItem> OrderProductItem { get; set; }
        public DbSet<NewInventory> NewInventory { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<TaxMaster> TaxMaster { get; set; }
        public DbSet<SubscriptionProvider> SubscriptionProvider { get; set; }
        public DbSet<UserWishlist> UserWishlist { get; set; }
        public DbSet<ShoppingCartItem> ShoppingCartItem { get; set; }
        public DbSet<Discount> Discount { get; set; }
        public DbSet<NewMasterSettingsSelections> NewMasterSettingsSelections { get; set; }
        public DbSet<NewMasterShippingCalculation> NewMasterShippingCalculation { get; set; }
        public DbSet<NewMasterParcelService> NewMasterParcelService { get; set; }
        public DbSet<SellerStaffMapping> SellerStaffMapping { get; set; }
        public DbSet<Seller> Seller { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<Manufacturer> Manufacturer { get; set; }
        public DbSet<SiteSettings> SiteSettings { get; set; }
        public DbSet<Area> Area { get; set; }
        public DbSet<ProductKeyFeatures> ProductKeyFeatures { get; set; }
        public DbSet<ProductSpecification> ProductSpecification { get; set; }
        public DbSet<ProductRating> ProductRating { get; set; }
        public DbSet<SellerRating> SellerRating { get; set; }
        public DbSet<BuyerAddress> BuyerAddress { get; set; }
        public DbSet<PasswordReset> PasswordReset { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<UserPermissionMapping> UserPermissionMapping { get; set; }
        public DbSet<ProductVariants> ProductVariants { get; set; }
        public DbSet<VariantOptions> VariantOptions { get; set; }
        public DbSet<Country> Country { get; set; }
        public DbSet<ShiprocketApiUser> ShiprocketApiUser { get; set; }
        public DbSet<ShiprocketOrderDetails> ShiprocketOrderDetails { get; set; }
        public DbSet<PushNotification> PushNotification { get; set; }
        public DbSet<ProductStoreMapping> ProductStoreMapping { get; set; }
    } 
}
