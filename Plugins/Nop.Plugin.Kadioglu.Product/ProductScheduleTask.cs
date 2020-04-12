using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Xml;
using Castle.Core.Logging;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Tasks;
using Nop.Core.Http;
using Nop.Core.Plugins;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.Seo;
using Nop.Services.Tasks;
using Nop.Services.Vendors;

namespace Nop.Plugin.Kadioglu.Product
{
    public class ProductScheduleTask : BasePlugin, IScheduleTask
    {
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ICategoryService _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly Nop.Services.Logging.ILogger _logger;
        private readonly IVendorService _vendorService;
        private readonly IUrlRecordService _urlRecordService;

        public ProductScheduleTask(
            IProductService productService,
            IManufacturerService manufacturerService,
            IPictureService pictureService,
            ICategoryService categoryService,
            ICurrencyService currencyService,
            Nop.Services.Logging.ILogger logger,
            IVendorService vendorService,
            IUrlRecordService urlRecordService,
            IScheduleTaskService scheduleTaskService)
        {
            _vendorService = vendorService;
            _logger = logger;
            _urlRecordService = urlRecordService;
            _pictureService = pictureService;
            _productService = productService;
            _manufacturerService = manufacturerService;
            _categoryService = categoryService;
            _scheduleTaskService = scheduleTaskService;
            _currencyService = currencyService;
        }

        public void Execute()
        {
            try
            {

                var request = (HttpWebRequest)WebRequest.Create("http://www.bilgisayarim.com.tr/xml?ClientCode=23676&UserName=Admin&Password=MS2VR5D5");
                using (var response = request.GetResponse())
                {
                    var document = new XmlDocument();
                    document.Load(response.GetResponseStream());
                    var userNodes = document.SelectNodes("//Urunler/Urun");

                    var vendor = _vendorService.GetAllVendors("Kadıoğlu").FirstOrDefault();
                    if (vendor == null)
                    {
                        vendor = new Core.Domain.Vendors.Vendor()
                        {
                            Active = true,
                            Name = "Kadıoğlu",
                            Email = "Kadioglu@com.tr",
                            Deleted = false,
                            PictureId = 0,
                            AddressId = 0,
                            DisplayOrder = 1,
                            PageSize = 6,
                            AllowCustomersToSelectPageSize = true,
                            PageSizeOptions = "6, 3, 9, 18"
                        };
                        _vendorService.InsertVendor(vendor);
                        _urlRecordService.InsertUrlRecord(new Core.Domain.Seo.UrlRecord()
                        {
                            EntityId = vendor.Id,
                            EntityName = "Vendor",
                            IsActive = true,
                            LanguageId = 0,
                            Slug = vendor.Name.Replace("+", "-").Replace("ö", "o").Replace("ş", "s").Replace("ı", "i").Replace("ğ", "g").Replace("ç", "c").Replace("ü", "u").Replace(".", "").Replace(",", "").Replace("\"", "").Replace(",", "").Replace(" ", "-").Replace("---", "-").Replace("--", "-").ToLower()
                        });
                    }

                    _logger.Information("Product Service " + vendor.Name);

                    foreach (XmlNode userNode in userNodes)
                    {
                        try
                        {
                            string urunID = userNode.SelectNodes("UrunID").Item(0) != null ? userNode.SelectNodes("UrunID").Item(0).InnerText : "";
                            var anyProduct = _productService.GtinProduct(urunID);
                            var product = new Nop.Core.Domain.Catalog.Product();
                            if (anyProduct == null)
                            {
                                _logger.Information("Product urunID " + urunID);
                                product.VendorId = vendor.Id;
                                product.Gtin = urunID;
                                Category mainCategories = null, parentCategories = null, childCategories = null;

                                _logger.Information("Product Fiyat");
                                if (userNode.SelectNodes("Fiyat").Item(0) != null)
                                {
                                    _logger.Information("Product Fiyat " + userNode.SelectNodes("Fiyat").Item(0).InnerText);
                                }

                                _logger.Information("Product DovizTuru");
                                if (userNode.SelectNodes("DovizTuru").Item(0) != null )
                                {
                                    _logger.Information("Product DovizTuru " + userNode.SelectNodes("DovizTuru").Item(0).InnerText);
                                }
                                decimal prices = 0;
                                string dovizTuru = "";
                                try
                                {
                                    prices = userNode.SelectNodes("Fiyat").Item(0) != null ? Convert.ToDecimal(userNode.SelectNodes("Fiyat").Item(0).InnerText, new CultureInfo("tr-TR")) : 0;
                                    dovizTuru = userNode.SelectNodes("DovizTuru").Item(0) != null ? userNode.SelectNodes("DovizTuru").Item(0).InnerText : "";
                                    if (!string.IsNullOrWhiteSpace(dovizTuru) && dovizTuru != "USD")
                                    {
                                        var getCurrrency = _currencyService.GetAllCurrencies(true);
                                        var getdoviz = getCurrrency.Where(x => x.Name == dovizTuru).FirstOrDefault();
                                        prices = prices / getdoviz.Rate;
                                        product.ProductCost = prices;
                                        prices = prices + ((prices / 100) * 15);
                                    }
                                    else
                                    {
                                        product.ProductCost = prices;
                                        prices = prices + ((prices / 100) * 15);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error("KadiogluExecute 3 ", ex);
                                }
                                _logger.Information("Product prices " + urunID);

                                string urunAciklama = userNode.SelectNodes("UrunAciklama").Item(0) != null ? userNode.SelectNodes("UrunAciklama").Item(0).InnerText : "";
                                string anaKategori = userNode.SelectNodes("AnaKategori").Item(0) != null ? userNode.SelectNodes("AnaKategori").Item(0).InnerText : "";
                                if (!string.IsNullOrWhiteSpace(anaKategori))
                                {
                                    mainCategories = CreateCategory(anaKategori, 0);
                                    var productCategory = new ProductCategory();
                                    productCategory.CategoryId = mainCategories.Id;
                                    productCategory.Product = product;
                                    productCategory.DisplayOrder = 3;
                                    product.ProductCategories.Add(productCategory);
                                }

                                _logger.Information("Product ProductCategory 1 " + urunID);
                                string araKategori = userNode.SelectNodes("AraKategori").Item(0) != null ? userNode.SelectNodes("AraKategori").Item(0).InnerText : "";
                                if (!string.IsNullOrWhiteSpace(araKategori))
                                {
                                    parentCategories = CreateCategory(araKategori, mainCategories != null ? mainCategories.Id : 1);
                                    var productCategory = new ProductCategory();
                                    productCategory.CategoryId = parentCategories.Id;
                                    productCategory.Product = product;
                                    productCategory.DisplayOrder = 3;
                                    product.ProductCategories.Add(productCategory);
                                }
                                _logger.Information("Product ProductCategory 2 " + urunID);

                                string altKategori = userNode.SelectNodes("AltKategori").Item(0) != null ? userNode.SelectNodes("AltKategori").Item(0).InnerText : "";
                                if (!string.IsNullOrWhiteSpace(altKategori))
                                {
                                    childCategories = CreateCategory(altKategori, parentCategories != null ? parentCategories.Id : 1);
                                    var productCategory = new ProductCategory();
                                    productCategory.CategoryId = childCategories.Id;
                                    productCategory.Product = product;
                                    productCategory.DisplayOrder = 3;
                                    product.ProductCategories.Add(productCategory);
                                }
                                _logger.Information("ProductCategory 3" + urunID);

                                product.ProductTypeId = 5;
                                product.ParentGroupedProductId = 0;
                                product.VisibleIndividually = true;
                                product.ProductTemplateId = 1;
                                product.VendorId = 0;
                                product.ShowOnHomePage = false;
                                product.AllowCustomerReviews = true;
                                product.ApprovedRatingSum = 0;
                                product.NotApprovedRatingSum = 0;
                                product.ApprovedTotalReviews = 1;
                                product.NotApprovedTotalReviews = 0;
                                product.SubjectToAcl = false;
                                product.LimitedToStores = false;
                                product.Sku = "";
                                product.IsGiftCard = false;
                                product.GiftCardTypeId = 0;
                                product.RequireOtherProducts = false;
                                product.AutomaticallyAddRequiredProducts = false;
                                product.IsDownload = false;
                                product.DownloadId = 0;
                                product.UnlimitedDownloads = false;
                                product.MaxNumberOfDownloads = 0;
                                product.DownloadActivationTypeId = 0;
                                product.HasSampleDownload = false;
                                product.SampleDownloadId = 0;
                                product.HasUserAgreement = false;
                                product.IsRecurring = false;
                                product.RecurringCycleLength = 0;
                                product.RecurringCyclePeriodId = 0;
                                product.RecurringTotalCycles = 0;
                                product.IsRental = false;
                                product.RentalPriceLength = 0;
                                product.RentalPricePeriodId = 0;
                                product.IsShipEnabled = true;
                                product.IsFreeShipping = false;
                                product.ShipSeparately = false;
                                product.AdditionalShippingCharge = 0;
                                product.DeliveryDateId = 0;
                                product.IsTaxExempt = false;
                                product.IsTelecommunicationsOrBroadcastingOrElectronicServices = false;
                                product.ManageInventoryMethodId = 1;
                                product.ProductAvailabilityRangeId = 0;
                                product.UseMultipleWarehouses = false;
                                product.WarehouseId = 0;
                                product.StockQuantity = userNode.SelectNodes("Stok").Item(0) != null ? Convert.ToInt32(userNode.SelectNodes("Stok").Item(0).InnerText) : 0;
                                product.DisplayStockAvailability = true;
                                product.DisplayStockQuantity = true;
                                product.MinStockQuantity = 10;
                                product.LowStockActivityId = 1;
                                product.NotifyAdminForQuantityBelow = 1;
                                product.BackorderModeId = 0;
                                product.AllowBackInStockSubscriptions = false;
                                product.OrderMinimumQuantity = 1;
                                product.OrderMaximumQuantity = 100;
                                product.AllowAddingOnlyExistingAttributeCombinations = false;
                                product.NotReturnable = false;
                                product.DisableBuyButton = false;
                                product.DisableWishlistButton = false;
                                product.AvailableForPreOrder = false;
                                product.CallForPrice = false;
                                product.Price = prices;
                                product.OldPrice = 0;
                                product.ProductCost = 0;
                                product.CustomerEntersPrice = false;
                                product.MinimumCustomerEnteredPrice = 0;
                                product.MaximumCustomerEnteredPrice = 0;
                                product.BasepriceEnabled = false;
                                product.BasepriceAmount = 0;
                                product.BasepriceUnitId = 0;
                                product.BasepriceBaseAmount = 0;
                                product.BasepriceBaseUnitId = 0;
                                product.MarkAsNew = true;
                                product.MarkAsNewStartDateTimeUtc = DateTime.Now;
                                product.MarkAsNewEndDateTimeUtc = new DateTime(2020, 04, 20);
                                product.HasTierPrices = false;
                                product.HasDiscountsApplied = false;
                                product.Weight = 0;
                                product.Length = 0;
                                product.Width = 0;
                                product.Height = 0;
                                product.DisplayOrder = 0;
                                product.Published = true;
                                product.Deleted = false;
                                product.CreatedOnUtc = DateTime.Now;
                                product.UpdatedOnUtc = DateTime.Now;
                                product.TaxCategoryId = 2;

                                string urunAciklama2 = userNode.SelectNodes("UrunAciklama2").Item(0) != null ? userNode.SelectNodes("UrunAciklama2").Item(0).InnerText : "";
                                string urunAciklama3 = userNode.SelectNodes("UrunAciklama3").Item(0) != null ? userNode.SelectNodes("UrunAciklama3").Item(0).InnerText : "";
                                string urunAciklama4 = userNode.SelectNodes("UrunAciklama4").Item(0) != null ? userNode.SelectNodes("UrunAciklama4").Item(0).InnerText : "";

                                product.MetaTitle = urunAciklama;
                                product.MetaDescription = urunAciklama ?? urunAciklama.Replace(" ", ",") + urunAciklama2 ?? "," + urunAciklama2.Replace(" ", ",") + urunAciklama3 ?? "," + urunAciklama3.Replace(" ", ",") + urunAciklama4 ?? "," + urunAciklama4.Replace(" ", ",");
                                product.MetaKeywords = urunAciklama ?? urunAciklama.Replace(" ", ",") + urunAciklama4 ?? "," + urunAciklama4.Replace(" ", ",");
                                product.Name = urunAciklama;
                                product.ShortDescription = urunAciklama2;
                                product.FullDescription = urunAciklama + Environment.NewLine + urunAciklama2 + Environment.NewLine + urunAciklama3 + Environment.NewLine + urunAciklama4;

                                //string urunKodu = userNode.SelectNodes("UrunKodu").Count > 0 ? userNode.SelectNodes("UrunKodu").Item(0).InnerText : "";

                                //string ureticiKodu = userNode.SelectNodes("UreticiKodu").Item(0).InnerText;
                                //string gtipCode = userNode.SelectNodes("GtipCode").Item(0).InnerText;
                                //string durum = userNode.SelectNodes("Durum").Item(0).InnerText;

                                string marka = userNode.SelectNodes("Marka").Item(0) != null ? userNode.SelectNodes("Marka").Item(0).InnerText : "";
                                if (!string.IsNullOrWhiteSpace(marka))
                                {
                                    var getManufacturer = _manufacturerService.GetAllManufacturers(marka);
                                    if (getManufacturer.TotalCount <= 0)
                                    {
                                        var manufacutrer = new Manufacturer();
                                        manufacutrer.Name = marka;
                                        manufacutrer.ManufacturerTemplateId = 1;
                                        manufacutrer.PageSize = 6;
                                        manufacutrer.AllowCustomersToSelectPageSize = true;
                                        manufacutrer.PageSizeOptions = "6,3,9";
                                        manufacutrer.SubjectToAcl = false;
                                        manufacutrer.LimitedToStores = false;
                                        manufacutrer.Deleted = false;
                                        manufacutrer.Published = true;
                                        manufacutrer.DisplayOrder = _manufacturerService.GetAllManufacturers().Count + 1;
                                        manufacutrer.CreatedOnUtc = DateTime.Now;
                                        manufacutrer.UpdatedOnUtc = DateTime.Now;
                                        _manufacturerService.InsertManufacturer(manufacutrer);

                                        _urlRecordService.InsertUrlRecord(new Core.Domain.Seo.UrlRecord()
                                        {
                                            EntityId = manufacutrer.Id,
                                            EntityName = "Manufacturer",
                                            IsActive = true,
                                            LanguageId = 0,
                                            Slug = manufacutrer.Name.Replace("+","-").Replace("ö","o").Replace("ş", "s").Replace("ı", "i").Replace("ğ", "g").Replace("ç", "c").Replace("ü", "u").Replace(".", "").Replace(",", "").Replace("\"", "").Replace(",", "").Replace(" ", "-").Replace("---", "-").Replace("--", "-").ToLower()
                                        });
                                        product.ProductManufacturers.Add(new ProductManufacturer() { ManufacturerId = manufacutrer.Id, Product = product, IsFeaturedProduct = false, DisplayOrder = 1 });
                                    }
                                    else
                                    {
                                        product.ProductManufacturers.Add(new ProductManufacturer() { ManufacturerId = getManufacturer.FirstOrDefault().Id, Product = product, IsFeaturedProduct = false, DisplayOrder = 1 });
                                    }
                                }
                                _logger.Information("Manufacturer " + urunID);

                                string gorselKucuk = userNode.SelectNodes("GorselKucuk").Item(0) != null ? userNode.SelectNodes("GorselKucuk").Item(0).InnerText : "";
                                string gorselBuyuk = userNode.SelectNodes("GorselBuyuk").Item(0) != null ? userNode.SelectNodes("GorselBuyuk").Item(0).InnerText : "";
                                string gorselKucuk2 = userNode.SelectNodes("GorselKucuk2").Item(0) != null ? userNode.SelectNodes("GorselKucuk2").Item(0).InnerText : "";
                                string gorselBuyuk2 = userNode.SelectNodes("GorselBuyuk2").Item(0) != null ? userNode.SelectNodes("GorselBuyuk2").Item(0).InnerText : "";

                                if (!string.IsNullOrWhiteSpace(gorselBuyuk))
                                {
                                    InsertImage(gorselBuyuk, product.Name.Replace(" ", "").Split("\"").FirstOrDefault(), product, 1);
                                }
                                if (!string.IsNullOrWhiteSpace(gorselBuyuk2))
                                {
                                    InsertImage(gorselBuyuk2, product.Name.Replace(" ", "").Split("\"").FirstOrDefault(), product, 2);
                                }
                                if (!string.IsNullOrWhiteSpace(gorselKucuk))
                                {
                                    InsertImage(gorselKucuk, product.Name.Replace(" ", "").Split("\"").FirstOrDefault(), product, 3);
                                }
                                if (!string.IsNullOrWhiteSpace(gorselKucuk2))
                                {
                                    InsertImage(gorselKucuk2, product.Name.Replace(" ", "").Split("\"").FirstOrDefault(), product, 4);
                                }
                                _productService.InsertProduct(product);

                                _urlRecordService.InsertUrlRecord(new Core.Domain.Seo.UrlRecord()
                                {
                                    EntityId = product.Id,
                                    EntityName = "Product",
                                    IsActive = true,
                                    LanguageId = 0,
                                    Slug = product.Name.Replace("+", "-").Replace("ö", "o").Replace("ş", "s").Replace("ı", "i").Replace("ğ", "g").Replace("ç", "c").Replace("ü", "u").Replace(".", "").Replace(",", "").Replace("\"", "").Replace(",", "").Replace(" ", "-").Replace("---", "-").Replace("--", "-").ToLower()
                                }); 
                                _logger.Information("KadiogluExecute Bilgi " + product.Name);
                            }
                            else
                            {
                                decimal prices = userNode.SelectNodes("Fiyat").Item(0) != null ? Convert.ToDecimal(userNode.SelectNodes("Fiyat").Item(0).InnerText, new CultureInfo("tr-TR")) : 0;
                                string dovizTuru = userNode.SelectNodes("DovizTuru").Item(0) != null ? userNode.SelectNodes("DovizTuru").Item(0).InnerText : "";
                                if (!string.IsNullOrWhiteSpace(dovizTuru) && dovizTuru != "USD")
                                {
                                    var getCurrrency = _currencyService.GetAllCurrencies(true);
                                    var getdoviz = getCurrrency.Where(x => x.Name == dovizTuru).FirstOrDefault();
                                    prices = prices / getdoviz.Rate;
                                    anyProduct.ProductCost = prices;
                                    prices = prices + ((prices / 100) * 15);
                                }
                                else
                                {
                                    anyProduct.ProductCost = prices;
                                    prices = prices + ((prices / 100) * 15);
                                }
                                anyProduct.StockQuantity = userNode.SelectNodes("Stok").Item(0) != null ? Convert.ToInt32(userNode.SelectNodes("Stok").Item(0).InnerText) : 0;
                                anyProduct.Price = prices;
                                anyProduct.VendorId = vendor.Id;
                                _productService.UpdateProduct(anyProduct);
                                _logger.Information("KadiogluExecute Bilgi 2 " + anyProduct.Name);
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("KadiogluExecute 1 ", ex);
                        }
                        
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.Error("KadiogluExecute", ex);
            }
        }

        private void InsertImage(string url, string name, Nop.Core.Domain.Catalog.Product product, int orderNo)
        {
            try
            {
                using (var myWebClient = new WebClient())
                {
                    byte[] myDataBuffer = myWebClient.DownloadData(url);
                    var picture = _pictureService.InsertPicture(myDataBuffer, "image/jpeg", name);

                    var newProductPic = new ProductPicture();
                    newProductPic.DisplayOrder = orderNo;
                    newProductPic.PictureId = picture.Id;
                    newProductPic.Product = product;
                    product.ProductPictures.Add(newProductPic);
                }
            }
            catch (Exception ex)
            {
                _logger.Error("InsertImage", ex);
            }
        }

        private Category CreateCategory(string anaKategori, int parentCategory)
        {
            if (_categoryService.AnyCategory(anaKategori))
            {
                return _categoryService.GetCategoryName(anaKategori);
            }
            var category = new Category();
            category.CreatedOnUtc = DateTime.UtcNow;
            category.UpdatedOnUtc = DateTime.UtcNow;
            category.Name = anaKategori;
            category.PageSize = 9;
            category.DisplayOrder = _categoryService.LastDisplayOrder();
            category.Deleted = false;
            category.Published = true;
            category.LimitedToStores = false;
            category.SubjectToAcl = false;
            category.IncludeInTopMenu = false;
            category.ShowOnHomePage = false;
            category.PageSizeOptions = "6, 3, 9";
            category.AllowCustomersToSelectPageSize = true;
            category.ParentCategoryId = parentCategory;
            category.CategoryTemplateId = 1;
            category.MetaKeywords = "Şansder, Şander Platform," + anaKategori.Replace(" ", ",");
            category.MetaDescription = "Şansder, Şansder Platform" + anaKategori.Replace(" ", ",");
            category.MetaTitle = anaKategori;
            _categoryService.InsertCategory(category);

            _urlRecordService.InsertUrlRecord(new Core.Domain.Seo.UrlRecord()
            {
                EntityId = category.Id,
                EntityName = "Category",
                IsActive = true,
                LanguageId = 0,
                Slug = category.Name.Replace("+", "-").Replace("ö", "o").Replace("ş", "s").Replace("ı", "i").Replace("ğ", "g").Replace("ç", "c").Replace("ü", "u").Replace(".", "").Replace(",", "").Replace("\"", "").Replace(",", "").Replace(" ", "-").Replace("---", "-").Replace("--", "-").ToLower()
            });
            return category;
        }


        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        { //install synchronization task
            if (_scheduleTaskService.GetTaskByType("Nop.Plugin.Kadioglu.Product.ProductScheduleTask") == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = true,
                    Seconds = 12 * 60 * 60,
                    Name = "Kadioglu (Product)",
                    Type = "Nop.Plugin.Kadioglu.Product.ProductScheduleTask",
                });
            }
            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            base.Uninstall();
        }
    }
}
