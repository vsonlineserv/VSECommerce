using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.Caching;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Domain.Settings
{
    public class SiteSettingsService
    {
        private readonly DataContext _context;
        public SiteSettingsService(DataContext context)
        {
            _context = context;
        }
        public List<SiteSettings> GetSiteSettings()
        {
            var siteSettings = _context.SiteSettings.ToList();
            return siteSettings;
        }
    }
}
