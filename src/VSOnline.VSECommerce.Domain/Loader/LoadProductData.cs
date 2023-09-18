using Microsoft.Extensions.Configuration;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Domain.Loader
{
    public class LoadProductData
    {
        IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        public string sqlConnectionString;

        private static readonly ILog logger = LogManager.GetLogger(typeof(LoadDataFromIndependentExcel));
        public LoadProductData()
        {
            sqlConnectionString = _configuration.GetConnectionString("DataContext");
        }

        public int LoadPricing()
        {
            int productsCount = 0;
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadPricingTableQuery, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                var s = ex.Message;
            }

            return productsCount;
        }
        public int LoadKeyFeatures()
        {
            int keyFeaturesCount = 0;
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlKeyFeaturesCommand = new SqlCommand(loadKeyFeaturesQuery, sqlconn);
                sqlconn.Open();
                keyFeaturesCount = sqlKeyFeaturesCommand.ExecuteNonQuery();
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            return keyFeaturesCount;
        }
        public int LoadDetailedSpecification()
        {

            int specificationResult = 0;
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlSpecificationCommand = new SqlCommand(loadSpecificationQuery, sqlconn);
                sqlSpecificationCommand.CommandTimeout = 0;
                sqlconn.Open();
                specificationResult = sqlSpecificationCommand.ExecuteNonQuery();
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            return specificationResult;
        }
        protected internal int LoadCategoryMaster()
        {
            int rows = 0;
            try
            {

                var loadCategoryMasterFilterTableQuery = @"MERGE CategoryMasterFilter AS TARGET USING (select distinct CategoryId,FilterParameter 
                                                   from temp_ProductFilterValue) AS SOURCE 
                                                          ON (TARGET.FilterParameter = SOURCE.FilterParameter
                                               AND TARGET.Category = SOURCE.CategoryId) 
                                                          WHEN NOT MATCHED BY TARGET THEN 
                                                          INSERT (Category, FilterParameter) 
                                                          VALUES (SOURCE.CategoryId, SOURCE.FilterParameter);";
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadCategoryMasterFilterTableQuery, sqlconn);
                sqlconn.Open();
                var Records = sqlProductCommand.ExecuteNonQuery();
                //MessageBox.Show(Records.ToString() + "   Rows Updated");
                rows = Records;
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            finally
            {

            }
            return rows;
        }
        protected internal void LoadProductFilterValueTableFromtemp_ProductFilterValueWithProductName()
        {
            try
            {
                var sqlQuery = @" INSERT INTO temp_ProductFilterValue
                     (CategoryId, ProductId, FilterParameter, FilterValue, FilterValueText)
                     (
                     Select Category.Categoryid CategoryId, Product.ProductId ProductId,  
                     temp_ProductFilterValueWithProductName.FilterParameter, 
                      temp_ProductFilterValueWithProductName.FilterValue, 
                       temp_ProductFilterValueWithProductName.FilterValueText
                     from Product 
                     Inner Join Category ON Product.Category = Category.CategoryId 
                     INNER JOIN temp_ProductFilterValueWithProductName
                     ON temp_ProductFilterValueWithProductName.ProductName = Product.Name COLLATE SQL_Latin1_General_CP1_CI_AS
                     )";
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(sqlQuery, sqlconn);
                sqlconn.Open();
                var Records = sqlProductCommand.ExecuteNonQuery();
                //MessageBox.Show(Records.ToString() + "   Rows Updated");

                sqlconn.Close();
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            finally
            {

            }
        }
        protected internal int LoadProductFilterValue()
        {
            var rows = 0;
            try
            {
                var LoadProductFilterValueTableQuery = @"MERGE ProductFilterValue AS TARGET
                                                          USING (select DISTINCT filter.Category,filter.FilterParameter,filter.Id,tempFilter.FilterValue,tempFilter.FilterValueText,tempFilter.ProductId from CategoryMasterFilter filter inner join  temp_ProductFilterValue tempFilter on filter.FilterParameter=tempFilter.FilterParameter and filter.Category=tempFilter.CategoryId and filter.Category is not null ) AS SOURCE 
                                                          ON (TARGET.CategoryMasterFilter = SOURCE.Id and TARGET.ProductId=SOURCE.ProductId and TARGET.FilterValue=SOURCE.FilterValue and TARGET.FilterValueText= SOURCE.FilterValueText) 
                                                           WHEN NOT MATCHED BY TARGET THEN 
                                                           INSERT (ProductId,CategoryMasterFilter,FilterValue,FilterValueText) 
                                                            VALUES (SOURCE.ProductId,SOURCE.Id,SOURCE.FilterValue,SOURCE.FilterValueText);";
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(LoadProductFilterValueTableQuery, sqlconn);
                sqlconn.Open();
                var Records = sqlProductCommand.ExecuteNonQuery();
                //MessageBox.Show(Records.ToString() + "   Rows Updated");
                rows = Records;
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                var s = ex.Message;
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            finally
            {

            }
            return rows;
        }
        public int LoadCategory(int storeId, int branchId)
        {
            int categoryResult = 0;
            int subCategoryResult = 0;
            try
            {
                string categoryQuery = loadCategoryQuery(storeId, branchId);
                string subCategoryQuery = loadSubCategoryQuery(storeId, branchId);

                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlCategoryCommand = new SqlCommand(categoryQuery, sqlconn);
                SqlCommand sqlSubCategoryCommand = new SqlCommand(subCategoryQuery, sqlconn);
                sqlconn.Open();
                categoryResult = sqlCategoryCommand.ExecuteNonQuery();
                subCategoryResult = sqlSubCategoryCommand.ExecuteNonQuery();
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            return categoryResult + subCategoryResult;
        }
        public int LoadManufacturer()
        {
            int result = 0;
            var query = @"MERGE INTO Manufacturer AS target
                USING (SELECT DISTINCT Manufacturer FROM temp_Product) AS source
                ON (target.Name = source.Manufacturer)
                WHEN NOT MATCHED BY TARGET THEN
                INSERT (Name, MetaTitle) Values (source.Manufacturer, source.Manufacturer);";
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlcmd = new SqlCommand(query, sqlconn);
                sqlconn.Open();
                result = sqlcmd.ExecuteNonQuery();
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            return result;
        }
        public int LoadProductsAndMapping()
        {
            int productsCount = 0;
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadProductsTableQuery, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            //hack for now as we divide by 2.
            return productsCount / 2;

        }

        private string loadPricingTableQuery = @"
                DECLARE @T TABLE(ProductId INT, StoreId INT, BranchId INT);
                 MERGE INTO Pricing AS target
                USING (SELECT DISTINCT  ProductId, temp_pricing.StoreId,	temp_pricing.BranchId,	
						temp_pricing.BranchName,	Price,	SpecialPrice,	AdditionalShippingCharge,	AdditionalTax
						,Name
				        FROM temp_Pricing
                       INNER JOIN Product
					ON (Product.Name =  temp_pricing.ProductName COLLATE SQL_Latin1_General_CP1_CI_AS And Product.StoreId =  temp_pricing.StoreId AND Product.BranchId =  temp_pricing.BranchId)
					INNER JOIN SellerBranch 
					ON SellerBranch.BranchName = temp_pricing.BranchName COLLATE SQL_Latin1_General_CP1_CI_AS
					AND SellerBranch.BranchId = temp_pricing.BranchId
                    ) AS source
                    ON (target.Product = source.ProductId
				    AND target.Store = source.StoreId
                    AND target.Branch = source.BranchId
					)
                WHEN NOT MATCHED BY TARGET THEN
				INSERT (Product, Store , Branch, Price, SpecialPrice,AdditionalShippingCharge,  AdditionalTax, CallForPrice, OldPrice, ProductCost,IsDeleted
				   ) 
				    Values
                    ( source.ProductId, source.StoreId, source.BranchId, source.Price, source.SpecialPrice, source.AdditionalShippingCharge
					, source.AdditionalTax, 0, '0.0000', '0.0000','False')
                WHEN MATCHED THEN
				UPDATE SET target.OldPrice = target.Price, target.Price = source.Price, target.SpecialPrice = source.SpecialPrice,
				target.AdditionalShippingCharge = source.AdditionalShippingCharge,target.AdditionalTax = source.AdditionalTax,
				target.CallForPrice = 0
                OUTPUT source.ProductId,source.StoreId, source.BranchId INTO @T;
                Delete tmpPricing FROM temp_Pricing tmpPricing INNER JOIN @T tempResult 
			                ON tempResult.StoreId = tmpPricing.StoreId
			                AND tempResult.BranchId = tmpPricing.BranchId
			                INNER JOIN Product ON Product.Name = tmpPricing.ProductName
			                AND tempResult.ProductId = Product.ProductId
			                INNER JOIN Pricing ON Product.ProductId = Pricing.Product
			                AND Pricing.SpecialPrice = tmpPricing.SpecialPrice
			                AND Pricing.Price = tmpPricing.Price
			                AND Pricing.Store = tmpPricing.StoreId
			                AND Pricing.Branch = tmpPricing.BranchId";

        private const string loadKeyFeaturesQuery = @" MERGE INTO ProductKeyFeatures AS target
                USING (SELECT DISTINCT ProductId, Parameter, KeyFeature FROM temp_ProductKeyFeatures
				INNER JOIN Product ON temp_ProductKeyFeatures.ProductName = Product.Name COLLATE SQL_Latin1_General_CP1_CI_AS)
				AS source
                ON (target.ProductId = source.ProductId
                    AND target.Parameter = source.Parameter COLLATE SQL_Latin1_General_CP1_CI_AS
                    AND target.KeyFeature = source.KeyFeature COLLATE SQL_Latin1_General_CP1_CI_AS)
                WHEN NOT MATCHED BY TARGET THEN
                INSERT (ProductId,Parameter, KeyFeature) Values (source.ProductId, source.Parameter, source.KeyFeature);";

        private const string loadSpecificationQuery = @"MERGE INTO ProductSpecification AS target
                USING (SELECT DISTINCT ProductId, SpecificationGroup,SpecificationAttribute,SpecificationDetails
				FROM [temp_ProductSpecification]
				INNER JOIN Product ON [temp_ProductSpecification].ProductName = Product.Name COLLATE SQL_Latin1_General_CP1_CI_AS)
				AS source
                ON (target.ProductId = source.ProductId
				 AND target.SpecificationGroup = source.SpecificationGroup COLLATE SQL_Latin1_General_CP1_CI_AS
                    AND target.SpecificationAttribute = source.SpecificationAttribute COLLATE SQL_Latin1_General_CP1_CI_AS
                    AND target.SpecificationDetails = source.SpecificationDetails COLLATE SQL_Latin1_General_CP1_CI_AS
					)
                WHEN NOT MATCHED BY TARGET THEN
                INSERT (ProductId, SpecificationGroup,SpecificationAttribute,SpecificationDetails) 
				Values (source.ProductId, source.SpecificationGroup,source.SpecificationAttribute,source.SpecificationDetails);";

        private string loadCategoryQuery(int storeId, int branchId)
        {
            string query = @"MERGE INTO Category AS target
                USING (SELECT DISTINCT Category, BranchId, StoreId FROM temp_Product) AS source
                ON (target.Name = source.Category AND target.BranchId = source.BranchId AND target.StoreId = source.StoreId)
                WHEN NOT MATCHED BY TARGET THEN
                INSERT (Name,CreatedOnUtc,PermaLink,StoreId,BranchId,IsDeleted,Published) Values (source.Category,'{cur_date}',REPLACE(LTRIM(RTRIM(LOWER(source.Category))), ' ', ''),'{storeId}','{branchId}','False','True');".FormatWith(new { cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"), storeId, branchId });
            return query;
        }

        private string loadSubCategoryQuery(int storeId, int branchId)
        {
            string query = @"With CategoryCTE AS 
                (
                Select distinct CategoryId, Category.Name from Category
                Inner Join temp_Product
                ON (Category.Name = temp_Product.Category and Category.BranchId = temp_Product.BranchId and Category.StoreId = temp_Product.StoreId)
                )
                ,
                SubCategoryTempCTE AS 
                (
               Select Distinct CategoryId,SubCategory, temp_Product.StoreId, temp_Product.BranchId from temp_Product
               Inner Join CategoryCTE
               ON CategoryCTE.Name = temp_Product.Category
                )
               MERGE INTO Category AS target
               USING (SELECT DISTINCT SubCategory,CategoryId,StoreId, BranchId   FROM SubCategoryTempCTE) AS source
               ON (target.storeid = source.storeid and target.branchid = source.branchid and target.Name = source.SubCategory)
               WHEN NOT MATCHED BY TARGET THEN
               INSERT (Name, ParentCategoryId,CreatedOnUtc,PermaLink,StoreId,BranchId,IsDeleted,Published) Values (source.SubCategory,CategoryId,'{cur_date}',REPLACE(LTRIM(RTRIM(LOWER(source.SubCategory))), ' ', ''),'{storeId}','{branchId}','False','True');".FormatWith(new { cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"), storeId, branchId });
            return query;
        }

        private string loadProductsTableQuery = @"declare @T table
                (
                  Name nvarchar(400),
                  MetaTitle nvarchar(400)
                );

                MERGE INTO Product AS target
                USING (SELECT DISTINCT temp_Product.Name, temp_Product.ShortDescription, temp_Product.FullDescription, 
				    temp_Product.MetaTitle, temp_Product.MetaKeywords, temp_Product.MetaDescription, ManufacturerPartNumber,
				    Gtin, IsGiftCard, Weight, Length, Width, Height, temp_Product.Color, temp_Product.DisplayOrder, temp_Product.Published
                    , ManufacturerId , SubCategory.CategoryId, temp_Product.Size1,temp_Product.Size2, temp_Product.Size3, temp_Product.Size4
                       , temp_Product.Size5, temp_Product.Size6,temp_Product.StoreId,temp_Product.BranchId
				        FROM temp_Product
                        INNER JOIN Manufacturer
					    ON temp_Product.Manufacturer = Manufacturer.MetaTitle
                        INNER JOIN Category SubCategory
					            ON (temp_Product.SubCategory = SubCategory.Name AND temp_Product.BranchId = SubCategory.BranchId AND temp_Product.StoreId = SubCategory.StoreId)
					    INNER JOIN Category cat
					         ON cat.CategoryId = SubCategory.ParentCategoryId 
                    ) AS source
                    ON (target.Name = source.Name
                    AND target.Manufacturer = source.ManufacturerId
                    AND target.Category = source.CategoryId AND target.BranchId = source.BranchId AND target.StoreId = source.StoreId)
                WHEN NOT MATCHED BY TARGET THEN
                INSERT (
				    ProductTypeId, Name, ShortDescription, FullDescription, 
				    MetaTitle,MetaKeywords, MetaDescription, ManufacturerPartNumber,
				    Gtin, IsGiftCard, Weight, Length, Width, Height, Color, DisplayOrder, Published
				    , Manufacturer, Category, ShowOnHomePage, CreatedOnUtc, UpdatedOnUtc, 
                    Size1, Size2, Size3,Size4,Size5,Size6,PermaLink,StoreId,BranchId,IsDeleted) 
				    Values
                    (1, source.Name, source.ShortDescription, ISNULL(source.FullDescription, source.Name),
				    source.Name, source.Name, ISNULL(source.ShortDescription, source.Name), source.ManufacturerPartNumber,
				    source.Gtin, 'False', source.Weight, source.Length, 
				    source.Width, source.Height, source.Color, 0, 'True'
                    , source.ManufacturerId, source.CategoryId, 'False'
				,'{cur_date}','{cur_date}'
                , Size1, Size2, Size3,Size4,Size5,Size6,REPLACE(LTRIM(RTRIM(LOWER(source.Name))), ' ', ''), source.StoreId, source.BranchId,'False')
                OUTPUT Inserted.Name, Inserted.MetaTitle INTO @T;


                UPDATE temp_Product
                SET Published = 'True'
                from @T as T
                where temp_Product.Name = T.Name COLLATE SQL_Latin1_General_CP1_CI_AS
                ".FormatWith(new { cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss") });//todo:may be later we can consider category, manufaturer.

        public int LoadCategoryForDefault(int storeId, int branchId)
        {
            int categoryResult = 0;
            int subCategoryResult = 0;
            try
            {
                string categoryQuery = loadCategoryQueryForDefault(storeId, branchId);
                string subCategoryQuery = loadSubCategoryQueryForDefault(storeId, branchId);

                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlCategoryCommand = new SqlCommand(categoryQuery, sqlconn);
                SqlCommand sqlSubCategoryCommand = new SqlCommand(subCategoryQuery, sqlconn);
                sqlconn.Open();
                categoryResult = sqlCategoryCommand.ExecuteNonQuery();
                subCategoryResult = sqlSubCategoryCommand.ExecuteNonQuery();
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            return categoryResult + subCategoryResult;
        }

        private string loadCategoryQueryForDefault(int storeId, int branchId)
        {
            string query = @"MERGE INTO Category AS target
                USING (SELECT DISTINCT Category, BranchId, StoreId,PictureName1 FROM temp_Product) AS source
                ON (target.Name = source.Category AND target.BranchId = source.BranchId AND target.StoreId = source.StoreId)
                WHEN NOT MATCHED BY TARGET THEN
                INSERT (Name,CreatedOnUtc,PermaLink,StoreId,BranchId,IsDeleted,Published,FlagShowBuy,CategoryImage,FlagSampleCategory) 
                Values (source.Category,'{cur_date}',REPLACE(LTRIM(RTRIM(LOWER(source.Category))), ' ', ''),'{storeId}',
               '{branchId}','False','True','True', source.PictureName1,'True');".FormatWith(new { cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"),storeId,branchId });
            return query;
        }

        private  string loadSubCategoryQueryForDefault(int storeId, int branchId)
        {
            string query = @"With CategoryCTE AS 
                (
                Select distinct CategoryId, Category.Name from Category
                Inner Join temp_Product
                ON (Category.Name = temp_Product.Category and Category.BranchId = temp_Product.BranchId and Category.StoreId = temp_Product.StoreId)
                )
                ,
                SubCategoryTempCTE AS 
                (
               Select Distinct CategoryId,SubCategory, temp_Product.StoreId, temp_Product.BranchId, PictureName2,ShowOnHomePage from temp_Product
               Inner Join CategoryCTE
               ON CategoryCTE.Name = temp_Product.Category
                )
               MERGE INTO Category AS target
               USING (SELECT DISTINCT SubCategory,CategoryId,StoreId, BranchId, PictureName2,ShowOnHomePage FROM SubCategoryTempCTE) AS source
               ON (target.storeid = source.storeid and target.branchid = source.branchid and target.Name = source.SubCategory)
               WHEN NOT MATCHED BY TARGET THEN
               INSERT (Name, ParentCategoryId,CreatedOnUtc,PermaLink,StoreId,BranchId,IsDeleted,Published,FlagShowBuy,CategoryImage,ShowOnHomePage,FlagSampleCategory)
               Values (source.SubCategory,CategoryId,'{cur_date}',REPLACE(LTRIM(RTRIM(LOWER(source.SubCategory))), ' ', ''),'{storeId}',
                '{branchId}','False','True','True',source.PictureName2,source.ShowOnHomePage,'True');".FormatWith(new { cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"), storeId, branchId });
            return query;  
        } 
        public int LoadProductsAndMappingForDefault(int manufactureId)   
        {
            int productsCount = 0;
            try
            {
                var loadProductsTableQueryForDefaultQuery = loadProductsTableQueryForDefault(manufactureId);
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadProductsTableQueryForDefaultQuery, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            //hack for now as we divide by 2.
            return productsCount / 2;

        }
        private string loadProductsTableQueryForDefault(int manufactureId) 
        {
            string query = @"declare @T table
                (
                  Name nvarchar(400),
                  MetaTitle nvarchar(400)
                );

                MERGE INTO Product AS target
                USING (SELECT DISTINCT temp_Product.Name, temp_Product.ShortDescription, temp_Product.FullDescription, 
				    temp_Product.MetaTitle, temp_Product.MetaKeywords, temp_Product.MetaDescription,
				    Gtin, IsGiftCard, Weight, Length, Width, Height, temp_Product.Color, temp_Product.DisplayOrder, temp_Product.Published
                     , SubCategory.CategoryId, temp_Product.Size1,temp_Product.Size2, temp_Product.Size3, temp_Product.Size4
                       , temp_Product.Size5, temp_Product.Size6,temp_Product.StoreId,temp_Product.BranchId,temp_Product.PermaLink,temp_Product.ProductShowOnHomePage
				        FROM temp_Product
                        INNER JOIN Category SubCategory
					            ON (temp_Product.SubCategory = SubCategory.Name AND temp_Product.BranchId = SubCategory.BranchId AND temp_Product.StoreId = SubCategory.StoreId)
					    INNER JOIN Category cat
					         ON cat.CategoryId = SubCategory.ParentCategoryId 
                    ) AS source
                    ON (target.Name = source.Name AND target.Category = source.CategoryId AND target.BranchId = source.BranchId AND target.StoreId = source.StoreId)
                WHEN NOT MATCHED BY TARGET THEN
                INSERT (
				    ProductTypeId, Name, ShortDescription, FullDescription, 
				    MetaTitle,MetaKeywords, MetaDescription,
				    Gtin, IsGiftCard, Weight, Length, Width, Height, Color, DisplayOrder, Published
				    , Category, CreatedOnUtc, UpdatedOnUtc, 
                    Size1, Size2, Size3,Size4,Size5,Size6,PermaLink,StoreId,BranchId,IsDeleted,ShowOnHomePage,Manufacturer,FlagSampleProducts) 
				    Values
                    (1, source.Name, source.ShortDescription, ISNULL(source.FullDescription, source.Name),
				    source.Name, source.Name, ISNULL(source.ShortDescription, source.Name),
				    source.Gtin, 'False', source.Weight, source.Length, 
				    source.Width, source.Height, source.Color, 0, 'True'
                    ,source.CategoryId
				,'{cur_date}','{cur_date}'
                , Size1, Size2, Size3,Size4,Size5,Size6,REPLACE(LTRIM(RTRIM(LOWER(source.PermaLink))), ' ', ''), source.StoreId, 
                source.BranchId,'False',source.ProductShowOnHomePage,{manufactureId},'True')
                OUTPUT Inserted.Name, Inserted.MetaTitle INTO @T;


                UPDATE temp_Product
                SET Published = 'True'
                from @T as T
                where temp_Product.Name = T.Name COLLATE SQL_Latin1_General_CP1_CI_AS
                ".FormatWith(new 
                        { cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"),
                          manufactureId
                        });//todo:may be later we can consider category, manufaturer.

            return query;
        } 

        public int LoadPricingForDefault()  
        {
            int productsCount = 0;
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadPricingTableQueryForDefault, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                var s = ex.Message;
            }

            return productsCount;
        }

        private string loadPricingTableQueryForDefault = @"
                DECLARE @T TABLE(ProductId INT, StoreId INT, BranchId INT);
                 MERGE INTO Pricing AS target
                USING (SELECT DISTINCT  ProductId, temp_pricing.StoreId,	temp_pricing.BranchId,	
						temp_pricing.BranchName,	Price,	SpecialPrice,	AdditionalShippingCharge,	AdditionalTax
						,Name
				        FROM temp_Pricing
                       INNER JOIN Product
					ON (Product.Name =  temp_pricing.ProductName COLLATE SQL_Latin1_General_CP1_CI_AS And Product.StoreId =  temp_pricing.StoreId AND Product.BranchId =  temp_pricing.BranchId)
					INNER JOIN SellerBranch 
					ON SellerBranch.BranchName = temp_pricing.BranchName COLLATE SQL_Latin1_General_CP1_CI_AS
					AND SellerBranch.BranchId = temp_pricing.BranchId
                    ) AS source
                    ON (target.Product = source.ProductId
				    AND target.Store = source.StoreId
                    AND target.Branch = source.BranchId
					)
                WHEN NOT MATCHED BY TARGET THEN
				INSERT (Product, Store , Branch, Price, SpecialPrice,AdditionalShippingCharge,  AdditionalTax, CallForPrice, OldPrice, ProductCost,IsDeleted
				   ) 
				    Values
                    ( source.ProductId, source.StoreId, source.BranchId, source.Price, source.SpecialPrice, source.AdditionalShippingCharge
					, source.AdditionalTax, 0, '0.0000', '0.0000','False')
                WHEN MATCHED THEN
				UPDATE SET target.OldPrice = target.Price, target.Price = source.Price, target.SpecialPrice = source.SpecialPrice,
				target.AdditionalShippingCharge = source.AdditionalShippingCharge,target.AdditionalTax = source.AdditionalTax,
				target.CallForPrice = 0
                OUTPUT source.ProductId,source.StoreId, source.BranchId INTO @T;
                Delete tmpPricing FROM temp_Pricing tmpPricing INNER JOIN @T tempResult 
			                ON tempResult.StoreId = tmpPricing.StoreId
			                AND tempResult.BranchId = tmpPricing.BranchId
			                INNER JOIN Product ON Product.Name = tmpPricing.ProductName
			                AND tempResult.ProductId = Product.ProductId
			                INNER JOIN Pricing ON Product.ProductId = Pricing.Product
			                AND Pricing.SpecialPrice = tmpPricing.SpecialPrice
			                AND Pricing.Price = tmpPricing.Price
			                AND Pricing.Store = tmpPricing.StoreId
			                AND Pricing.Branch = tmpPricing.BranchId";

        public int LoadProductImageForDefault()
        {
            int productsCount = 0;
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadProductImageTableQueryForDefault, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                var s = ex.Message;
            }

            return productsCount;
        }

        private string loadProductImageTableQueryForDefault = @"    
                DECLARE @T TABLE(ProductId INT);
                 MERGE INTO ProductImage AS target
                USING (SELECT DISTINCT  ProductId, temp_Product.PictureName
				        FROM temp_Product
                       INNER JOIN Product
						ON Product.Name =  temp_Product.Name COLLATE SQL_Latin1_General_CP1_CI_AS
                    ) AS source
                    ON (target.ProductId = source.ProductId
					)
                WHEN NOT MATCHED BY TARGET THEN
				INSERT (ProductId, PictureName , CreatedDate) 
				    Values
                    (source.ProductId, source.PictureName, '{cur_date}');".FormatWith(new { cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss") });

        public int LoadManufacturerDefault(string BranchName, int storeId, int branchId)
        {
            int result = 0;
            try
            {
                var manufacture = @"select ManufacturerId from Manufacturer where Name = '{branchName}'".FormatWith(new
                {
                    branchName = BranchName
                });

                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlcmd = new SqlCommand(manufacture, sqlconn);
                sqlconn.Open();
                var manufactId = sqlcmd.ExecuteScalar();
                sqlconn.Close();
                if (manufactId == null)
                {
                    var manufactureQuery = @"INSERT INTO Manufacturer (Name, MetaTitle, Deleted, DisplayOrder,CreatedOnUtc,StoreId,BranchId) 
				    Values
                    ('{branchName}','{branchName}', 'False', 0, '{cur_date}',{storeId},{branchId});".FormatWith(new
                    {
                        cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"),
                        branchName = BranchName,
                        storeId,
                        branchId
                    });
                    SqlConnection sqlconn1 = new SqlConnection(sqlConnectionString);
                    SqlCommand sqlcmd1 = new SqlCommand(manufactureQuery, sqlconn1);
                    sqlconn1.Open();
                    result = sqlcmd1.ExecuteNonQuery();
                    sqlconn1.Close();
                }
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            return result;
        }
        public int GetManufacturerIdDefault(string BranchName)
        {
            int result = 0;
            var manufactureQuery = @"select ManufacturerId from Manufacturer where Name = '{branchName}'".FormatWith(new
            {
                branchName = BranchName
            });
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlcmd = new SqlCommand(manufactureQuery, sqlconn);
                sqlconn.Open();
                var manufactId = sqlcmd.ExecuteScalar();
                result = (int)manufactId;
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            return result;
        }

        // For default data v2

        public int LoadCategoryForDefaultV2(int storeId, int branchId)
        {
            int categoryResult = 0;
            int subCategoryResult = 0;
            try
            {
                string categoryQuery = loadCategoryQueryForDefaultV2(storeId, branchId);
                string subCategoryQuery = loadSubCategoryQueryForDefaultV2(storeId, branchId);

                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlCategoryCommand = new SqlCommand(categoryQuery, sqlconn);
                SqlCommand sqlSubCategoryCommand = new SqlCommand(subCategoryQuery, sqlconn);
                sqlconn.Open();
                categoryResult = sqlCategoryCommand.ExecuteNonQuery();
                subCategoryResult = sqlSubCategoryCommand.ExecuteNonQuery();
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            return categoryResult + subCategoryResult;
        }

        private string loadCategoryQueryForDefaultV2(int storeId, int branchId)
        {
            string query = @"MERGE INTO Category AS target
                USING (SELECT DISTINCT Category, BranchId, StoreId,PictureName1 FROM temp_Product) AS source
                ON (target.Name = source.Category AND target.BranchId = source.BranchId AND target.StoreId = source.StoreId)
                WHEN NOT MATCHED BY TARGET THEN
                INSERT (Name,CreatedOnUtc,PermaLink,StoreId,BranchId,IsDeleted,Published,FlagShowBuy,CategoryImage,FlagSampleCategory) 
                Values (source.Category,'{cur_date}',REPLACE(LTRIM(RTRIM(LOWER(source.Category))), ' ', ''),
                '{storeId}','{branchId}','False','True','True', source.PictureName1,'True');".FormatWith(new { cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"), storeId, branchId });
            return query;
        }

        private string loadSubCategoryQueryForDefaultV2(int storeId, int branchId)
        {
            string query = @"With CategoryCTE AS 
                (
                Select distinct CategoryId, Category.Name from Category
                Inner Join temp_Product
                ON (Category.Name = temp_Product.Category and Category.BranchId = temp_Product.BranchId and Category.StoreId = temp_Product.StoreId)
                )
                ,
                SubCategoryTempCTE AS 
                (
               Select Distinct CategoryId,SubCategory, temp_Product.StoreId, temp_Product.BranchId, PictureName2,ShowOnHomePage from temp_Product
               Inner Join CategoryCTE
               ON CategoryCTE.Name = temp_Product.Category
                )
               MERGE INTO Category AS target
               USING (SELECT DISTINCT SubCategory,CategoryId,StoreId, BranchId, PictureName2,ShowOnHomePage FROM SubCategoryTempCTE) AS source
               ON (target.storeid = source.storeid and target.branchid = source.branchid and target.Name = source.SubCategory)
               WHEN NOT MATCHED BY TARGET THEN
               INSERT (Name, ParentCategoryId,CreatedOnUtc,PermaLink,StoreId,BranchId,IsDeleted,Published,FlagShowBuy,CategoryImage,ShowOnHomePage,FlagSampleCategory) 
               Values (source.SubCategory,CategoryId,'{cur_date}',REPLACE(LTRIM(RTRIM(LOWER(source.SubCategory))), ' ', ''),
               '{storeId}','{branchId}','False','True','True',source.PictureName2,source.ShowOnHomePage,'True');".FormatWith(new { cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"), storeId, branchId });
            return query;
        }
        public int LoadProductsAndMappingForDefaultV2(int manufactureId)
        {
            int productsCount = 0;
            try
            {
                var loadProductsTableQueryForDefaultQuery = loadProductsTableQueryForDefaultV2(manufactureId);
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadProductsTableQueryForDefaultQuery, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            //hack for now as we divide by 2.
            return productsCount / 2;

        }
        private string loadProductsTableQueryForDefaultV2(int manufactureId)
        {
            string query = @"declare @T table
                (
                  Name nvarchar(400)
                );

                 MERGE INTO ProductStoreMapping AS target
                USING (SELECT DISTINCT  ProductId,Product.FullDescription,Product.PermaLink,SubCategory.CategoryId, temp_Product.StoreId, temp_Product.BranchId, temp_Product.ProductShowOnHomePage ,temp_Product.Name
				        FROM temp_Product
                       INNER JOIN Product
					ON (Product.Name =  temp_Product.Name COLLATE SQL_Latin1_General_CP1_CI_AS AND Product.FullDescription =  temp_Product.FullDescription COLLATE SQL_Latin1_General_CP1_CI_AS AND Product.FlagSampleProducts = 'True')
                        INNER JOIN Category SubCategory
					            ON (temp_Product.SubCategory = SubCategory.Name AND temp_Product.BranchId = SubCategory.BranchId AND temp_Product.StoreId = SubCategory.StoreId)
					    INNER JOIN Category cat
					         ON cat.CategoryId = SubCategory.ParentCategoryId 
                    ) AS source
                    ON (target.ProductId = source.ProductId and target.StoreId = source.StoreId and target.BranchId = source.BranchId)
                WHEN NOT MATCHED BY TARGET THEN
				INSERT (ProductId, StoreId , BranchId, CreatedOnUtc,IsDeleted,Published,ShowOnHomePage,Manufacturer,Category,Name,FullDescription,PermaLink,FlagSampleProducts) 
				    Values
                    ( source.ProductId, source.StoreId, source.BranchId, '{cur_date}', 'False', 'True', source.ProductShowOnHomePage,{manufactureId},source.CategoryId,
                source.Name,source.FullDescription,source.PermaLink,'True')
                OUTPUT source.Name INTO @T;
                UPDATE temp_Product
                SET Published = 'True'
                from @T as T
                where temp_Product.Name = T.Name COLLATE SQL_Latin1_General_CP1_CI_AS
                ".FormatWith(new
            {
                cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"),
                manufactureId
            });

            return query;
        }
        public int LoadPricingForDefaultV2()
        {
            int productsCount = 0;
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadPricingTableQueryForDefaultV2, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                var s = ex.Message;
            }

            return productsCount;
        }

        private string loadPricingTableQueryForDefaultV2 = @"
                DECLARE @T TABLE(ProductId INT, StoreId INT, BranchId INT);
                 MERGE INTO Pricing AS target
                USING (SELECT DISTINCT  ProductId, temp_pricing.StoreId,	temp_pricing.BranchId,	
						temp_pricing.BranchName,	Price,	SpecialPrice,	AdditionalShippingCharge,	AdditionalTax
						,Name,Product.FullDescription
				        FROM temp_Pricing
                       INNER JOIN Product
					ON (Product.Name =  temp_pricing.ProductName COLLATE SQL_Latin1_General_CP1_CI_AS AND Product.FullDescription =  temp_pricing.FullDescription COLLATE SQL_Latin1_General_CP1_CI_AS AND Product.FlagSampleProducts = 'True')
					INNER JOIN SellerBranch 
					ON SellerBranch.BranchName = temp_pricing.BranchName COLLATE SQL_Latin1_General_CP1_CI_AS
					AND SellerBranch.BranchId = temp_pricing.BranchId
                    ) AS source
                    ON (target.Product = source.ProductId and target.store = source.StoreId and target.Branch = source.BranchId)
                WHEN NOT MATCHED BY TARGET THEN
				INSERT (Product, Store , Branch, Price, SpecialPrice,AdditionalShippingCharge,  AdditionalTax, CallForPrice, OldPrice, ProductCost,IsDeleted
				   ) 
				    Values
                    ( source.ProductId, source.StoreId, source.BranchId, source.Price, source.SpecialPrice, source.AdditionalShippingCharge
					, source.AdditionalTax, 0, '0.0000', '0.0000','False')
                WHEN MATCHED THEN
				UPDATE SET target.OldPrice = target.Price, target.Price = source.Price, target.SpecialPrice = source.SpecialPrice,
				target.AdditionalShippingCharge = source.AdditionalShippingCharge,target.AdditionalTax = source.AdditionalTax,
				target.CallForPrice = 0
                OUTPUT source.ProductId,source.StoreId, source.BranchId INTO @T;
                Delete tmpPricing FROM temp_Pricing tmpPricing INNER JOIN @T tempResult 
			                ON tempResult.StoreId = tmpPricing.StoreId
			                AND tempResult.BranchId = tmpPricing.BranchId
			                INNER JOIN Product ON Product.Name = tmpPricing.ProductName
			                AND tempResult.ProductId = Product.ProductId
			                INNER JOIN Pricing ON Product.ProductId = Pricing.Product
			                AND Pricing.SpecialPrice = tmpPricing.SpecialPrice
			                AND Pricing.Price = tmpPricing.Price
			                AND Pricing.Store = tmpPricing.StoreId
			                AND Pricing.Branch = tmpPricing.BranchId";

        public int LoadProductsAndMappingForDefaultV3()
        {
            int productsCount = 0;
            try
            {
                var loadProductsTableQueryForDefaultQuery = loadProductsTableQueryForDefaultV3();
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadProductsTableQueryForDefaultQuery, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            //hack for now as we divide by 2.
            return productsCount / 2;

        }
        private string loadProductsTableQueryForDefaultV3()
        {
            string query = @"declare @T table
                (
                  Name nvarchar(400),
                  MetaTitle nvarchar(400)
                );

                MERGE INTO Product AS target
                USING (SELECT DISTINCT temp_Product.Name, temp_Product.ShortDescription, temp_Product.FullDescription, 
				    temp_Product.MetaTitle, temp_Product.MetaKeywords, temp_Product.MetaDescription,
				    Gtin, IsGiftCard, Weight, Length, Width, Height, temp_Product.Color, temp_Product.DisplayOrder, temp_Product.Published
                     , SubCategory.CategoryId, temp_Product.Size1,temp_Product.Size2, temp_Product.Size3, temp_Product.Size4
                       , temp_Product.Size5, temp_Product.Size6,temp_Product.StoreId,temp_Product.BranchId,temp_Product.PermaLink,temp_Product.ProductShowOnHomePage
				        FROM temp_Product
                        INNER JOIN Category SubCategory
					            ON (temp_Product.SubCategory = SubCategory.Name AND temp_Product.BranchId = SubCategory.BranchId AND temp_Product.StoreId = SubCategory.StoreId)
					    INNER JOIN Category cat
					         ON cat.CategoryId = SubCategory.ParentCategoryId 
                    ) AS source
                    ON (target.Name = source.Name AND target.Category = source.CategoryId)
                WHEN NOT MATCHED BY TARGET THEN
                INSERT (
				    ProductTypeId, Name, ShortDescription, FullDescription, 
				    MetaTitle,MetaKeywords, MetaDescription,
				    Gtin, IsGiftCard, Weight, Length, Width, Height, Color, DisplayOrder, Published
				    , Category, CreatedOnUtc, UpdatedOnUtc, 
                    Size1, Size2, Size3,Size4,Size5,Size6,PermaLink,IsDeleted,ShowOnHomePage) 
                    Values
                    (1, source.Name, source.ShortDescription, ISNULL(source.FullDescription, source.Name),
				    source.Name, source.Name, ISNULL(source.ShortDescription, source.Name),
				    source.Gtin, 'False', source.Weight, source.Length, 
				    source.Width, source.Height, source.Color, 0, 'True'
                    ,source.CategoryId
				,'{cur_date}','{cur_date}'
                , Size1, Size2, Size3,Size4,Size5,Size6,REPLACE(LTRIM(RTRIM(LOWER(source.PermaLink))), ' ', '')
                ,'False',source.ProductShowOnHomePage)
                OUTPUT Inserted.Name, Inserted.MetaTitle INTO @T;


                UPDATE temp_Product
                SET Published = 'True'
                from @T as T
                where temp_Product.Name = T.Name COLLATE SQL_Latin1_General_CP1_CI_AS
                ".FormatWith(new
            {
                cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"),
            });

            return query;
        }
        public int LoadProductsAndMappingForDefaultV3(int manufactureId)
        {
            int productsCount = 0;
            try
            {
                var loadProductsTableQueryForDefaultQuery = loadProductsTableQueryForDefaultV3(manufactureId);
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadProductsTableQueryForDefaultQuery, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                logger.Error("-------------------ERROR LOG -----------------");
                logger.Error(ex.Message + ex.InnerException);
                logger.Error("-----------------END OF ERROR LOG-----------------");
            }
            //hack for now as we divide by 2.
            return productsCount / 2;

        }
        private string loadProductsTableQueryForDefaultV3(int manufactureId)
        {
            string query = @"declare @T table
                (
                  Name nvarchar(400)
                );

                 MERGE INTO ProductStoreMapping AS target
                USING (SELECT DISTINCT  ProductId,Product.FullDescription,Product.PermaLink,SubCategory.CategoryId, temp_Product.StoreId, temp_Product.BranchId, temp_Product.ProductShowOnHomePage ,temp_Product.Name
				        FROM temp_Product
                       INNER JOIN Product
					ON (Product.Name =  temp_Product.Name COLLATE SQL_Latin1_General_CP1_CI_AS 
                    And (Product.FlagSampleProducts = 'False'))
                        INNER JOIN Category SubCategory
					            ON (temp_Product.SubCategory = SubCategory.Name AND temp_Product.BranchId = SubCategory.BranchId AND temp_Product.StoreId = SubCategory.StoreId)
					    INNER JOIN Category cat
					         ON cat.CategoryId = SubCategory.ParentCategoryId 
                    ) AS source
                    ON (target.ProductId = source.ProductId and target.StoreId = source.StoreId and target.BranchId = source.BranchId)
                WHEN NOT MATCHED BY TARGET THEN
				INSERT (ProductId, StoreId , BranchId, CreatedOnUtc,IsDeleted,Published,ShowOnHomePage,Manufacturer,Category,Name,FullDescription,PermaLink) 
				    Values
                    ( source.ProductId, source.StoreId, source.BranchId, '{cur_date}', 'False', 'True', source.ProductShowOnHomePage,{manufactureId},source.CategoryId,
                source.Name,source.FullDescription,source.PermaLink)
                OUTPUT source.Name INTO @T;
                UPDATE temp_Product
                SET Published = 'True'
                from @T as T
                where temp_Product.Name = T.Name COLLATE SQL_Latin1_General_CP1_CI_AS
                ".FormatWith(new
            {
                cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss"),
                manufactureId
            });

            return query;
        }
        public int LoadPricingForDefaultV3()
        {
            int productsCount = 0;
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadPricingTableQueryForDefaultV3, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                var s = ex.Message;
            }

            return productsCount;
        }

        private string loadPricingTableQueryForDefaultV3 = @"
                DECLARE @T TABLE(ProductId INT, StoreId INT, BranchId INT);
                 MERGE INTO Pricing AS target
                USING (SELECT DISTINCT  ProductId, temp_pricing.StoreId,	temp_pricing.BranchId,	
						temp_pricing.BranchName,	Price,	SpecialPrice,	AdditionalShippingCharge,	AdditionalTax
						,Name,Product.FullDescription
				        FROM temp_Pricing
                       INNER JOIN Product
					ON (Product.Name =  temp_pricing.ProductName COLLATE SQL_Latin1_General_CP1_CI_AS 
                    And (Product.FlagSampleProducts = 'False'))
					INNER JOIN SellerBranch 
					ON SellerBranch.BranchName = temp_pricing.BranchName COLLATE SQL_Latin1_General_CP1_CI_AS
					AND SellerBranch.BranchId = temp_pricing.BranchId
                    ) AS source
                    ON (target.Product = source.ProductId and target.store = source.StoreId and target.Branch = source.BranchId)
                WHEN NOT MATCHED BY TARGET THEN
				INSERT (Product, Store , Branch, Price, SpecialPrice,AdditionalShippingCharge,  AdditionalTax, CallForPrice, OldPrice, ProductCost,IsDeleted
				   ) 
				    Values
                    ( source.ProductId, source.StoreId, source.BranchId, source.Price, source.SpecialPrice, source.AdditionalShippingCharge
					, source.AdditionalTax, 0, '0.0000', '0.0000','False')
                WHEN MATCHED THEN
				UPDATE SET target.OldPrice = target.Price, target.Price = source.Price, target.SpecialPrice = source.SpecialPrice,
				target.AdditionalShippingCharge = source.AdditionalShippingCharge,target.AdditionalTax = source.AdditionalTax,
				target.CallForPrice = 0
                OUTPUT source.ProductId,source.StoreId, source.BranchId INTO @T;
                Delete tmpPricing FROM temp_Pricing tmpPricing INNER JOIN @T tempResult 
			                ON tempResult.StoreId = tmpPricing.StoreId
			                AND tempResult.BranchId = tmpPricing.BranchId
			                INNER JOIN Product ON Product.Name = tmpPricing.ProductName
			                AND tempResult.ProductId = Product.ProductId
			                INNER JOIN Pricing ON Product.ProductId = Pricing.Product
			                AND Pricing.SpecialPrice = tmpPricing.SpecialPrice
			                AND Pricing.Price = tmpPricing.Price
			                AND Pricing.Store = tmpPricing.StoreId
			                AND Pricing.Branch = tmpPricing.BranchId";

        public int LoadProductImageForDefaultV3()
        {
            int productsCount = 0;
            try
            {
                SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                SqlCommand sqlProductCommand = new SqlCommand(loadProductImageTableQueryForDefaultV3, sqlconn);
                sqlconn.Open();
                productsCount = sqlProductCommand.ExecuteNonQuery();
                //update publish flag 
                sqlconn.Close();
            }
            catch (Exception ex)
            {
                var s = ex.Message;
            }

            return productsCount;
        }

        private string loadProductImageTableQueryForDefaultV3 = @"    
                DECLARE @T TABLE(ProductId INT);
                 MERGE INTO ProductImage AS target
                USING (SELECT DISTINCT  ProductId, temp_Product.PictureName
				        FROM temp_Product
                       INNER JOIN Product
						ON Product.Name =  temp_Product.Name COLLATE SQL_Latin1_General_CP1_CI_AS And (Product.FlagSampleProducts is null)
                    ) AS source
                    ON (target.ProductId = source.ProductId
					)
                WHEN NOT MATCHED BY TARGET THEN
				INSERT (ProductId, PictureName , CreatedDate) 
				    Values
                    (source.ProductId, source.PictureName, '{cur_date}');".FormatWith(new { cur_date = DateTime.UtcNow.ToString("MM/dd/yyyy HH:mm:ss") });


    }
}
