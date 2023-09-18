using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain
{
    public class SearchConfiguration
    {
        IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        private readonly IConfigurationSection _appSettings;

        public SearchConfiguration()
        {
            _appSettings = _configuration.GetSection("AppSettings");
        }
    }
}
