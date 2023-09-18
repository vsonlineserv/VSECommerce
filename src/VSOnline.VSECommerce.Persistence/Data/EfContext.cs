using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace VSOnline.VSECommerce.Persistence.Data
{
    public class EfContext : DbContext
    {

        public EfContext(IConfiguration configuration) : base(nameOrConnectionString: configuration.GetConnectionString("DataContext"))
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            Database.SetInitializer<EfContext>(null);
            base.OnModelCreating(modelBuilder);

        }
    }


}
