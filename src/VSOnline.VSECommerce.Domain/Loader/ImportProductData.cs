using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSOnline.VSECommerce.Domain.Loader
{
    public class ImportProductData
    {
        private readonly LoadProductData _loadProductData;
        IConfigurationRoot _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
        public string sqlConnectionString;
        public ImportProductData(LoadProductData loadProductData)
        {
            _loadProductData = loadProductData;
            sqlConnectionString = _configuration.GetConnectionString("DataContext");

        }
        string tempProductTable = "temp_Product";
        string tempPricingTable = "temp_Pricing";
        string tempKeyFeatureTable = "temp_ProductKeyFeatures";
        string tempSpecificationProductTable = "temp_ProductSpecification";
        string tempFilterProductTable = "temp_ProductFilterValueWithProductName";
        public string ImportDataTableToProduct(DataTable dt, string branchName, int storeId, int branchId)
        {
            string message = "";
            try
            {

                bool temploadStatus = LoadTempProductAndPricing(dt, branchName, storeId, branchId);
                bool tempFeaturesUploaded = LoadTempKeyFeaturesAndDetailedSpec(dt);
                string loadProductStatus = LoadProductTableFromTemp(temploadStatus, storeId, branchId);

                message = message + loadProductStatus;

                int loadPricingStatus = _loadProductData.LoadPricing();
                message = message + "Updated Pricing for" + loadPricingStatus.ToString();

                int loadKeyFeatureStatus = _loadProductData.LoadKeyFeatures();
                message = message + "Updated KeyFeatures for" + loadKeyFeatureStatus.ToString();

                int loadDetailedSpecStatus = _loadProductData.LoadDetailedSpecification();
                message = message + "Updated KeyFeatures for" + loadDetailedSpecStatus.ToString();

                //Load filters.
                int masterFilter = _loadProductData.LoadCategoryMaster();
                _loadProductData.LoadProductFilterValueTableFromtemp_ProductFilterValueWithProductName();
                int loadProductFilterStatus = _loadProductData.LoadProductFilterValue();
                message = message + "Updated KeyFeatures for" + loadProductFilterStatus.ToString();

                DeleteTempProduct(storeId, branchId);
                DeleteTempPricing(storeId, branchId);
            }
            catch (Exception ex)
            {
                return message + ex.Message;
            }
            return message;
        }
        private bool LoadTempProductAndPricing(DataTable dtProduct, string branchName, int storeId, int branchId)
        {
            DataTable resultProductDataTable = new DataTable();
            DataTable resultPricingDataTable = new DataTable();

            var flagData = GetProductDataTable(dtProduct, resultProductDataTable, resultPricingDataTable, branchName, storeId, branchId);
            try
            {
                if (flagData)
                {
                    DeleteTempProduct(storeId, branchId);
                    DeleteTempPricing(storeId, branchId);
                    //create our connection strings
                    string sclearsql = "delete from " + tempProductTable + " WHERE Published = 'True'";
                    SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                    SqlCommand sqlcmd = new SqlCommand(sclearsql, sqlconn);
                    try
                    {
                        sqlconn.Open();
                        sqlcmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        sqlconn.Close();
                    }

                    SqlBulkCopy bulkcopy = new SqlBulkCopy(sqlConnectionString);
                    bulkcopy.DestinationTableName = tempProductTable;

                    foreach (DataColumn column in resultProductDataTable.Columns)
                    {
                        bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                    }

                    bulkcopy.WriteToServer(resultProductDataTable);


                    bulkcopy.DestinationTableName = tempPricingTable;
                    bulkcopy.ColumnMappings.Clear();
                    foreach (DataColumn column in resultPricingDataTable.Columns)
                    {
                        bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                    }
                    bulkcopy.WriteToServer(resultPricingDataTable);
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        private bool GetProductDataTable(DataTable productDataTable, DataTable productResultDataTable, DataTable resultPricingDataTable, string branchName, int storeId, int branchId)
        {
            try
            {
                var productTableRows = productDataTable.Select();
                var tableColumnSize = productDataTable.Columns.Count;
                //ProductName Column 
                int indexProductName = productDataTable.Columns.IndexOf("ProductName");


                productResultDataTable.Columns.Add("PictureName");
                productResultDataTable.Columns.Add("PictureName1");
                productResultDataTable.Columns.Add("PictureName2");
                productResultDataTable.Columns.Add("Category");
                productResultDataTable.Columns.Add("SubCategory");
                productResultDataTable.Columns.Add("Name");
                productResultDataTable.Columns.Add("ShortDescription");
                productResultDataTable.Columns.Add("FullDescription");
                productResultDataTable.Columns.Add("Manufacturer");
                productResultDataTable.Columns.Add("MetaKeywords");
                productResultDataTable.Columns.Add("MetaDescription");
                productResultDataTable.Columns.Add("MetaTitle");
                productResultDataTable.Columns.Add("ManufacturerPartNumber");
                productResultDataTable.Columns.Add("Weight");
                productResultDataTable.Columns.Add("Length");
                productResultDataTable.Columns.Add("Width");
                productResultDataTable.Columns.Add("Height");
                productResultDataTable.Columns.Add("Color");
                productResultDataTable.Columns.Add("Size1");
                productResultDataTable.Columns.Add("Size2");
                productResultDataTable.Columns.Add("Size3");
                productResultDataTable.Columns.Add("Size4");
                productResultDataTable.Columns.Add("Size5");
                productResultDataTable.Columns.Add("Size6");
                productResultDataTable.Columns.Add("PermaLink");
                productResultDataTable.Columns.Add("StoreId");
                productResultDataTable.Columns.Add("BranchId");

                //Pricing 

                resultPricingDataTable.Columns.Add("ProductName");
                resultPricingDataTable.Columns.Add("StoreId");
                resultPricingDataTable.Columns.Add("BranchId");
                resultPricingDataTable.Columns.Add("BranchName");
                resultPricingDataTable.Columns.Add("Price");
                resultPricingDataTable.Columns.Add("SpecialPrice");
                resultPricingDataTable.Columns.Add("AdditionalShippingCharge");
                resultPricingDataTable.Columns.Add("AdditionalTax");


                foreach (var row in productTableRows)
                {
                    var productName = row[indexProductName].ToString();
                    if (!string.IsNullOrEmpty(productName))
                    {
                        DataRow drResult = productResultDataTable.NewRow();
                        DataRow drPricingResult = resultPricingDataTable.NewRow();
                        drResult["Name"] = productName;
                        drResult["StoreId"] = storeId;
                        drResult["BranchId"] = branchId;
                        MapProductColumns(drResult, row);


                        productResultDataTable.Rows.Add(drResult);

                        drPricingResult["ProductName"] = productName;
                        drPricingResult["StoreId"] = storeId;
                        drPricingResult["BranchName"] = branchName;
                        drPricingResult["BranchId"] = branchId;
                        MapPricingColumns(drPricingResult, row);
                        resultPricingDataTable.Rows.Add(drPricingResult);
                    }

                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private void MapPricingColumns(DataRow drPricingResult, DataRow row)
        {
            drPricingResult["Price"] = row["Price"];
            drPricingResult["SpecialPrice"] = row["SpecialPrice"];
            drPricingResult["AdditionalShippingCharge"] = row["AdditionalShippingCharge"];
            drPricingResult["AdditionalTax"] = row["AdditionalTax"];
        }

        private void MapProductColumns(DataRow drResult, DataRow row)
        {

            drResult["Category"] = row["Category"];
            drResult["SubCategory"] = row["SubCategory"];
            drResult["FullDescription"] = row["FullDescription"];
            drResult["Manufacturer"] = row["Manufacturer"];
            drResult["ManufacturerPartNumber"] = row["ManufacturerPartNumber"];
            drResult["Weight"] = row["Weight"];
            drResult["Length"] = row["Length"];
            drResult["Width"] = row["Width"];
            drResult["Height"] = row["Height"];
            drResult["Color"] = row["Color"];
        }


        private bool LoadTempKeyFeaturesAndDetailedSpec(DataTable dtProduct)
        {
            DataTable finalKeyFeatureDataTable = new DataTable();
            DataTable finalDetailedSpecDataTable = new DataTable();
            DataTable finalProductFilterDataTable = new DataTable();

            bool flagData = GetKeyFeatureDataTable(dtProduct, finalKeyFeatureDataTable, finalDetailedSpecDataTable, finalProductFilterDataTable);
            try
            {
                SqlBulkCopy bulkcopy = new SqlBulkCopy(sqlConnectionString);
                bulkcopy.DestinationTableName = tempKeyFeatureTable;
                bulkcopy.WriteToServer(finalKeyFeatureDataTable);

                bulkcopy.DestinationTableName = tempSpecificationProductTable;
                bulkcopy.WriteToServer(finalDetailedSpecDataTable);

                bulkcopy.DestinationTableName = tempFilterProductTable;
                bulkcopy.WriteToServer(finalProductFilterDataTable);

            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private bool GetKeyFeatureDataTable(DataTable productDataTable, DataTable finalKeyFeatureDataTable, DataTable finalDetailedSpecDataTable, DataTable finalProductFilterDataTable)
        {
            try
            {
                var rowsKeyFeatures = productDataTable.Select();
                var tableColumnSize = productDataTable.Columns.Count;
                //ProductName Column 
                int indexProductName = productDataTable.Columns.IndexOf("ProductName");


                finalKeyFeatureDataTable.Columns.Add("ProductName");
                finalKeyFeatureDataTable.Columns.Add("Parameter");
                finalKeyFeatureDataTable.Columns.Add("KeyFeature");

                finalDetailedSpecDataTable.Columns.Add("ProductName");
                finalDetailedSpecDataTable.Columns.Add("SpecificationGroup");
                finalDetailedSpecDataTable.Columns.Add("SpecificationAttribute");
                finalDetailedSpecDataTable.Columns.Add("SpecificationDetails");


                finalProductFilterDataTable.Columns.Add("ProductName");
                finalProductFilterDataTable.Columns.Add("FilterParameter");
                finalProductFilterDataTable.Columns.Add("FilterValue");
                finalProductFilterDataTable.Columns.Add("FilterValueText");


                foreach (var keyfeatureRow in rowsKeyFeatures)
                {
                    for (int i = 0; i < tableColumnSize; i++)
                    {
                        string columnName = LoaderHelper.GetColumn(keyfeatureRow, i);

                        if (columnName.StartsWith("Keyfeature-"))
                        {
                            string[] splitColumnName = columnName.Split('-');
                            var parameterName = splitColumnName[splitColumnName.Length - 1];

                            var productName = keyfeatureRow[indexProductName].ToString();
                            var parameter = parameterName;
                            var value = keyfeatureRow[i].ToString();
                            DataRow dr = finalKeyFeatureDataTable.NewRow();
                            dr["ProductName"] = productName;
                            dr["Parameter"] = parameter;
                            dr["KeyFeature"] = value;
                            finalKeyFeatureDataTable.Rows.Add(dr);

                            if (columnName.Contains("Filter-"))
                            {
                                DataRow drFilter = finalProductFilterDataTable.NewRow();
                                drFilter["ProductName"] = productName;
                                drFilter["FilterParameter"] = parameter;
                                drFilter["FilterValue"] = value;
                                drFilter["FilterValueText"] = value;
                                finalProductFilterDataTable.Rows.Add(drFilter);
                            }
                        }

                        if (columnName.StartsWith("Specification"))
                        {
                            string[] splitSpecificationColumnName = columnName.Split('-');
                            var specificationAttribute = splitSpecificationColumnName[splitSpecificationColumnName.Length - 1];
                            var productName = keyfeatureRow[indexProductName].ToString();
                            var specificationGroup = "General";
                            if (splitSpecificationColumnName.Length > 2)
                            {
                                specificationGroup = splitSpecificationColumnName[1];
                            }
                            DataRow detailedSpecRow = finalDetailedSpecDataTable.NewRow();

                            detailedSpecRow["ProductName"] = productName;
                            detailedSpecRow["SpecificationGroup"] = specificationGroup;
                            detailedSpecRow["SpecificationAttribute"] = specificationAttribute;
                            detailedSpecRow["SpecificationDetails"] = keyfeatureRow[i].ToString();

                            finalDetailedSpecDataTable.Rows.Add(detailedSpecRow);

                        }

                    }
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        private string LoadProductTableFromTemp(bool temploadStatus, int storeId, int branchId)
        {
            string message = "";
            if (temploadStatus)
            {
                message = " \r\n " + " Excel Validated for product......";
                try
                {
                    int numNewCategory = _loadProductData.LoadCategory(storeId, branchId);
                    message = message + " \r\n " + " Successfully loaded " + numNewCategory + " number of category";

                    int numManufacturer = _loadProductData.LoadManufacturer();
                    message = message + " \r\n " + " Successfully loaded " + numManufacturer + " number of Brands";

                    int loadedProductCount = _loadProductData.LoadProductsAndMapping();
                    message = message + " \r\n " + "Successfully loaded " + loadedProductCount + " number of Products";
                }
                catch (Exception ex)
                {
                    message = message + " \r\n " + ex.Message;
                }

            }
            else
            {
                message = message + " \r\n " + " Excel is having issue. Please Check for Column is blank or any issue in excel data......";
                message = message + " \r\n " + "Error loading products";
                message = message + " \r\n " + "Error loading products...Excel Is having issue. Please Check for Column is blank or any issue in excel data";
            }
            message = message + " \r\n " + " Done with Updating Products... " + DateTime.Now.ToString();

            return message;
        }

        // For load default data
        public string ImportDataTableToDefaultData(DataTable dt, string branchName, int storeId, int branchId)
        {
            string message = "";
            try
            {

                bool temploadStatus = LoadTempProductAndPricingForDefault(dt, branchName, storeId, branchId);
                string loadProductStatus = LoadProductTableFromTempForDefault(temploadStatus, storeId, branchId, branchName);

                message = message + loadProductStatus;

                int loadPricingStatus = _loadProductData.LoadPricingForDefault();
                message = message + "Updated Pricing for" + loadPricingStatus.ToString();

                int loadProductImageStatus = _loadProductData.LoadProductImageForDefault();
                message = message + "Updated ProductImage for" + loadProductImageStatus.ToString();

                DeleteTempProduct(storeId,branchId);
                DeleteTempPricing(storeId, branchId);

            }
            catch (Exception ex)
            {
                return message + ex.Message;
            }
            return message;
        }
        private bool LoadTempProductAndPricingForDefault(DataTable dtProduct, string branchName, int storeId, int branchId)
        {
            DataTable resultProductDataTable = new DataTable();
            DataTable resultPricingDataTable = new DataTable();

            var flagData = GetProductDataTableForDefault(dtProduct, resultProductDataTable, resultPricingDataTable, branchName, storeId, branchId);
            try
            {
                if (flagData)
                {
                    DeleteTempProduct(storeId, branchId);
                    DeleteTempPricing(storeId, branchId);
                    //create our connection strings
                    string sclearsql = "delete from " + tempProductTable + " WHERE Published = 'True'";
                    SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                    SqlCommand sqlcmd = new SqlCommand(sclearsql, sqlconn);
                    try
                    {
                        sqlconn.Open();
                        sqlcmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        sqlconn.Close();
                    }

                    SqlBulkCopy bulkcopy = new SqlBulkCopy(sqlConnectionString);
                    bulkcopy.DestinationTableName = tempProductTable;

                    foreach (DataColumn column in resultProductDataTable.Columns)
                    {
                        bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                    }

                    bulkcopy.WriteToServer(resultProductDataTable);


                    bulkcopy.DestinationTableName = tempPricingTable;
                    bulkcopy.ColumnMappings.Clear();
                    foreach (DataColumn column in resultPricingDataTable.Columns)
                    {
                        bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                    }
                    bulkcopy.WriteToServer(resultPricingDataTable);
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
            return true;
        }
        private bool GetProductDataTableForDefault(DataTable productDataTable, DataTable productResultDataTable, DataTable resultPricingDataTable, string branchName, int storeId, int branchId)
        {
            try
            {
                var productTableRows = productDataTable.Select();
                var tableColumnSize = productDataTable.Columns.Count;
                //ProductName Column 
                int indexProductName = productDataTable.Columns.IndexOf("ProductName");
            
                productResultDataTable.Columns.Add("PictureName");
                productResultDataTable.Columns.Add("Category");
                productResultDataTable.Columns.Add("SubCategory");
                productResultDataTable.Columns.Add("Name");
                productResultDataTable.Columns.Add("ShortDescription");
                productResultDataTable.Columns.Add("FullDescription");
                productResultDataTable.Columns.Add("Manufacturer");
                productResultDataTable.Columns.Add("MetaKeywords");
                productResultDataTable.Columns.Add("MetaDescription");
                productResultDataTable.Columns.Add("MetaTitle");
                productResultDataTable.Columns.Add("ManufacturerPartNumber");
                productResultDataTable.Columns.Add("Weight");
                productResultDataTable.Columns.Add("Length");
                productResultDataTable.Columns.Add("Width");
                productResultDataTable.Columns.Add("Height");
                productResultDataTable.Columns.Add("Color");
                productResultDataTable.Columns.Add("Size1");
                productResultDataTable.Columns.Add("Size2");
                productResultDataTable.Columns.Add("Size3");
                productResultDataTable.Columns.Add("Size4");
                productResultDataTable.Columns.Add("Size5");
                productResultDataTable.Columns.Add("Size6");
                productResultDataTable.Columns.Add("PermaLink");
                productResultDataTable.Columns.Add("StoreId");
                productResultDataTable.Columns.Add("BranchId");
                productResultDataTable.Columns.Add("PictureName1");
                productResultDataTable.Columns.Add("PictureName2");
                productResultDataTable.Columns.Add("ProductShowOnHomePage");
                productResultDataTable.Columns.Add("ShowOnHomePage");

                //Pricing 

                resultPricingDataTable.Columns.Add("ProductName");
                resultPricingDataTable.Columns.Add("StoreId");
                resultPricingDataTable.Columns.Add("BranchId");
                resultPricingDataTable.Columns.Add("BranchName");
                resultPricingDataTable.Columns.Add("Price");
                resultPricingDataTable.Columns.Add("SpecialPrice");
                resultPricingDataTable.Columns.Add("AdditionalShippingCharge");
                resultPricingDataTable.Columns.Add("AdditionalTax");


                foreach (var row in productTableRows)
                {
                    var productName = row[indexProductName].ToString();
                    if (!string.IsNullOrEmpty(productName))
                    {
                        DataRow drResult = productResultDataTable.NewRow();
                        DataRow drPricingResult = resultPricingDataTable.NewRow();
                        drResult["Name"] = productName;
                        drResult["StoreId"] = storeId;
                        drResult["BranchId"] = branchId;
                        MapProductColumnsForDefault(drResult, row);


                        productResultDataTable.Rows.Add(drResult);

                        drPricingResult["ProductName"] = productName;
                        drPricingResult["StoreId"] = storeId;
                        drPricingResult["BranchName"] = branchName;
                        drPricingResult["BranchId"] = branchId;
                        MapPricingColumnsForDefault(drPricingResult, row);
                        resultPricingDataTable.Rows.Add(drPricingResult);
                    }

                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private void MapProductColumnsForDefault(DataRow drResult, DataRow row)
        {

            drResult["Category"] = row["Category"];
            drResult["SubCategory"] = row["SubCategory"];
            drResult["FullDescription"] = row["FullDescription"];
            drResult["PermaLink"] = row["PermaLink"];
            drResult["PictureName"] = row["ProductImages"];
            drResult["ShowOnHomePage"] = row["ShowOnHomePage"];
            drResult["ProductShowOnHomePage"] = row["ProductShowOnHomePage"];
            drResult["PictureName1"] = row["CategoryImage"];
            drResult["PictureName2"] = row["SubCategoryImage"];
        }
        private void MapPricingColumnsForDefault(DataRow drPricingResult, DataRow row)
        {
            drPricingResult["Price"] = row["Price"];
            drPricingResult["SpecialPrice"] = row["SpecialPrice"];
        }
        private string LoadProductTableFromTempForDefault(bool temploadStatus,int storeId, int branchId, string branchName)
        {
            string message = "";
            if (temploadStatus)
            {
                message = " \r\n " + " Excel Validated for product......";
                try
                {
                    int numNewCategory = _loadProductData.LoadCategoryForDefault(storeId, branchId);
                    message = message + " \r\n " + " Successfully loaded " + numNewCategory + " number of category";

                    int numNewManufacture = _loadProductData.LoadManufacturerDefault(branchName, storeId, branchId);
                    message = message + " \r\n " + " Successfully loaded " + numNewManufacture + " number of Manufacture";

                    int manufactureId = _loadProductData.GetManufacturerIdDefault(branchName);

                    int loadedProductCount = _loadProductData.LoadProductsAndMappingForDefault(manufactureId);

                    message = message + " \r\n " + "Successfully loaded " + loadedProductCount + " number of Products";
                }
                catch (Exception ex)
                {
                    message = message + " \r\n " + ex.Message;
                }
            }
            else
            {
                message = message + " \r\n " + " Excel is having issue. Please Check for Column is blank or any issue in excel data......";
                message = message + " \r\n " + "Error loading products";
                message = message + " \r\n " + "Error loading products...Excel Is having issue. Please Check for Column is blank or any issue in excel data";
            }
            message = message + " \r\n " + " Done with Updating Products... " + DateTime.Now.ToString();

            return message;
        }

        private void DeleteTempProduct(int storeId, int branchId)
        {
            int productsCount = 0;
            string sclearsql = "delete from " + tempProductTable + " WHERE StoreId = "+ storeId + " and BranchId = " + branchId;
            SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
            SqlCommand sqlcmd = new SqlCommand(sclearsql, sqlconn);
            try
            {
                sqlconn.Open();
                productsCount = sqlcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlconn.Close();
            }
        }
        private void DeleteTempPricing(int storeId, int branchId)
        {
            int productsCount = 0;
            string sclearsql = "delete from " + tempPricingTable + " WHERE StoreId = " + storeId + " and BranchId = " + branchId;
            SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
            SqlCommand sqlcmd = new SqlCommand(sclearsql, sqlconn);
            try
            {
                sqlconn.Open();
                productsCount = sqlcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlconn.Close();
            }
        }
        // for load default data v2
        public string ImportDataTableToDefaultDataV2(DataTable dt, string branchName, int storeId, int branchId)
        {
            string message = "";
            try
            {

                bool temploadStatus = LoadTempProductAndPricingForDefaultV2(dt, branchName, storeId, branchId);
                string loadProductStatus = LoadProductTableFromTempForDefaultV2(temploadStatus, storeId, branchId, branchName);

                message = message + loadProductStatus;

                DeleteTempProduct(storeId, branchId);
                DeleteTempPricing(storeId, branchId);
            }
            catch (Exception ex)
            {
                return message + ex.Message;
            }
            return message;
        }

        private bool LoadTempProductAndPricingForDefaultV2(DataTable dtProduct, string branchName, int storeId, int branchId)
        {
            DataTable resultProductDataTable = new DataTable();
            DataTable resultPricingDataTable = new DataTable();

            var flagData = GetProductDataTableForDefaultV2(dtProduct, resultProductDataTable, resultPricingDataTable, branchName, storeId, branchId);
            try
            {
                if (flagData)
                {
                    DeleteTempProduct(storeId, branchId);
                    DeleteTempPricing(storeId, branchId);
                    //create our connection strings
                    string sclearsql = "delete from " + tempProductTable + " WHERE Published = 'True'";
                    SqlConnection sqlconn = new SqlConnection(sqlConnectionString);
                    SqlCommand sqlcmd = new SqlCommand(sclearsql, sqlconn);
                    try
                    {
                        sqlconn.Open();
                        sqlcmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {

                    }
                    finally
                    {
                        sqlconn.Close();
                    }

                    SqlBulkCopy bulkcopy = new SqlBulkCopy(sqlConnectionString);
                    bulkcopy.DestinationTableName = tempProductTable;

                    foreach (DataColumn column in resultProductDataTable.Columns)
                    {
                        bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                    }

                    bulkcopy.WriteToServer(resultProductDataTable);


                    bulkcopy.DestinationTableName = tempPricingTable;
                    bulkcopy.ColumnMappings.Clear();
                    foreach (DataColumn column in resultPricingDataTable.Columns)
                    {
                        bulkcopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

                    }
                    bulkcopy.WriteToServer(resultPricingDataTable);
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private bool GetProductDataTableForDefaultV2(DataTable productDataTable, DataTable productResultDataTable, DataTable resultPricingDataTable, string branchName, int storeId, int branchId)
        {
            try
            {
                var productTableRows = productDataTable.Select();
                var tableColumnSize = productDataTable.Columns.Count;
                //ProductName Column 
                int indexProductName = productDataTable.Columns.IndexOf("ProductName");

                productResultDataTable.Columns.Add("PictureName");
                productResultDataTable.Columns.Add("Category");
                productResultDataTable.Columns.Add("SubCategory");
                productResultDataTable.Columns.Add("Name");
                productResultDataTable.Columns.Add("ShortDescription");
                productResultDataTable.Columns.Add("FullDescription");
                productResultDataTable.Columns.Add("Manufacturer");
                productResultDataTable.Columns.Add("MetaKeywords");
                productResultDataTable.Columns.Add("MetaDescription");
                productResultDataTable.Columns.Add("MetaTitle");
                productResultDataTable.Columns.Add("ManufacturerPartNumber");
                productResultDataTable.Columns.Add("Weight");
                productResultDataTable.Columns.Add("Length");
                productResultDataTable.Columns.Add("Width");
                productResultDataTable.Columns.Add("Height");
                productResultDataTable.Columns.Add("Color");
                productResultDataTable.Columns.Add("Size1");
                productResultDataTable.Columns.Add("Size2");
                productResultDataTable.Columns.Add("Size3");
                productResultDataTable.Columns.Add("Size4");
                productResultDataTable.Columns.Add("Size5");
                productResultDataTable.Columns.Add("Size6");
                productResultDataTable.Columns.Add("PermaLink");
                productResultDataTable.Columns.Add("StoreId");
                productResultDataTable.Columns.Add("BranchId");
                productResultDataTable.Columns.Add("PictureName1");
                productResultDataTable.Columns.Add("PictureName2");
                productResultDataTable.Columns.Add("ProductShowOnHomePage");
                productResultDataTable.Columns.Add("ShowOnHomePage");

                //Pricing 

                resultPricingDataTable.Columns.Add("ProductName");
                resultPricingDataTable.Columns.Add("StoreId");
                resultPricingDataTable.Columns.Add("BranchId");
                resultPricingDataTable.Columns.Add("BranchName");
                resultPricingDataTable.Columns.Add("Price");
                resultPricingDataTable.Columns.Add("SpecialPrice");
                resultPricingDataTable.Columns.Add("AdditionalShippingCharge");
                resultPricingDataTable.Columns.Add("AdditionalTax");
                resultPricingDataTable.Columns.Add("FullDescription");


                foreach (var row in productTableRows)
                {
                    var productName = row[indexProductName].ToString();
                    if (!string.IsNullOrEmpty(productName))
                    {
                        DataRow drResult = productResultDataTable.NewRow();
                        DataRow drPricingResult = resultPricingDataTable.NewRow();
                        drResult["Name"] = productName;
                        drResult["StoreId"] = storeId;
                        drResult["BranchId"] = branchId;
                        MapProductColumnsForDefaultV2(drResult, row);


                        productResultDataTable.Rows.Add(drResult);

                        drPricingResult["ProductName"] = productName;
                        drPricingResult["StoreId"] = storeId;
                        drPricingResult["BranchName"] = branchName;
                        drPricingResult["BranchId"] = branchId;
                        MapPricingColumnsForDefaultV2(drPricingResult, row);
                        resultPricingDataTable.Rows.Add(drPricingResult);
                    }

                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        private void MapProductColumnsForDefaultV2(DataRow drResult, DataRow row)
        {
            drResult["Category"] = row["Category"];
            drResult["SubCategory"] = row["SubCategory"];
            drResult["FullDescription"] = row["FullDescription"];
            drResult["PermaLink"] = row["PermaLink"];
            drResult["PictureName"] = row["ProductImages"];
            drResult["ShowOnHomePage"] = row["ShowOnHomePage"];
            drResult["ProductShowOnHomePage"] = row["ProductShowOnHomePage"];
            drResult["PictureName1"] = row["CategoryImage"];
            drResult["PictureName2"] = row["SubCategoryImage"];
        }
        private void MapPricingColumnsForDefaultV2(DataRow drPricingResult, DataRow row)
        {
            drPricingResult["Price"] = row["Price"];
            drPricingResult["SpecialPrice"] = row["SpecialPrice"];
            drPricingResult["FullDescription"] = row["FullDescription"];
        }
        private string LoadProductTableFromTempForDefaultV2(bool temploadStatus, int storeId, int branchId, string branchName)
        {
            string message = "";
            if (temploadStatus)
            {
                message = " \r\n " + " Excel Validated for product......";
                try
                {

                    int numNewCategory = _loadProductData.LoadCategoryForDefaultV2(storeId, branchId);
                    message = message + " \r\n " + " Successfully loaded " + numNewCategory + " number of category";

                    int numNewManufacture = _loadProductData.LoadManufacturerDefault(branchName, storeId, branchId);
                    message = message + " \r\n " + " Successfully loaded " + numNewManufacture + " number of Manufacture";

                    int manufactureId = _loadProductData.GetManufacturerIdDefault(branchName);

                    int loadedProductCount = _loadProductData.LoadProductsAndMappingForDefaultV2(manufactureId);

                    message = message + " \r\n " + "Successfully loaded " + loadedProductCount + " number of Products";

                    int loadPricingStatus = _loadProductData.LoadPricingForDefaultV2();
                    message = message + "Updated Pricing for" + loadPricingStatus.ToString();
                }
                catch (Exception ex)
                {
                    message = message + " \r\n " + ex.Message;
                }
            }
            else
            {
                message = message + " \r\n " + " Excel is having issue. Please Check for Column is blank or any issue in excel data......";
                message = message + " \r\n " + "Error loading products";
                message = message + " \r\n " + "Error loading products...Excel Is having issue. Please Check for Column is blank or any issue in excel data";
            }
            message = message + " \r\n " + " Done with Updating Products... " + DateTime.Now.ToString();

            return message;
        }

        // for load default data v3
        public string ImportDataTableToDefaultDataV3(DataTable dt, string branchName, int storeId, int branchId)
        {
            string message = "";
            try
            {

                bool temploadStatus = LoadTempProductAndPricingForDefaultV2(dt, branchName, storeId, branchId);
                string loadProductStatus = LoadProductTableFromTempForDefaultV3(temploadStatus, storeId, branchId, branchName);

                message = message + loadProductStatus;

                int loadPricingStatus = _loadProductData.LoadPricingForDefaultV3();
                message = message + "Updated Pricing for" + loadPricingStatus.ToString();

                int loadProductImageStatus = _loadProductData.LoadProductImageForDefaultV3();
                message = message + "Updated ProductImage for" + loadProductImageStatus.ToString();

                DeleteTempProduct(storeId, branchId);
                DeleteTempPricing(storeId, branchId);
            }
            catch (Exception ex)
            {
                return message + ex.Message;
            }
            return message;
        }
        private string LoadProductTableFromTempForDefaultV3(bool temploadStatus, int storeId, int branchId, string branchName)
        {
            string message = "";
            if (temploadStatus)
            {
                message = " \r\n " + " Excel Validated for product......";
                try
                {

                    int numNewCategory = _loadProductData.LoadCategoryForDefaultV2(storeId, branchId);
                    message = message + " \r\n " + " Successfully loaded " + numNewCategory + " number of category";

                    int numNewManufacture = _loadProductData.LoadManufacturerDefault(branchName, storeId, branchId);
                    message = message + " \r\n " + " Successfully loaded " + numNewManufacture + " number of Manufacture";

                    int manufactureId = _loadProductData.GetManufacturerIdDefault(branchName);

                    int loadedProductCount = _loadProductData.LoadProductsAndMappingForDefaultV3();

                    message = message + " \r\n " + "Successfully loaded " + loadedProductCount + " number of Products";

                    int loadedProductMappingCount = _loadProductData.LoadProductsAndMappingForDefaultV3(manufactureId);

                    message = message + " \r\n " + "Successfully loaded " + loadedProductMappingCount + " number of Products";
                }
                catch (Exception ex)
                {
                    message = message + " \r\n " + ex.Message;
                }
            }
            else
            {
                message = message + " \r\n " + " Excel is having issue. Please Check for Column is blank or any issue in excel data......";
                message = message + " \r\n " + "Error loading products";
                message = message + " \r\n " + "Error loading products...Excel Is having issue. Please Check for Column is blank or any issue in excel data";
            }
            message = message + " \r\n " + " Done with Updating Products... " + DateTime.Now.ToString();

            return message;
        }
    }
}
