using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.DTO;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain
{
    public class CategoryRepository
    {
        private readonly DataContext _context;
        IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        private readonly IConfigurationSection _appSettings;
        public CategoryRepository(DataContext context)
        {
            _context = context;
            _appSettings = _configuration.GetSection("AppSettings");
        }

        public BaseUpdateResultSet CreateCategory(int BranchId, CategoryModelDTO categoryModel, string currentUser)
        {
            BaseUpdateResultSet result = new BaseUpdateResultSet();
            try
            {
                Category category = new Category();
                category.Name = categoryModel.Name;
                category.CategoryGroupTag = categoryModel.CategoryGroupTag;
                category.ParentCategoryId = categoryModel.SelectedCategory;
                category.FlagShowBuy = categoryModel.FlagShowBuy;
                category.GroupDisplayOrder = categoryModel.GroupDisplayOrder;
                category.Published = categoryModel.Published;
                category.CreatedOnUtc = DateTime.UtcNow;
                category.UpdatedOnUtc = DateTime.UtcNow;
                category.PermaLink = categoryModel.Name.ToLower().Replace(" ", "-");
                category.BranchId = BranchId;
                category.CreatedBy = currentUser;
                category.IsDeleted = false;
                _context.Category.Add(category);
                _context.SaveChanges();
                result.UpdatedId = category.CategoryId;
                result.Status = Enums.UpdateStatus.Success;
                return result;
            }
            catch (Exception ex)
            {
                result.Status = Enums.UpdateStatus.Error;
                return result;
            }
        }
        public RetailerProductFilterResult GetRetailerProductFilterResult(RetailerProductFilterResult retailerProductFilterResult)
        {
            var parentCategories = GetAllCategory();
            var subCategoryFilter = GetAllSubCategory();

            retailerProductFilterResult.CategoryFilter = parentCategories.Select(cf => new CategoryFilterDTO { CategoryId = cf.CategoryId, Name = cf.Name })
                .ToList<CategoryFilterDTO>();
            retailerProductFilterResult.SubCategoryFilter = subCategoryFilter;

            return retailerProductFilterResult;

        }
        private List<Category> GetAllCategory()
        {
            return _context.Category.Where<Category>(x => x.ParentCategoryId == null
                && x.IsDeleted != true && x.Published == true).OrderBy(x => x.DisplayOrder).ToList<Category>();
        }
        private List<SubCategoryFilterDTO> GetAllSubCategory()
        {
            return _context.Category.Where<Category>(x => x.ParentCategoryId != null
              && x.IsDeleted != true && x.Published == true)
              .Select(cf => new SubCategoryFilterDTO { CategoryId = cf.CategoryId, Name = cf.Name, ParentCategoryId = cf.ParentCategoryId })
              .ToList<SubCategoryFilterDTO>();
        }
        public List<MenuResult> GetCategoryMenu(List<int> branchIdList)
        {
            var parentCategories = _context.Category.Where(x => x.MarketPlaceVbuyCategory == true && x.ParentCategoryId == null && x.IsDeleted != true && x.Published == true).OrderBy(x => x.DisplayOrder).ToList();
            var menuResultList = new List<MenuResult>();

            foreach (Category parentCategory in parentCategories)
            {
                MenuResult menuResult = new MenuResult();
                menuResult.ParentCategoryId = parentCategory.CategoryId;
                menuResult.ParentCategoryName = parentCategory.Name;
                if (parentCategory.CategoryImage != null)
                {
                    if (!parentCategory.CategoryImage.Contains("http"))
                    {
                        menuResult.CategoryImage = _appSettings.GetValue<string>("ImageUrlBase") + parentCategory.CategoryImage;
                    }
                    else
                    {
                        menuResult.CategoryImage = parentCategory.CategoryImage;
                    }
                }
                menuResult.PermaLink = parentCategory.PermaLink;

                var subCategories = _context.Category.Where(x => x.ParentCategoryId == parentCategory.CategoryId && x.IsDeleted != true && x.Published == true).OrderBy(z => z.GroupDisplayOrder).ThenBy(y => y.CategoryGroupTag).ThenBy(g => g.DisplayOrder);
                menuResult.SubMenu = new List<SubMenuResult>();
                foreach (Category category in subCategories)
                {
                    SubMenuResult subMenuResult = new SubMenuResult();
                    subMenuResult.SubCategoryId = category.CategoryId;
                    subMenuResult.SubCategoryName = category.Name;
                    subMenuResult.CategoryGroupTag = category.CategoryGroupTag;
                    if (category.CategoryImage != null)
                    {
                        if (!category.CategoryImage.Contains("http"))
                        {
                            subMenuResult.CategoryImage = _appSettings.GetValue<string>("ImageUrlBase") + category.CategoryImage;
                        }
                        else
                        {
                            subMenuResult.CategoryImage = category.CategoryImage;
                        }
                    }
                    subMenuResult.PermaLink = category.PermaLink;
                    menuResult.SubMenu.Add(subMenuResult);
                }
                menuResultList.Add(menuResult);
            }
            return menuResultList;
        }

        //for single db Hyperlocal
        public List<MenuResult> GetCategoryMenu_(int BranchId)
        {
            var parentCategories = GetAllCategory_(BranchId);
            var menuResultList = new List<MenuResult>();

            foreach (Category parentCategory in parentCategories)
            {
                MenuResult menuResult = new MenuResult();
                menuResult.ParentCategoryId = parentCategory.CategoryId;
                menuResult.ParentCategoryName = parentCategory.Name;
                if (parentCategory.CategoryImage != null)
                {
                    if (!parentCategory.CategoryImage.Contains("http"))
                    {
                        menuResult.CategoryImage = _appSettings.GetValue<string>("ImageUrlBase") + parentCategory.CategoryImage;
                    }
                    else
                    {
                        menuResult.CategoryImage = parentCategory.CategoryImage;
                    }
                }
                menuResult.PermaLink = parentCategory.PermaLink;

                var subCategories = _context.Category.Where(x => x.ParentCategoryId == parentCategory.CategoryId && x.IsDeleted != true
                && x.Published == true && x.BranchId == BranchId).OrderBy(z => z.GroupDisplayOrder).ThenBy(y => y.CategoryGroupTag).ThenBy(g => g.DisplayOrder);
                menuResult.SubMenu = new List<SubMenuResult>();
                foreach (Category category in subCategories)
                {
                    SubMenuResult subMenuResult = new SubMenuResult();
                    subMenuResult.SubCategoryId = category.CategoryId;
                    subMenuResult.SubCategoryName = category.Name;
                    subMenuResult.CategoryGroupTag = category.CategoryGroupTag;
                    if (category.CategoryImage != null)
                    {
                        if (!category.CategoryImage.Contains("http"))
                        {
                            subMenuResult.CategoryImage = _appSettings.GetValue<string>("ImageUrlBase") + category.CategoryImage;
                        }
                        else
                        {
                            subMenuResult.CategoryImage = category.CategoryImage;
                        }
                    }
                    subMenuResult.PermaLink = category.PermaLink;
                    menuResult.SubMenu.Add(subMenuResult);
                }
                menuResultList.Add(menuResult);
            }
            return menuResultList;
        }
        private List<Category> GetAllCategory_(int BranchId)
        {
            return _context.Category.Where<Category>(x => x.ParentCategoryId == null
                && x.IsDeleted != true && x.Published == true && x.BranchId == BranchId).OrderBy(x => x.DisplayOrder).ToList<Category>();
        }

        // for Hyperlocal
        public string GetStoresCategoryQuery(int storeId)
        {
            var query = @"SELECT distinct Category from Pricing 
                            Inner Join Product ON Product.ProductId = Pricing.Product
                            Inner Join Category On Category.CategoryId = Product.Category
                            WHERE Store = {storeId}".FormatWith(new { storeId });
            return query;
        }

        public RetailerProductFilterResult GetRetailerProductFilterResult(RetailerProductFilterResult retailerProductFilterResult, List<int> categoryList)
        {

            var subCategoryFilter = GetAllSubCategoryForVbuy(categoryList);
            List<int?> parentCategoryIdList = subCategoryFilter.Select(x => x.ParentCategoryId).ToList();
            var parentCategories = GetParentCategoryForVbuy(parentCategoryIdList);

            retailerProductFilterResult.CategoryFilter = parentCategories.Select(cf => new CategoryFilterDTO { CategoryId = cf.CategoryId, Name = cf.Name })
                .ToList<CategoryFilterDTO>();
            retailerProductFilterResult.SubCategoryFilter = subCategoryFilter;

            return retailerProductFilterResult;

        }
        private List<SubCategoryFilterDTO> GetAllSubCategoryForVbuy(List<int> categoryList)
        {
            return _context.Category.Where<Category>(x => x.ParentCategoryId != null
              && x.IsDeleted != true && x.Published == true && categoryList.Contains(x.CategoryId))
              .Select(cf => new SubCategoryFilterDTO { CategoryId = cf.CategoryId, Name = cf.Name, ParentCategoryId = cf.ParentCategoryId })
              .ToList<SubCategoryFilterDTO>();
        }
        private List<Category> GetParentCategoryForVbuy(List<int?> parentCategoryList)
        {
            return _context.Category.Where<Category>(x => x.ParentCategoryId == null && parentCategoryList.Contains(x.CategoryId)
                && x.IsDeleted != true && x.Published == true).OrderBy(x => x.DisplayOrder).ToList<Category>();
        }
    }
}
