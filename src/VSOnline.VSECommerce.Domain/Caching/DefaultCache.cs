using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.Helper;
using VSOnline.VSECommerce.Domain.Infrastructure;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Caching
{
    public class DefaultCache
    {
        private readonly IMemoryCache _cache;
        private readonly DataContext _context;
        private readonly ProductHelper _productHelper;
        private readonly IMapper _mapper;
        public DefaultCache(IMemoryCache memoryCache, DataContext context, ProductHelper productHelper, IMapper mapper)
        {
            _cache = memoryCache;
            _context = context;
            _productHelper = productHelper;
            _mapper = mapper;
        }

        public List<ProductModelWithCategory> GetProductModelCategory(string enumCacheString,string categoryValue,List<int> branchIdList)
        {
            List<ProductModelWithCategory> cacheProductModelList;
            if (!_cache.TryGetValue(enumCacheString, out cacheProductModelList))
            {
                var categoryKeyInt = Int32.Parse(categoryValue);
                var productList = _context.ProductStoreMapping.Where(x => branchIdList.Contains(x.BranchId) && x.Category == categoryKeyInt && x.Published == true && x.IsDeleted != true && x.ShowOnHomePage == true).Include(y => y.CategoryDetails).Distinct().Take(6).ToList();
                List<ProductModelWithCategory> productModelList = new List<ProductModelWithCategory>();
                cacheProductModelList = _mapper.Map<List<ProductStoreMapping>, List<ProductModelWithCategory>>(productList, productModelList);
                _cache.Set(enumCacheString, cacheProductModelList);
            }
            return cacheProductModelList;
        }
        public List<ProductModelWithCategory> GetProductModelCategory(string enumCacheString, string categoryValue, int BranchId)
        {
            List<ProductModelWithCategory> cacheProductModelList;
            if (!_cache.TryGetValue(enumCacheString, out cacheProductModelList))
            {
                var categoryKeyInt = Int32.Parse(categoryValue);
                var productList = _context.ProductStoreMapping.Where(x => x.Category == categoryKeyInt && x.Published == true && x.IsDeleted != true && x.ShowOnHomePage == true
                && x.BranchId == BranchId).Include(y => y.CategoryDetails).Distinct().Take(6).ToList();

                List<ProductModelWithCategory> productModelList = new List<ProductModelWithCategory>();
                cacheProductModelList = _mapper.Map<List<ProductStoreMapping>, List<ProductModelWithCategory>>(productList, productModelList);
                _cache.Set(enumCacheString, cacheProductModelList);
            }
            return cacheProductModelList;
        }
    }
}
