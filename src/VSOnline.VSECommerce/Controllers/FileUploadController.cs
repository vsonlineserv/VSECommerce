using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;
using VSOnline.VSECommerce.Domain;
using VSOnline.VSECommerce.Domain.Loader;
using VSOnline.VSECommerce.Permission;
using VSOnline.VSECommerce.Persistence.Data;
using VSOnline.VSECommerce.Persistence.Entity;
using VSOnline.VSECommerce.Utilities;

namespace VSOnline.VSECommerce.Controllers
{
    [Route("")]
    [ApiController]
    public class FileUploadController : VSControllerBase
    {
        private readonly DataContext _context;
        private readonly SellerRepository _sellerRepository;
        private readonly ImportProductData _importProductData;
        public FileUploadController(DataContext context, IOptions<AppSettings> _appSettings, SellerRepository sellerRepository, ImportProductData importProductData) : base(_appSettings)
        {
            _context = context;
            _sellerRepository = sellerRepository;
            _importProductData = importProductData;
        }

        [Authorize(Policy = PolicyTypes.Category_Write)]
        [HttpPost("Seller/{BranchId}/UploadCategoryImage/{categoryId}")]
        public IActionResult UploadCategoryImage(int BranchId, int categoryId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var storeId = _context.SellerBranch.Where(x => x.BranchId == BranchId).Select(x => x.Store).FirstOrDefault();
                var ReferenceId = _context.Seller.Where(x => x.StoreId == storeId).Select(x => x.StoreRefereneId).FirstOrDefault();
                var storeReferenceId = ReferenceId.ToString();

                string fileName = string.Empty;
                var requestImage = HttpContext.Request;
                if (requestImage.Form.Files.Count > 0)
                {
                    var httpPostedFile = requestImage.Form.Files[0];

                    if (httpPostedFile.ContentType == "image/jpeg" || httpPostedFile.ContentType == "image/png" || httpPostedFile.ContentType == "image/jpg" || httpPostedFile.ContentType == "image/gif" || httpPostedFile.ContentType == "image/webp")
                    {
                        if (httpPostedFile != null && categoryId > 0)
                        {
                            var category = _context.Category.Where(x => x.CategoryId == categoryId).First<Category>();
                            bool flagTransfer = false;
                            string folderName = _appSettings.CategoryFolder;
                            string extenstion = string.Empty;
                            extenstion = System.IO.Path.GetExtension(httpPostedFile.FileName);
                            fileName = storeReferenceId + "/" +folderName + category.CategoryId + "-" + DateTime.UtcNow.Ticks.ToString() + extenstion;
                            flagTransfer = FtpTransfer(httpPostedFile, fileName);

                            if (fileName != null)
                            {
                                category.CategoryImage = flagTransfer ? fileName : "";
                            }
                            if (flagTransfer)
                            {
                                _context.Category.Update(category);
                                _context.SaveChanges();
                            }
                        }
                    }
                }
                return Ok(Enums.UpdateStatus.Success.ToString());
            }
            catch (Exception e)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Write)]
        [HttpPost("Seller/{BranchId}/UploadMultipleImage/{productId}")]
        public IActionResult UploadMultipleImage(int BranchId, int productId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var storeId = _context.SellerBranch.Where(x => x.BranchId == BranchId).Select(x => x.Store).FirstOrDefault();
                var ReferenceId = _context.Seller.Where(x => x.StoreId == storeId).Select(x => x.StoreRefereneId).FirstOrDefault();
                var storeReferenceId = ReferenceId.ToString();
                var httpContext = HttpContext.Request;
                if (httpContext.Form.Files.Count > 0)
                {
                    for (int i = 0; i < httpContext.Form.Files.Count; i++)
                    {
                        var httpPostedFile = httpContext.Form.Files[i];
                        if (httpPostedFile.ContentType == "image/jpeg" || httpPostedFile.ContentType == "image/png" || httpPostedFile.ContentType == "image/jpg" || httpPostedFile.ContentType == "image/gif" || httpPostedFile.ContentType == "image/webp")
                        {
                            if (httpPostedFile != null && productId > 0)
                            {
                                var product = _context.Product.Where(x => x.ProductId == productId).First<Product>();
                                var imageCount = _context.ProductImage.Where(x => x.ProductId == productId).Count();
                                if (Convert.ToInt32(imageCount) <= 7)
                                {
                                    bool flagTransfer = false;
                                    string folderName = _appSettings.ImageFolder;
                                    //var fileName = folderName + product.ProductId + i + httpPostedFile.FileName; // need to add store reference id
                                    var fileName = storeReferenceId + "/" +folderName + product.ProductId + i + httpPostedFile.FileName;
                                    flagTransfer = FtpTransfer(httpPostedFile, fileName);
                                    ProductImage productImage = new ProductImage();
                                    productImage.ProductId = product.ProductId;
                                    productImage.PictureName = fileName;
                                    productImage.CreatedDate = DateTime.UtcNow;
                                    _context.ProductImage.Add(productImage);
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    return BadRequest("Max 7 Images can be allowed");
                                }
                            }
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "There is some error");
            }
        }

        [Authorize(Policy = PolicyTypes.Product_Write)]
        [HttpPost("Seller/{BranchId}/UploadBulkProducts")]
        public IActionResult UploadBulkProducts(int BranchId)
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var httpContext = HttpContext.Request;
                if (httpContext.Form.Files.Count > 0)
                {
                }
                if (HttpContext.Request.Form.Files.Any())
                {
                    // Get the uploaded image from the Files collection
                    var httpPostedFile = HttpContext.Request.Form.Files[0];
                    //var currentUser = User.Identity.Name;
                    var currentUser = User.FindFirst("UserId")?.Value;
                    var retailer = _sellerRepository.GetRetailerInfo(currentUser);

                    if (httpPostedFile.ContentType == "application/vnd.ms-excel"
                         || httpPostedFile.ContentType == "application/csv" || httpPostedFile.ContentType == "text/plain"
                        || httpPostedFile.ContentType == "text/comma-separated-values" || httpPostedFile.ContentType == "application/vnd.msexcel")
                    {
                        var csvProductDataTable = new DataTable();

                        string folderNameExcel = _appSettings.ProductUploadFolder.ToString();
                        var fileName = folderNameExcel + "_" + DateTime.UtcNow.ToString() + "_" + currentUser + "_" + httpPostedFile.FileName;


                        if (httpPostedFile.ContentType == "text/plain")
                        {
                            csvProductDataTable = ImportCSV(httpPostedFile, new string[] { "\t" });
                        }
                        else
                        {
                            csvProductDataTable = ImportfromExcelProductData(httpPostedFile);
                        }

                        //FtpTransfer(httpPostedFile, fileName);

                        //if (csvProductDataTable != null && csvProductDataTable.Rows.Count > 0)
                        //{
                        //    var flagDataProcessed = _importProductData.ImportDataTableToProduct(csvProductDataTable, retailer.Branches[0].BranchName, retailer.StoreId, retailer.Branches[0].BranchId);
                        //    return Ok(flagDataProcessed);
                        //}
                        if (csvProductDataTable != null && csvProductDataTable.Rows.Count > 0)
                        {
                            var flagDataProcessed = _importProductData.ImportDataTableToDefaultDataV3(csvProductDataTable, retailer.Branches[0].BranchName, retailer.StoreId, BranchId);
                            return Ok(flagDataProcessed);
                        }

                    }
                    return BadRequest(Enums.UpdateStatus.Error.ToString());
                }
            }
            catch
            {
                return StatusCode(500, "There is some error");
            }
            return BadRequest(Enums.UpdateStatus.Failure.ToString());

        }
        private DataTable ImportCSV(IFormFile httpPostedFile, string[] sepString)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(httpPostedFile.OpenReadStream()))
            {

                string firstLine = sr.ReadLine();
                var headers = firstLine.Split(sepString, StringSplitOptions.None);
                foreach (var header in headers)
                {
                    //create column headers
                    dt.Columns.Add(header);
                }
                int columnInterval = headers.Count();
                string newLine = sr.ReadLine();
                while (newLine != null)
                {
                    //loop adds each row to the datatable
                    var fields = newLine.Split(sepString, StringSplitOptions.None); // csv delimiter    
                    var currentLength = fields.Count();
                    if (currentLength < columnInterval)
                    {
                        while (currentLength < columnInterval)
                        {
                            //if the count of items in the row is less than the column row go to next line until count matches column number total
                            newLine += sr.ReadLine();
                            currentLength = newLine.Split(sepString, StringSplitOptions.None).Count();
                        }
                        fields = newLine.Split(sepString, StringSplitOptions.None);
                    }
                    if (currentLength > columnInterval)
                    {
                        //ideally never executes - but if csv row has too many separators, line is skipped
                        //newLine = sr.ReadLine();
                        //continue;
                    }
                    dt.Rows.Add(fields);
                    newLine = sr.ReadLine();
                }
                sr.Close();
            }

            return dt;
        }
        private DataTable ImportfromExcelProductData(IFormFile httpPostedFile)
        {
            DataTable dataTable = new DataTable();
            if (Request.Form.Files.Count > 0)
            {
                if (httpPostedFile != null && httpPostedFile.Length > 0)
                {
                    Stream stream = httpPostedFile.OpenReadStream();
                    IExcelDataReader reader = null;
                    if (httpPostedFile.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (httpPostedFile.FileName.EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });
                    reader.Close();

                    if (result != null && result.Tables.Count > 0)
                    {
                        return result.Tables[0];
                    }
                    return null;
                }
            }
            return null;
        }
        private bool FtpTransfer(IFormFile httpPostedFile, string fileName)
        {
            try
            {
                // to compress file 
                if (httpPostedFile.Length > 500000)
                {
                    using (var ms = new MemoryStream())
                    {
                        Bitmap originalBMP = new Bitmap(httpPostedFile.OpenReadStream());
                        int origWidth = originalBMP.Width;
                        int origHeight = originalBMP.Height;
                        int sngRatio = origWidth / origHeight;
                        int newWidth = 473;
                        int newHeight = 593;
                        Bitmap newBMPbig = new Bitmap(originalBMP, newWidth, newHeight);
                        Graphics oGraphics1 = Graphics.FromImage(newBMPbig);
                        oGraphics1.SmoothingMode = SmoothingMode.AntiAlias; oGraphics1.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        oGraphics1.DrawImage(originalBMP, 0, 0, newWidth, newHeight);
                        //newBMPbig.Save(fileName + ".jpg");
                        newBMPbig.Save(ms, ImageFormat.Png);
                        var memor = ms.GetBuffer();
                        var fileBytes = memor.ToArray();
                        WebRequest request = WebRequest.Create("ftp://" + _appSettings.FTPAddress + fileName);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.Credentials = new NetworkCredential(_appSettings.FTPUserName, _appSettings.FTPPassword);
                        Stream reqStream = request.GetRequestStream();
                        reqStream.Write(fileBytes, 0, fileBytes.Length);
                        reqStream.Close();
                        originalBMP.Dispose();
                        newBMPbig.Dispose();
                        oGraphics1.Dispose();
                    }
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        httpPostedFile.CopyTo(ms);
                        var fileBytes = ms.ToArray();
                        WebRequest request = WebRequest.Create("ftp://" + _appSettings.FTPAddress + fileName);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        request.Credentials = new NetworkCredential(_appSettings.FTPUserName, _appSettings.FTPPassword);
                        Stream reqStream = request.GetRequestStream();
                        reqStream.Write(fileBytes, 0, fileBytes.Length);
                        reqStream.Close();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // for load default data 
        [HttpGet("UploadDefaultData")]
        public string UploadDefaultData(string excelFileName,string BranchName,int StoreId,int BranchId)
        {

            try
            {
                //Read template file from the DefaultData folder
                string defaultData = @"DefaultData/";
                string path = defaultData + excelFileName;

                var stream = System.IO.File.OpenRead(path);

                var httpPostedFile = new FormFile(stream, 0, stream.Length, "streamFile", path.Split(@"\").Last());

                var csvProductDataTable = new DataTable();

                csvProductDataTable = ImportfromExcelProductDataForDefault(httpPostedFile);

                if (csvProductDataTable != null && csvProductDataTable.Rows.Count > 0)
                {
                    var flagDataProcessed = _importProductData.ImportDataTableToDefaultData(csvProductDataTable, BranchName, StoreId, BranchId);
                    return flagDataProcessed;
                }
                return Enums.UpdateStatus.Error.ToString();
            }
            catch (Exception ex)
            {
                return Enums.UpdateStatus.Error.ToString();
            }
            return Enums.UpdateStatus.Failure.ToString();

        }

        private DataTable ImportfromExcelProductDataForDefault(IFormFile httpPostedFile)
        {
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                DataTable dataTable = new DataTable();

                if (httpPostedFile != null && httpPostedFile.Length > 0)
                {
                    Stream stream = httpPostedFile.OpenReadStream();
                    IExcelDataReader reader = null;
                    if (httpPostedFile.FileName.EndsWith(".xls"))
                    {
                        reader = ExcelReaderFactory.CreateBinaryReader(stream);
                    }
                    else if (httpPostedFile.FileName.EndsWith(".xlsx"))
                    {
                        reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                    }
                    DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });
                    reader.Close();

                    if (result != null && result.Tables.Count > 0)
                    {
                        return result.Tables[0];
                    }
                    return null;
                }
            }
            catch(Exception ex)
            {

            }
            return null;
        }


        //manufacurer
        [Authorize(Policy = PolicyTypes.Category_Write)]
        [HttpPost("Seller/{BranchId}/UploadManufacturerImage/{ManufacturerId}")]
        public IActionResult UploadManufacturerImage(int BranchId, int ManufacturerId)  
        {
            try
            {
                var branchIds = User.FindAll("BranchId").ToList();
                if (!branchIds.Where(a => a.Value == BranchId.ToString()).Any())
                {
                    return Unauthorized();
                }
                var storeId = _context.SellerBranch.Where(x => x.BranchId == BranchId).Select(x => x.Store).FirstOrDefault();
                var ReferenceId = _context.Seller.Where(x => x.StoreId == storeId).Select(x => x.StoreRefereneId).FirstOrDefault();
                var storeReferenceId = ReferenceId.ToString();

                string fileName = string.Empty;
                var requestImage = HttpContext.Request;
                if (requestImage.Form.Files.Count > 0)
                {
                    var httpPostedFile = requestImage.Form.Files[0];

                    if (httpPostedFile.ContentType == "image/jpeg" || httpPostedFile.ContentType == "image/png" || httpPostedFile.ContentType == "image/jpg" || httpPostedFile.ContentType == "image/gif" || httpPostedFile.ContentType == "image/webp")
                    {
                        if (httpPostedFile != null && ManufacturerId > 0)
                        {
                            var manufacturer = _context.Manufacturer.Where(x => x.ManufacturerId == ManufacturerId).FirstOrDefault();
                            if(manufacturer != null)
                            {
                                bool flagTransfer = false;
                                string folderName = _appSettings.CategoryFolder;
                                string extenstion = string.Empty;
                                extenstion = System.IO.Path.GetExtension(httpPostedFile.FileName);
                                fileName = storeReferenceId + "/" + folderName + manufacturer.ManufacturerId + "-" + DateTime.UtcNow.Ticks.ToString() + extenstion;
                                flagTransfer = FtpTransfer(httpPostedFile, fileName);

                                if (fileName != null)
                                {
                                    manufacturer.ManufacturerImage = flagTransfer ? fileName : "";
                                }
                                if (flagTransfer)
                                {
                                    _context.Manufacturer.Update(manufacturer);
                                    _context.SaveChanges();
                                    return Ok(Enums.UpdateStatus.Success.ToString());
                                }
                                else
                                {
                                    return Ok("File Not Uploaded");
                                }
                            }
                        }
                    }
                    return Ok("Supported File types are jpeg/png");
                }
                return Ok("File Cannot be Empty");
            }
            catch (Exception e)
            {
                return StatusCode(500, "There is some error");
            }
        }

        // For Default data in product store mapping table productId v2
        [HttpGet("UploadDefaultDataV2")]
        public string UploadDefaultDataV2(string excelFileName, string BranchName, int StoreId, int BranchId)
        {

            try
            {
                //Read template file from the DefaultData folder
                string defaultData = @"DefaultData/";
            
                string path = defaultData + excelFileName;

                var stream = System.IO.File.OpenRead(path);

                var httpPostedFile = new FormFile(stream, 0, stream.Length, "streamFile", path.Split(@"\").Last());

                var csvProductDataTable = new DataTable();

                csvProductDataTable = ImportfromExcelProductDataForDefault(httpPostedFile);

                if (csvProductDataTable != null && csvProductDataTable.Rows.Count > 0)
                {
                    var flagDataProcessed = _importProductData.ImportDataTableToDefaultDataV2(csvProductDataTable, BranchName, StoreId, BranchId);
                    return flagDataProcessed;
                }
                return Enums.UpdateStatus.Error.ToString();
            }
            catch (Exception ex)
            {
                return Enums.UpdateStatus.Error.ToString();
            }
            return Enums.UpdateStatus.Failure.ToString();

        }

        [HttpGet("UploadDefaultDataV3")]
        public string UploadDefaultDataV3(string BranchName, int StoreId, int BranchId)
        {

            try
            {
                string defaultData = @"StoreData/";
                string path = defaultData + BranchName + ".xlsx";

                var stream = System.IO.File.OpenRead(path);

                var httpPostedFile = new FormFile(stream, 0, stream.Length, "streamFile", path.Split(@"\").Last());
                var csvProductDataTable = new DataTable();

                csvProductDataTable = ImportfromExcelProductDataForDefault(httpPostedFile);

                if (csvProductDataTable != null && csvProductDataTable.Rows.Count > 0)
                {
                    var flagDataProcessed = _importProductData.ImportDataTableToDefaultDataV3(csvProductDataTable, BranchName, StoreId, BranchId);
                    return flagDataProcessed;
                }
                return Enums.UpdateStatus.Error.ToString();

            }
            catch (Exception ex)
            {
                return Enums.UpdateStatus.Error.ToString();
            }
            return Enums.UpdateStatus.Failure.ToString();

        }

        [Authorize]
        [HttpPost("UploadUserImage/{userId}")]
        public IActionResult UploadUserImage(int userId)
        {
            try
            {
                string fileName = string.Empty;
                var requestImage = HttpContext.Request;
                if (requestImage.Form.Files.Count > 0)
                {
                    var httpPostedFile = requestImage.Form.Files[0];

                    if (httpPostedFile.ContentType == "image/jpeg" || httpPostedFile.ContentType == "image/png" || httpPostedFile.ContentType == "image/jpg" || httpPostedFile.ContentType == "image/gif" || httpPostedFile.ContentType == "image/webp")
                    {
                        if (httpPostedFile != null && userId > 0)
                        {
                            var user = _context.User.Where(x => x.UserId == userId).First<User>();
                            bool flagTransfer = false;
                            string folderName = _appSettings.UserFolder;
                            string extenstion = string.Empty;
                            extenstion = System.IO.Path.GetExtension(httpPostedFile.FileName);
                            fileName = folderName + "/" + user.UserId + "-" + DateTime.UtcNow.Ticks.ToString() + extenstion;
                            flagTransfer = FtpTransfer(httpPostedFile, fileName);

                            if (fileName != null)
                            {
                                user.UserProfileImage = flagTransfer ? fileName : "";
                            }
                            if (flagTransfer)
                            {
                                _context.User.Update(user);
                                _context.SaveChanges();
                            }
                        }
                    }
                }
                return Ok(Enums.UpdateStatus.Success.ToString());
            }
            catch (Exception e)
            {
                return StatusCode(500, "There is some error");
            }
        }
    }
}
