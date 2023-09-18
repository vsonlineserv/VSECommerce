using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Domain.ResultSet;
using VSOnline.VSECommerce.Persistence.Entity;

namespace VSOnline.VSECommerce.Domain.DTO
{
    public class DTOMapper
    {
        private readonly IMapper _mapper;

        public Category ToEntity(CategoryResult dto, Category destination)
        {
            return _mapper.Map(dto, destination);
        }

        public List<CategoryResult> ToEntityList(List<Category> categoryList,List<CategoryResult> categoryDTOList)
        {
            return _mapper.Map<List<Category>, List<CategoryResult>>(categoryList, categoryDTOList);
        }

        public List<ProductModelWithCategory> ToProductModelWithCategoryList(List<Product> productList, List<ProductModelWithCategory> productModelWithCategoryList)
        {
            return _mapper.Map<List<Product>, List<ProductModelWithCategory>>(productList, productModelWithCategoryList);
        }

        public CategoryResult ToEntityObject(Category category,CategoryResult categoryResult)
        {
            return _mapper.Map<Category, CategoryResult>(category, categoryResult);
        }
    }
}
