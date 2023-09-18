using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;

namespace VSOnline.VSECommerce.Domain
{
    public class SearchClient
    {
        IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        private readonly IConfigurationSection _appSettings;
        private readonly SearchConfiguration _searchConfiguration;
        private readonly SearchHelper _searchHelper;

        public SearchClient(SearchHelper searchHelper, SearchConfiguration searchConfiguration)
        {
            _appSettings = _configuration.GetSection("AppSettings");
            _searchConfiguration = searchConfiguration;
            _searchHelper = searchHelper;
        }
    }
}
