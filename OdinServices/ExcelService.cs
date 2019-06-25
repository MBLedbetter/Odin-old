﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using OdinModels;
using Microsoft.Office.Interop.Excel;
using System.Windows.Forms;
using System.Linq;
using System.Collections.ObjectModel;
using Odin.Data;

namespace OdinServices
{
    public class ExcelService
    {
        #region Properties

        private Microsoft.Office.Interop.Excel.Application app = null;

        private Microsoft.Office.Interop.Excel.Workbook workbook = null;

        private Microsoft.Office.Interop.Excel.Worksheet worksheet = null;

        /// <summary>
        ///     List of all letters in the alphabet
        /// </summary>
        public string[] Alphabet = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        
        /// <summary>
        ///     List of existing images on externalCaptures folder on the web server
        /// </summary>
        private List<string> ExistingFiles
        {
            get
            {
                return _existingFiles;
            }
            set
            {
                _existingFiles = value;
            }
        }
        private List<string> _existingFiles = new List<string>();

        /// <summary>
        ///     Gets or sets the ftp service
        /// </summary>
        public FtpService FtpService { get; set; }

        /// <summary>
        ///     Gets or sets the itemservice
        /// </summary>
        public ItemService ItemService { get; set; }
        
        /// <summary>
        ///     Gets or sets the missing ftp files
        /// </summary>
        public List<string> MissingFtpFiles
        {
            get
            {
                return _missingFtpFiles;
            }
            set
            {
                _missingFtpFiles = value;
            }
        }
        private List<string> _missingFtpFiles = new List<string>();

        /// <summary>
        ///     Gets or sets the optionService
        /// </summary>
        public OptionService OptionService { get; set; }

        /// <summary>
        ///     Gets or sets the requestnum
        /// </summary>
        int RequestNum { get; set; }

        public IRequestRepository RequestRepository { get; set; }
        
        public ITemplateRepository TemplateRepository { get; set; }

        /// <summary>
        ///     Gets or sets the LayoutList
        /// </summary>
        public List<Layout> LayoutList
        {
            get { return _layoutList; }
            set { _layoutList = value; }
        }
        private List<Layout> _layoutList = new List<Layout>();

        #region HeaderLists

        /// <summary>
        ///     List of headers for the item template
        /// </summary>
        public List<string> ItemHeaders = new List<string>();

        /// <summary>
        ///     List of headers for the template template
        /// </summary>
        public List<string> TemplateHeaders = new List<string>();

        #endregion // HeaderLists

        #endregion // Properties

        #region Methods

        /// <summary>
        ///     Check if submitted file exists amongst the files on the server. Adds file name to MissingFtpFiles 
        ///     if it doesn't exist
        /// </summary>
        /// <param name="fileName"></param>
        public bool CheckFtpFileExists(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string[] x = fileName.Split('/');
                if (!this.ExistingFiles.Contains(x[x.Length - 1].Trim()))
                {
                    this.MissingFtpFiles.Add(x[x.Length - 1]);
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Create a excell documents for a given list of items
        /// </summary>
        /// <param name="workbookType"></param>
        /// <param name="itemsList"></param>
        /// <returns></returns>
        public void CreateExcelSheet(ObservableCollection<ItemObject> itemsList, ObservableCollection<ExcelCell> excelCells, string customer, string strFilePath = null)
        {
            bool createFile = true;
            if (strFilePath == null)
            {
                SaveFileDialog dlg = new SaveFileDialog()
                {
                    Filter = "Excel Workbooks (*.xlsx)|*.xlsx"
                };
                if (dlg.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }
                strFilePath = dlg.FileName;
            }
            Cursor.Show();
            app = new Microsoft.Office.Interop.Excel.Application()
            {
                Visible = false
            };
            workbook = app.Workbooks.Add(1);
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];
            AddHeaders(excelCells);

            AddRowData(excelCells, itemsList, customer);
            
            if (createFile)
            {
                workbook.SaveAs(strFilePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing);
                Cursor.Hide();
                workbook.ReadOnly.Equals(false);
                workbook.Close();
                MessageBox.Show("Excel Document is Complete. " + strFilePath);
            }        
        }

        #region Add data methods

        /// <summary>
        ///     Writes out the field name as the first row of template
        /// </summary>
        /// <param name="excelCells"></param>
        public void AddHeaders(ObservableCollection<ExcelCell> excelCells)
        {
            int count = 1;
            foreach (ExcelCell header in excelCells)
            {
                worksheet.Cells[1, count] = header.Field;
                count++;
            }
        }

        /// <summary>
        ///     Inserts a row data for each item into an excel sheet
        /// </summary>
        /// <param name="excelCells"></param>
        /// <param name="item"></param>
        /// <param name="row"></param>
        public void AddRowData(ObservableCollection<ExcelCell> excelCells, ObservableCollection<ItemObject> items, string customer)
        {
            int columnCount = 1;
            foreach (ExcelCell cell in excelCells)
            {
                RetrieveCellValue(cell.Field, items, customer, columnCount);
                columnCount++;
            }
        }

        /// <summary>
        ///     Inserts a row of data for the template export file
        /// </summary>
        /// <param name="template"></param>
        /// <param name="row"></param>
        private void AddTemplateData(ItemObject template, int row)
        {
            worksheet.Cells[row, 1] = template.TemplateId; // Template ID
            worksheet.Cells[row, 2] = template.ItemGroup; // Item Group
            worksheet.Cells[row, 3] = template.ItemFamily; // Item Family
            worksheet.Cells[row, 4] = template.ItemCategory; // Item Category
            worksheet.Cells[row, 5] = template.CostProfileGroup; // Cost Profile Group
            worksheet.Cells[row, 6] = template.MfgSource; // Mfg Source
            worksheet.Cells[row, 7] = template.Weight; // Item Weight
            worksheet.Cells[row, 8] = template.Length; // Item Length
            worksheet.Cells[row, 9] = template.Height; // Item Height
            worksheet.Cells[row, 10] = template.Width; // Item Width
            worksheet.Cells[row, 11] = template.TariffCode; // Tariff Code
            worksheet.Cells[row, 12] = template.CountryOfOrigin; // Country of Origin
            worksheet.Cells[row, 13] = template.DefaultActualCostUsd; // Default Actual Cost USD
            worksheet.Cells[row, 14] = template.DefaultActualCostCad; // Default Actual Cost CAD
            worksheet.Cells[row, 15] = template.PricingGroup; // Price Group (Product)
            worksheet.Cells[row, 16] = template.AccountingGroup; // Acctg Group (Product)
            worksheet.Cells[row, 17] = template.ListPriceUsd; // List Price (USD)
            worksheet.Cells[row, 18] = template.ListPriceCad; // List Price (CAD)
            worksheet.Cells[row, 19] = template.ListPriceMxn; // List Price (MXN)
            worksheet.Cells[row, 20] = template.Msrp; // MSRP
            worksheet.Cells[row, 21] = template.MsrpCad; // MSRP CAD
            worksheet.Cells[row, 22] = template.MsrpMxn; // MSRP MXN
            worksheet.Cells[row, 23] = template.Udex; // UDEX
            worksheet.Cells[row, 24] = template.Gpc; // GPC
            worksheet.Cells[row, 25] = template.PrintOnDemand; // Print On Demand
            worksheet.Cells[row, 26] = template.ProductGroup; // Product Group
            worksheet.Cells[row, 27] = template.ProductLine; // Product Line
            worksheet.Cells[row, 28] = template.ProductFormat; // Product Format
            worksheet.Cells[row, 29] = template.CasepackHeight; // Casepack Height
            worksheet.Cells[row, 30] = template.CasepackLength; // Casepack Length
            worksheet.Cells[row, 31] = template.CasepackWeight; // Casepack Weight
            worksheet.Cells[row, 32] = template.CasepackWidth; // Casepack Width
            worksheet.Cells[row, 33] = template.CasepackQty; // Casepack Qty
            worksheet.Cells[row, 34] = template.InnerpackHeight; // Innerpack Height
            worksheet.Cells[row, 35] = template.InnerpackLength; // Innerpack Length
            worksheet.Cells[row, 36] = template.InnerpackWeight; // Innerpack Weight
            worksheet.Cells[row, 37] = template.InnerpackWidth; // Innerpack Width
            worksheet.Cells[row, 38] = template.InnerpackQuantity; // Innerpack Qty
            worksheet.Cells[row, 39] = template.Category; // Category
            worksheet.Cells[row, 40] = template.Category2; // Category2
            worksheet.Cells[row, 41] = template.Category3; // Category3
            worksheet.Cells[row, 42] = template.Copyright; // Copyright
            worksheet.Cells[row, 43] = template.MetaDescription; // Meta Description
            worksheet.Cells[row, 44] = template.Size; // Size
            worksheet.Cells[row, 45] = template.EcommerceBullet1; // Ecommerce Bullet 1
            worksheet.Cells[row, 46] = template.EcommerceBullet2; // Ecommerce Bullet 2
            worksheet.Cells[row, 47] = template.EcommerceBullet3; // Ecommerce Bullet 3
            worksheet.Cells[row, 48] = template.EcommerceBullet4; // Ecommerce Bullet 4
            worksheet.Cells[row, 49] = template.EcommerceBullet5; // Ecommerce Bullet 5
            worksheet.Cells[row, 50] = template.EcommerceComponents; // Ecommerce Components
            worksheet.Cells[row, 51] = template.EcommerceCost; // Ecommerce Cost
            worksheet.Cells[row, 52] = template.EcommerceExternalIdType; // Ecommerce External ID Type
            worksheet.Cells[row, 53] = template.EcommerceItemHeight; // Ecommerce Item Height
            worksheet.Cells[row, 54] = template.EcommerceItemLength; // Ecommerce Item Length
            worksheet.Cells[row, 55] = template.EcommerceItemTypeKeywords; // Ecommerce Item Type Keywords
            worksheet.Cells[row, 56] = template.EcommerceItemWeight; // Ecommerce Item Weight
            worksheet.Cells[row, 57] = template.EcommerceItemWidth; // Ecommerce Item Width
            worksheet.Cells[row, 58] = template.EcommerceModelName; // Ecommerce Model Name
            worksheet.Cells[row, 59] = template.EcommercePackageHeight; // Ecommerce Package Height
            worksheet.Cells[row, 60] = template.EcommercePackageLength; // Ecommerce Package Length
            worksheet.Cells[row, 61] = template.EcommercePackageWeight; // Ecommerce Package Weight
            worksheet.Cells[row, 62] = template.EcommercePackageWidth; // Ecommerce Package Width
            worksheet.Cells[row, 63] = template.EcommercePageQty; // Ecommerce Page Count
            worksheet.Cells[row, 64] = template.EcommerceProductCategory; // Ecommerce Product Category
            worksheet.Cells[row, 65] = template.EcommerceProductDescription; // Ecommerce Product Description
            worksheet.Cells[row, 66] = template.EcommerceProductSubcategory; // Ecommerce Product Subcategory
            worksheet.Cells[row, 67] = template.EcommerceManufacturerName; // Ecommerce Manufacturer Name
            worksheet.Cells[row, 68] = template.EcommerceMsrp; // Ecommerce Msrp
            worksheet.Cells[row, 69] = template.EcommerceSize; // Ecommerce Size
            worksheet.Cells[row, 70] = template.WebsitePrice; // Website Price
        }

        #endregion // Add data methods

        #region Insert Methods

        /// <summary>
        ///     Establish connection and run InsertExcelLayoutColumn foreach cell in list
        /// </summary>
        /// <param name="cellList"></param>
        /// <returns></returns>
        public void InsertExcelLayoutColumns(ObservableCollection<ExcelCell> cellList)
        {
            TemplateRepository.InsertExcelLayoutColumns(cellList);
        }

        /// <summary>
        ///     Inserts a column data into ODIN_EXCEL_LAYOUT_IDS
        /// </summary>
        /// <param name="layoutName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public void InsertExcelLayout(string layoutName, string customer, string productType)
        {
            TemplateRepository.InsertExcelLayout(layoutName, customer, productType);
            RetrieveExcelLayouts();
        }

        #endregion // Insert Methods

        #region Modifier Methods

        /// <summary>
        ///     Converts the existing image path to reflect the location of additional images
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string ModifyAdditionalImageUrl(string value)
        {
            string[] pathParts = value.Split('/');
            string fileName = pathParts[pathParts.Length - 1];
            string result = "https://trendsinternational.com/media/externalCaptures/" + fileName;
            result = result.Replace(" ", "%20");
            return result;
        }

        /// <summary>
        ///     Creates a combined bullet point seperated string for the items bullet points
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string ModifyBulletedCopy(ItemObject item)
        {
            string result = string.Empty;

            if (!string.IsNullOrEmpty(item.EcommerceBullet1)) { result += "\u2022" + item.EcommerceBullet1; }
            if (!string.IsNullOrEmpty(item.EcommerceBullet2)) { result += "\r\n\u2022" + item.EcommerceBullet2; }
            if (!string.IsNullOrEmpty(item.EcommerceBullet3)) { result += "\r\n\u2022" + item.EcommerceBullet3; }
            if (!string.IsNullOrEmpty(item.EcommerceBullet4)) { result += "\r\n\u2022" + item.EcommerceBullet4; }
            if (!string.IsNullOrEmpty(item.EcommerceBullet5)) { result += "\r\n\u2022" + item.EcommerceBullet5; }
            return result;
        }

        /// <summary>
        ///     Adds (1) to all components that don't have a parenthetical number
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        public string ModifyComponents(string components)
        {
            string result = string.Empty;

            string[] componentList = components.Split('^');
            foreach (string x in componentList)
            {
                if (x.Contains("("))
                {
                    result += x + "^";
                }
                else
                {
                    result += x + "(1)^";
                }
            }
            result = result.TrimEnd('^');
            return result;
        }

        /// <summary>
        ///     Merge ecommerce bullet fields into a single comman delimited string
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string MergeBullets(ItemObject item)
        {
            string returnValue = string.Empty;
            returnValue += item.EcommerceBullet1;
            returnValue += ", " + item.EcommerceBullet2;
            returnValue += ", " + item.EcommerceBullet3;
            if (!string.IsNullOrEmpty(item.EcommerceBullet4))
            {
                returnValue += ", " + item.EcommerceBullet4;
            }
            if (!string.IsNullOrEmpty(item.EcommerceBullet5))
            {
                returnValue += ", " + item.EcommerceBullet5;
            }
            return returnValue;
        }

        /// <summary>
        ///     removes prefix from sku and appends the number to the end of the keywords
        /// </summary>
        /// <param name="sku"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public string ModifyKeywords(string sku, string keywords)
        {
            string value;
            string skuInt = Regex.Replace(sku, "[^0-9.]", "");
            keywords = keywords.Replace(';', ',');
            if (!keywords.EndsWith(","))
            {
                keywords = keywords + ", ";
            }
            value = keywords + skuInt;
            return value;
        }

        /// <summary>
        ///     Returns a list of the configurable variations for this parent item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string SetConfigurableVariationLabels(ItemObject item)
        {
            return "poster_options=Poster Options";
        }

        /// <summary>
        ///     Returns the configurable variation labels for this parent item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string SetConfigurableVariations(List<string> items)
        {
            string result = string.Empty;
            foreach(string x in items)
            {
                if(!string.IsNullOrEmpty(result))
                {
                    result += "|";
                }
                result += "sku=" + x + ",poster_options=" + ReturnPosterOption(x);
            }
            return result;
        }

        public string SetCustomOptions(ItemObject item)
        {
            string result = string.Empty;

            foreach (string itemId in item.RelatedProducts)
            {
                if (itemId.Contains("SIL22X34") || itemId.Contains("BLK22X34") || itemId.Contains("POD"))
                {
                    result += "name = Frame,";
                    result += "type = drop_down,";
                    result += "required = 0,";
                    result += "price_type =fixed,";
                    result += "file_extension =,";
                    result += "image_size_x = 0,";
                    result += "image_size_y = 0,";
                    if (itemId.Contains("SIL22X34"))
                    {
                        result += "price = 40.0000,";
                        result += "sku = SIL22X34,";
                        result += "option_title = Silver Frame";
                    }
                    else if (itemId.Contains("BLK22X34"))
                    {
                        result += "price = 40.0000,";
                        result += "sku = BLK22X34,";
                        result += "option_title = Black Frame";
                    }
                    else if (itemId.Contains("POD"))
                    {
                        result += "price = 10.0000,";
                        result += "sku = PREMIUM,";
                        result += "option_title = Premium Print";
                    }
                    result += "|";
                }
            }
            if (!string.IsNullOrEmpty(result))
            {
                result = result.Remove(result.Length - 1);
            }
            return result;
        }

        /// <summary>
        ///     If the file exists on externalCaptures return filepath otherwise returns ""
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string SetImagePath(string filePath, bool fileExists)
        {
            if (!string.IsNullOrEmpty(filePath) && fileExists)
            {
                string[] pathParts = filePath.Split('/');
                string fileName = pathParts[pathParts.Length - 1];
                string result = "http://trendsinternational.com/media/externalCaptures/" + fileName;
                result = result.Replace(" ", "%20");
                return result;
            }
            return "";            
        }

        /// <summary>
        ///     Sorts a list of items into groups based on the non-prefixed item sku. Will return a list of grouped item numbers with a coresponding
        ///     list of prefixes.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, List<ItemObject>>> SortAmazonItemVariations(ObservableCollection<ItemObject> items)
        {
            List<KeyValuePair<string, ItemObject>> initialList = new List<KeyValuePair<string, ItemObject>>();
            List<KeyValuePair<string, List<ItemObject>>> resultList = new List<KeyValuePair<string, List<ItemObject>>>();
            List<string> existingItemIds = new List<string>();
            foreach (ItemObject item in items)
            {
                if (item.ItemId.Substring(0, 2) == "RP")
                {
                    initialList.Add(new KeyValuePair<string, ItemObject>(item.ItemId.Substring(2), item));
                    if (!existingItemIds.Contains(item.ItemId.Substring(2)))
                    {
                        existingItemIds.Add(item.ItemId.Substring(2));
                    }
                }
                else if (item.ItemId.Substring(0, 2) == "FR")
                {
                    initialList.Add(new KeyValuePair<string, ItemObject>(item.ItemId.Substring(2), item));
                    if (!existingItemIds.Contains(item.ItemId.Substring(2)))
                    {
                        existingItemIds.Add(item.ItemId.Substring(2));
                    }
                }
                else if (item.ItemId.Substring(0, 3) == "POD")
                {
                    initialList.Add(new KeyValuePair<string, ItemObject>(item.ItemId.Substring(3), item));
                    if (!existingItemIds.Contains(item.ItemId.Substring(3)))
                    {
                        existingItemIds.Add(item.ItemId.Substring(3));
                    }
                }
            }
            foreach (string itemId in existingItemIds)
            {
                List<ItemObject> idList = new List<ItemObject>();
                foreach (KeyValuePair<string, ItemObject> x in initialList)
                {
                    if (x.Key == itemId)
                    {
                        idList.Add(x.Value);
                    }
                }
                KeyValuePair<string, List<ItemObject>> z = new KeyValuePair<string, List<ItemObject>>(itemId, idList);
                resultList.Add(z);
            }
            return resultList;
        }

        /// <summary>
        ///     Trims amazon keywords down to maximum field value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string TrimSearchTerms(string value, string customer)
        {
            if (customer == "AMAZON")
            {
                if (value.Length > 385)
                {
                    value = value.Substring(0, 385);
                    while (!value.EndsWith(";"))
                    {
                        value = value.Remove(value.Length - 1, 1);
                    }
                    return value;
                }
            }
            return value;            
        }

        #endregion // Modifier Methods
        
        /// <summary>
        ///     Create a excell documents for a given list of items
        /// </summary>
        /// <param name="workbookType"></param>
        /// <param name="itemsList"></param>
        /// <returns></returns>
        public bool CreateItemWorkbook(string layoutName, ObservableCollection<ItemObject> itemsList, string filePath = null)
        {
            this.MissingFtpFiles = new List<string>();
            ObservableCollection<ExcelCell> excelCells = RetrieveExcelLayoutData(layoutName);
            string customer = RetrieveExcelLayoutCustomer(layoutName);
            CreateExcelSheet(itemsList, excelCells, customer, filePath);
            return true;
        }

        /// <summary>
        ///     Creates comman seperated string of headers for the magento csv file
        /// </summary>
        /// <returns></returns>
        private string CreateMagentoHeaders()
        {
            string value = string.Empty;

            value += "sku,";
            value += "_store,";
            value += "_attribute_set,";
            value += "_type,";
            value += "_category,";
            value += "_root_category,";
            value += "_product_websites,";
            value += "color,";
            value += "cost,";
            value += "country_of_manufacture,";
            value += "created_at,";
            value += "has_options,";
            value += "height,";
            value += "language,";
            value += "legal,";
            value += "license,";
            value += "license_2,";
            value += "license_end_date,";
            value += "meta_description,";
            value += "meta_keyword,";
            value += "meta_title,";
            value += "msrp,";
            value += "msrpcan,";
            value += "msrp_display_actual_price_type,";
            value += "msrp_enabled,";
            value += "name,";
            value += "date_added,";
            value += "news_from_date,";
            value += "news_to_date,";
            value += "options_container,";
            value += "price,";
            value += "pricecan,";
            value += "required_options,";
            value += "short_description,";
            value += "status,";
            value += "tax_class_id,";
            value += "territory,";
            value += "upc,";
            value += "updated_at,";
            value += "url_key,";
            value += "url_path,";
            value += "visibility,";
            value += "weight,";
            value += "width,";
            value += "size,";
            value += "min_qty,";
            value += "pack_qty,";
            value += "use_config_min_qty,";
            value += "is_qty_decimal,";
            value += "backorders,";
            value += "use_config_backorders,";
            value += "min_sale_qty,";
            value += "use_config_min_sale_qty,";
            value += "max_sale_qty,";
            value += "use_config_max_sale_qty,";
            value += "is_in_stock,";
            value += "notify_stock_qty,";
            value += "use_config_notify_stock_qty,";
            value += "manage_stock,";
            value += "use_config_manage_stock,";
            value += "stock_status_changed_auto,";
            value += "use_config_qty_increments,";
            value += "qty_increments,";
            value += "use_config_enable_qty_inc,";
            value += "enable_qty_increments,";
            value += "is_decimal_divided,";
            value += "_links_related_sku,";
            value += "_links_related_position,";
            value += "_media_attribute_id,";
            value += "_media_position,";
            value += "_media_is_disabled,";
            value += "qty,";
            value += "license_begin_date";

            return value;
        }

        /// <summary>
        ///     Creates comman seperated string of headers for the magento 2 csv file
        /// </summary>
        /// <returns></returns>
        private string CreateMagento2Headers()
        {
            string value = string.Empty;
            value += "sku,"; /* A */
            value += "store_view_code,"; /* B */
            value += "attribute_set_code,"; /* C */
            value += "product_type,"; /* D */
            value += "categories,"; /* E */
            value += "product_websites,"; /* F */
            value += "name,"; /* G */
            value += "description,"; /* H */
            value += "short_description,"; /* I */
            value += "weight,"; /* J */
            value += "product_online,"; /* K */
            value += "tax_class_name,"; /* L */
            value += "visibility,"; /* M */
            value += "price,"; /* N */
            value += "special_price,"; /* O */
            value += "special_price_from_date,"; /* P */
            value += "special_price_to_date,"; /* Q */
            value += "url_key,"; /* R */
            value += "meta_title,"; /* S */
            value += "meta_keywords,"; /* T */
            value += "meta_description,"; /* U */
            value += "base_image,"; /* V */
            value += "base_image_label,"; /* W */
            value += "small_image,"; /* X */
            value += "small_image_label,"; /* Y */
            value += "thumbnail_image,"; /* Z */
            value += "thumbnail_image_label,"; /* AA */
            value += "swatch_image,"; /* AB */
            value += "swatch_image_label,"; /* AC */
            value += "created_at,"; /* AD */
            value += "updated_at,"; /* AE */
            value += "new_from_date,"; /* AF */
            value += "new_to_date,"; /* AG */
            value += "display_product_options_in,"; /* AH */
            value += "map_price,"; /* AI */
            value += "msrp_price,"; /* AJ */
            value += "map_enabled,"; /* AK */
            value += "gift_message_available,"; /* AL */
            value += "custom_design,"; /* AM */
            value += "custom_design_from,"; /* AN */
            value += "custom_design_to,"; /* AO */
            value += "custom_layout_update,"; /* AP */
            value += "page_layout,"; /* AQ*/
            value += "product_options_container,"; /* AR */
            value += "msrp_display_actual_price_type,"; /* AS */
            value += "country_of_manufacture,"; /* AT */
            value += "additional_attributes,"; /* AU */
            value += "qty,"; /* AV */
            value += "out_of_stock_qty,"; /* AW */
            value += "use_config_min_qty,"; /* AX */
            value += "is_qty_decimal,"; /* AY */
            value += "allow_backorders,"; /* AZ */
            value += "use_config_backorders,"; /* BA */
            value += "min_cart_qty,"; /* BB */
            value += "use_config_min_sale_qty,"; /* BC */
            value += "max_cart_qty,"; /* BD */
            value += "use_config_max_sale_qty,"; /* BE */
            value += "is_in_stock,"; /* BF */
            value += "notify_on_stock_below,"; /* BG */
            value += "use_config_notify_stock_qty,"; /* BH */
            value += "manage_stock,"; /* BI */
            value += "use_config_manage_stock,"; /* BJ */
            value += "use_config_qty_increments,"; /* BK */
            value += "qty_increments,"; /* BL */
            value += "use_config_enable_qty_inc,"; /* BM */
            value += "enable_qty_increments,"; /* BN */
            value += "is_decimal_divided,"; /* BO */
            value += "website_id,"; /* BP */
            value += "deferred_stock_update,"; /* BQ */
            value += "use_config_deferred_stock_update,"; /* BR */
            value += "related_skus,"; /* BS */
            value += "related_position,"; /* BT */
            value += "crosssell_skus,"; /* BU */
            value += "crosssell_position,"; /* BV */
            value += "upsell_skus,"; /* BW */
            value += "upsell_position,"; /* BX */
            value += "additional_images,"; /* BY */
            value += "additional_image_labels,"; /* BZ */
            value += "hide_from_product_page,"; /* CA */
            value += "custom_options,"; /* CB */
            value += "bundle_price_type,"; /* CC */
            value += "bundle_sku_type,"; /* CD */
            value += "bundle_price_view,"; /* CE */
            value += "bundle_weight_type,"; /* CF */
            value += "bundle_values,"; /* CG */
            value += "bundle_shipment_type,"; /* CH */
            value += "giftcard_type,"; /* CI */
            value += "giftcard_allow_open_amount,"; /* CJ */
            value += "giftcard_open_amount_min,"; /* CK */
            value += "giftcard_open_amount_max,"; /* CL */
            value += "giftcard_amount,"; /* CM */
            value += "use_config_is_redeemable,"; /* CN */
            value += "giftcard_is_redeemable,"; /* CO */
            value += "use_config_lifetime,"; /* CP */
            value += "giftcard_lifetime,"; /* CQ */
            value += "use_config_allow_message,"; /* CR */
            value += "giftcard_allow_message,"; /* CS */
            value += "use_config_email_template,"; /* CT */
            value += "giftcard_email_template,"; /* CU */
            value += "associated_skus,"; /* CV */
            value += "configurable_variations,"; /* CW */
            value += "configurable_variation_labels"; /* CX */

            return value;
        }

        /// <summary>
        ///     Create a excell document for a given list of items
        /// </summary>
        /// <param name="workbookType"></param>
        /// <param name="itemsList"></param>
        /// <returns></returns>
        public void CreateTemplateWorkbook(List<ItemObject> templateList = null)
        {
            string strFilePath;
            List<ItemObject> TemplateList = new List<ItemObject>();
            if (templateList == null)
            {
                TemplateList.Add(new ItemObject(2));
            }
            else
            {
                TemplateList = templateList;
            }
            SaveFileDialog dlg = new SaveFileDialog()
            {
                Filter = "Excel Workbooks (*.xlsx)|*.xlsx"
            };
            if (dlg.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            strFilePath = dlg.FileName;
            Cursor.Show();
            app = new Microsoft.Office.Interop.Excel.Application()
            {
                Visible = false
            };
            workbook = app.Workbooks.Add(1);
            worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];

            int row = 2;
            WriteHeaders(this.TemplateHeaders);
            foreach (ItemObject template in TemplateList)
            {
                AddTemplateData(template, row);
                row++;
            }
            workbook.SaveAs(strFilePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                Type.Missing, XlSaveAsAccessMode.xlShared, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing);
            Cursor.Hide();
            MessageBox.Show("Excel Document is Complete. " + strFilePath);
        }
        
        /// <summary>
        ///     Retrieves all additional attributes and creates a comma seperated string
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string FormatAdditionalAttributes(ItemObject item, bool isChild)
        {
            string result = "gift_wrapping_available=No,product_image_size = Default,product_page_type = Full Width,sw_featured = No";

            if (!string.IsNullOrEmpty(item.License) && GlobalData.ReturnShopTrendsLicenses().Contains(item.License.Trim()))
            {
                result += ",license=" + item.License;
            }

            /*
            if(!string.IsNullOrEmpty(item.Property))
            {
                result += ",property=" + item.Property;
            }
            */
            if(isChild)
            {
                result += ",poster_options=" + ReturnPosterOption(item.ItemId);
                result += ",upc=" + item.Upc;
                // result += ",size=" + item.Width + "\"x" + item.Height + "\"";
            }
            return result;
        }

        /// <summary>
        ///     Returns the image names for all the additional image fields
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string FormatAdditionalImages(ItemObject item)
        {
            string result = string.Empty;
            result += (!string.IsNullOrEmpty(item.AltImageFile1)) ? ItemService.ReturnImageName(item.ItemId, 2) + "," : "";
            result += (!string.IsNullOrEmpty(item.AltImageFile2)) ? ItemService.ReturnImageName(item.ItemId, 3) + "," : "";
            result += (!string.IsNullOrEmpty(item.AltImageFile3)) ? ItemService.ReturnImageName(item.ItemId, 4) + "," : "";
            result += (!string.IsNullOrEmpty(item.AltImageFile4)) ? ItemService.ReturnImageName(item.ItemId, 5) + "," : "";
            if(result != string.Empty) { return result.TrimEnd(','); }
            return result;
        }

        /// <summary>
        ///     Retrieves categories and creates a comma seperated string
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string FormatMagento2Categories(ItemObject item)
        {
            string result = "Default Category,Default Category/Shop All";

            if (GlobalData.ReturnShopTrendsLicenses().Contains(item.License))
            {
                if (item.License == "Hello Kitty")
                {
                    item.License = "Sanrio";
                }
                result += ",Default Category/Shop by Brand/" + item.License;
                
                if (!string.IsNullOrEmpty(item.Property))
                {
                    if (item.Property != item.License)
                    {
                        result += ",Default Category/Shop by Brand/" + item.License + "/" + item.Property;
                    }
                }

                if (item.License == "NFL"|| item.License == "MLB" || item.License == "NBA" || item.License == "NHL")
                {
                    result += ",Default Category/Shop by Genre/Sports";
                    if(item.License =="NFL")
                    {
                        result += ",Default Category/Shop by Genre/Sports/Football";
                    }
                    else if (item.License == "NBA")
                    {
                        result += ",Default Category/Shop by Genre/Sports/Basketball";
                    }
                    else if (item.License == "MLB")
                    {
                        result += ",Default Category/Shop by Genre/Sports/Baseball";
                    }
                    else if (item.License == "NHL")
                    {
                        result += ",Default Category/Shop by Genre/Sports/Hockey";
                    }
                }
            }

            if(!string.IsNullOrEmpty(item.Genre1) && item.Genre1 != "OTHER")
            {
                result += ",Default Category/Shop by Genre/" + DbUtil.UppercaseFirst(item.Genre1);
            }
            if (!string.IsNullOrEmpty(item.Genre2) && item.Genre2 != "OTHER")
            {
                result += ",Default Category/Shop by Genre/" + DbUtil.UppercaseFirst(item.Genre2);
            }
            if (!string.IsNullOrEmpty(item.Genre3) && item.Genre3 != "OTHER")
            {
                result += ",Default Category/Shop by Genre/" + DbUtil.UppercaseFirst(item.Genre3);
            }

            return result;
        }

        /// <summary>
        ///     Removes "24X36" from the front of title
        /// </summary>
        /// <param name="oldTitle"></param>
        /// <returns></returns>
        public string FormatMagento2Title(string oldTitle)
        {
            string newTitle = oldTitle;
            if(oldTitle.Substring(0,6).ToUpper()=="24X36 ")
            {
                newTitle = oldTitle.Remove(0, 6);
            }
            return newTitle;
        }

        /// <summary>
        ///     Formats the keywords using ecom name, license, property and keyword overrides
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string FormatMagento2Keywords(ItemObject item)
        {
            string result = string.Empty;

            result += ItemService.RetrieveItemIdCore(item.ItemId) + ",";

            if (!string.IsNullOrEmpty(item.EcommerceItemName))
            {
                result += item.EcommerceItemName + ",";
                if(item.EcommerceItemName.Contains('-'))
                {
                    result += item.EcommerceItemName.Replace("-", "") + ",";
                }
            }
            if (!string.IsNullOrEmpty(item.License))
            {
                result += item.License + ",";
                if (item.License.Contains('-'))
                {
                    result += item.License.Replace("-", "") + ",";
                }
            }
            if (!string.IsNullOrEmpty(item.Property))
            {
                result += item.Property + ",";
                if (item.Property.Contains('-'))
                {
                    result += item.Property.Replace("-", "") + ",";
                }
            }
            if (!string.IsNullOrEmpty(item.Genre1))
            {
                result += item.Genre1 + ",";
            }
            if (!string.IsNullOrEmpty(item.Genre2))
            {
                result += item.Genre2 + ",";
            }
            if (!string.IsNullOrEmpty(item.Genre3))
            {
                result += item.Genre3 + ",";
            }
            if (!string.IsNullOrEmpty(item.ItemKeywordsOverride))
            {
                result += item.ItemKeywordsOverride + ",";
            }

            return result.TrimEnd(',');
        }

        /// <summary>
        ///     Lowercases and replaces spaces with underscoress
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public string FormatMagento2Url(string title, string itemId)
        {
            string result = string.Empty;

            result = title + "_" + itemId;

            if(string.IsNullOrEmpty(result))
            {
                result = result.ToLower().Replace(" ", "_");
            }
            return result;
        }

        public List<string> NewCategories(string cat1, string cat2, string cat3)
        {
            List<string> results = new List<string>();
            string[] inputs = { cat1, cat2, cat3 };

            foreach (string input in inputs)
            {
                if (!string.IsNullOrEmpty(input))
                {
                    string currentCat = string.Empty;
                    string category = input;
                    string[] subCategories = category.Split(':');
                    for (int x = 0; x < subCategories.Length; x++)
                    {
                        if (string.IsNullOrEmpty(currentCat))
                        {
                            currentCat = subCategories[x].Trim();
                        }
                        else
                        {
                            currentCat += "/" + subCategories[x].Trim();
                        }

                        if (!string.IsNullOrEmpty(currentCat))
                        {
                            if (!results.Contains(currentCat))
                            {
                                results.Add(currentCat);
                            }
                        }
                    }
                }
            }

            return results;
        }

        /// <summary>
        ///     Populates the template header list
        /// </summary>
        public void PopulateTemplateHeaders()
        {
            this.TemplateHeaders.Add("Template ID");
            this.TemplateHeaders.Add("Item Group");
            this.TemplateHeaders.Add("Item Family");
            this.TemplateHeaders.Add("Item Category");
            this.TemplateHeaders.Add("Cost Profile Group");
            this.TemplateHeaders.Add("Mfg Source");
            this.TemplateHeaders.Add("Item Weight");
            this.TemplateHeaders.Add("Item Length");
            this.TemplateHeaders.Add("Item Height");
            this.TemplateHeaders.Add("Item Width");
            this.TemplateHeaders.Add("Tariff Code");
            this.TemplateHeaders.Add("Country of Origin");
            this.TemplateHeaders.Add("Default Actual Cost USD");
            this.TemplateHeaders.Add("Default Actual Cost CAD");
            this.TemplateHeaders.Add("Price Group (Product)");
            this.TemplateHeaders.Add("Acctg Group (Product)");
            this.TemplateHeaders.Add("List Price (USD)");
            this.TemplateHeaders.Add("List Price (CAD)");
            this.TemplateHeaders.Add("List Price (MXN)");
            this.TemplateHeaders.Add("MSRP");
            this.TemplateHeaders.Add("MSRP CAD");
            this.TemplateHeaders.Add("MSRP MXN");
            this.TemplateHeaders.Add("UDEX");
            this.TemplateHeaders.Add("GPC");
            this.TemplateHeaders.Add("Print On Demand");
            this.TemplateHeaders.Add("Product Group");
            this.TemplateHeaders.Add("Product Line");
            this.TemplateHeaders.Add("Product Format");
            this.TemplateHeaders.Add("Casepack Height");
            this.TemplateHeaders.Add("Casepack Length");
            this.TemplateHeaders.Add("Casepack Weight");
            this.TemplateHeaders.Add("Casepack Width");
            this.TemplateHeaders.Add("Casepack Qty");
            this.TemplateHeaders.Add("Innerpack Height");
            this.TemplateHeaders.Add("Innerpack Length");
            this.TemplateHeaders.Add("Innerpack Weight");
            this.TemplateHeaders.Add("Innerpack Width");
            this.TemplateHeaders.Add("Innerpack Qty");
            this.TemplateHeaders.Add("Category");
            this.TemplateHeaders.Add("Category2");
            this.TemplateHeaders.Add("Category3");
            this.TemplateHeaders.Add("Copyright");
            this.TemplateHeaders.Add("Meta Description");
            this.TemplateHeaders.Add("Size");
            this.TemplateHeaders.Add("Ecommerce Bullet 1");
            this.TemplateHeaders.Add("Ecommerce Bullet 2");
            this.TemplateHeaders.Add("Ecommerce Bullet 3");
            this.TemplateHeaders.Add("Ecommerce Bullet 4");
            this.TemplateHeaders.Add("Ecommerce Bullet 5");
            this.TemplateHeaders.Add("Ecommerce Components");
            this.TemplateHeaders.Add("Ecommerce Cost");
            this.TemplateHeaders.Add("Ecommerce External ID Type");
            this.TemplateHeaders.Add("Ecommerce Item Height");
            this.TemplateHeaders.Add("Ecommerce Item Type Keywords");
            this.TemplateHeaders.Add("Ecommerce Item Length");
            this.TemplateHeaders.Add("Ecommerce Item Weight");
            this.TemplateHeaders.Add("Ecommerce Item Width");
            this.TemplateHeaders.Add("Ecommerce Model Name");
            this.TemplateHeaders.Add("Ecommerce Package Height");
            this.TemplateHeaders.Add("Ecommerce Package Length");
            this.TemplateHeaders.Add("Ecommerce Package Weight");
            this.TemplateHeaders.Add("Ecommerce Package Width");
            this.TemplateHeaders.Add("Ecommerce Page Qty");
            this.TemplateHeaders.Add("Ecommerce Product Category");
            this.TemplateHeaders.Add("Ecommerce Product Description");
            this.TemplateHeaders.Add("Ecommerce Product Subcategory");
            this.TemplateHeaders.Add("Ecommerce Manufacturer Name");
            this.TemplateHeaders.Add("Ecommerce Msrp");
            this.TemplateHeaders.Add("Ecommerce Size");
            this.TemplateHeaders.Add("Website Price");
        }
                
        #region Removal Methods
        
        /// <summary>
        ///     Removes layout data associate with the given layout id from EXCEL_LAYOUT_DATA and EXCEL_LAYOUT_IDS
        /// </summary>
        /// <param name="layoutName"></param>
        public void RemoveExcelLayout(int layoutId)
        {
            TemplateRepository.RemoveExcelLayout(layoutId);
            TemplateRepository.RemoveExcelLayoutColumns(layoutId);
        }

        /// <summary>
        ///     Removes all column data for the given layout
        /// </summary>
        /// <param name="layoutId"></param>
        /// <returns></returns>
        public void RemoveExcelLayoutColumns(int layoutId)
        {
            TemplateRepository.RemoveExcelLayoutColumns(layoutId);
        }

        #endregion // Removal Methods

        #region Retrieval Methods

        /// <summary>
        ///     Reads the fieldname and associates the coresponding value from the item
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public void RetrieveCellValue(string fieldName, ObservableCollection<ItemObject> items, string customer, int columnCount)
        {
            int row = 2;
            if (!string.IsNullOrEmpty(fieldName))
            {
                // this is used for custom text columns //
                if (fieldName.Substring(0, 1) == "\"")
                {
                    foreach (ItemObject item in items)
                    {
                        WriteCell(row, columnCount, fieldName.Replace("\"", ""));
                        row++;
                    }
                }

                else
                {
                    switch (fieldName)
                    {
                        case "-EMPTY-":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, "");
                                row++;
                            }
                            break;
                        case "Acctg Group (Product)":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.AccountingGroup);
                                row++;
                            }
                            break;
                        case "Bill Of Materials":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ReturnBillOfMaterials());
                                row++;
                            }
                            break;
                        case "Batteries Needed":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, "");
                                row++;
                            }
                            break;
                        case "Battery Cell Type":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, ReturnBatteryCellType(item.PricingGroup));
                                row++;
                            }
                            break;
                        case "Brand Name":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, ReturnBrandName(item.ProductLine));
                                row++;
                            }
                            break;
                        case "Browse Keyword":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, ReturnBrowseKeyword(item.EcommerceProductSubcategory));
                                row++;
                            }
                            break;
                        case "Casepack Height":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.CasepackHeight);
                                row++;
                            }
                            break;
                        case "Casepack Length":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.CasepackLength);
                                row++;
                            }
                            break;
                        case "Casepack Weight":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.CasepackWeight);
                                row++;
                            }
                            break;
                        case "Casepack Width":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.CasepackWidth);
                                row++;
                            }
                            break;
                        case "Casepack Qty":
                            foreach (ItemObject item in items)
                            {
                                if (!string.IsNullOrEmpty(item.CasepackQty))
                                {
                                    WriteCell(row, columnCount, item.CasepackQty);
                                }
                                else
                                {
                                    WriteCell(row, columnCount, "0");
                                }
                                row++;
                            }
                            break;
                        case "Category":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Category);
                                row++;
                            }
                            break;
                        case "Category2":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Category2);
                                row++;
                            }
                            break;
                        case "Category3":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Category3);
                                row++;
                            }
                            break;
                        case "Copyright":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Copyright);
                                row++;
                            }
                            break;
                        case "Cost Profile Group":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.CostProfileGroup);
                                row++;
                            }
                            break;
                        case "Country of Origin":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.CountryOfOrigin);
                                row++;
                            }
                            break;
                        case "Country of Origin Full":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, ItemService.RetrieveFullCountryOfOrigin(item.CountryOfOrigin));
                                row++;
                            }
                            break;
                        case "Country of Origin - Country Code":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, ItemService.RetrieveFullCountryOfOrigin(item.CountryOfOrigin) + " - " + item.CountryOfOrigin);
                                row++;
                            }
                            break;
                        case "Default Actual Cost CAD":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.DefaultActualCostCad);
                                row++;
                            }
                            break;
                        case "Default Actual Cost USD":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.DefaultActualCostUsd);
                                row++;
                            }
                            break;
                        case "Description":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Description);
                                row++;
                            }
                            break;
                        case "Direct Import":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.DirectImport);
                                row++;
                            }
                            break;
                        case "Dtc Price":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.DtcPrice);
                                row++;
                            }
                            break;
                        case "EAN":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Ean);
                                row++;
                            }
                            break;
                        case "Ecommerce ASIN":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceAsin);
                                row++;
                            }
                            break;
                        case "Ecommerce Bullet ALL":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, MergeBullets(item));
                                row++;
                            }
                            break;
                        case "Ecommerce Bullet ALL Bulleted":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, ModifyBulletedCopy(item));
                                row++;
                            }
                            break;
                        case "Ecommerce Bullet 1":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceBullet1);
                                row++;
                            }
                            break;
                        case "Ecommerce Bullet 2":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceBullet2);
                                row++;
                            }
                            break;
                        case "Ecommerce Bullet 3":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceBullet3);
                                row++;
                            }
                            break;
                        case "Ecommerce Bullet 4":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceBullet4);
                                row++;
                            }
                            break;
                        case "Ecommerce Bullet 5":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceBullet5);
                                row++;
                            }
                            break;
                        case "Ecommerce Components":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, ModifyComponents(item.EcommerceComponents));
                                row++;
                            }
                            break;
                        case "Ecommerce Components Count":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, ReturnNumberOfComponents(item.EcommerceComponents));
                                row++;
                            }
                            break;
                        case "Ecommerce Cost":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceCost);
                                row++;
                            }
                            break;
                        case "Ecommerce Country of Origin":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceCountryofOrigin);
                                row++;
                            }
                            break;
                        case "Ecommerce External ID":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceExternalId);
                                row++;
                            }
                            break;
                        case "Ecommerce External ID Type":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceExternalIdType);
                                row++;
                            }
                            break;
                        case "Ecommerce Generic Keywords":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, TrimSearchTerms(item.EcommerceGenericKeywords, customer));
                                row++;
                            }
                            break;
                        case "Ecommerce Image Path 1":
                            foreach (ItemObject item in items)
                            {
                                bool fileExists = CheckFtpFileExists(item.EcommerceImagePath1);
                                WriteCell(row, columnCount, SetImagePath(item.EcommerceImagePath1, fileExists));
                                row++;
                            }
                            break;
                        case "Ecommerce Image Path 2":
                            foreach (ItemObject item in items)
                            {
                                bool fileExists = CheckFtpFileExists(item.EcommerceImagePath2);
                                WriteCell(row, columnCount, SetImagePath(item.EcommerceImagePath2, fileExists));
                                row++;
                            }
                            break;
                        case "Ecommerce Image Path 3":
                            foreach (ItemObject item in items)
                            {
                                bool fileExists = CheckFtpFileExists(item.EcommerceImagePath3);
                                WriteCell(row, columnCount, SetImagePath(item.EcommerceImagePath3, fileExists));
                                row++;
                            }
                            break;
                        case "Ecommerce Image Path 4":
                            foreach (ItemObject item in items)
                            {
                                bool fileExists = CheckFtpFileExists(item.EcommerceImagePath4);
                                WriteCell(row, columnCount, SetImagePath(item.EcommerceImagePath4, fileExists));
                                row++;
                            }
                            break;
                        case "Ecommerce Image Path 5":
                            foreach (ItemObject item in items)
                            {
                                bool fileExists = CheckFtpFileExists(item.EcommerceImagePath5);
                                WriteCell(row, columnCount, SetImagePath(item.EcommerceImagePath5, fileExists));
                                row++;
                            }
                            break;
                        case "Ecommerce Item Height":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceItemHeight);
                                row++;
                            }
                            break;
                        case "Ecommerce Item Length":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceItemLength);
                                row++;
                            }
                            break;
                        case "Ecommerce Item Name":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceItemName);
                                row++;
                            }
                            break;
                        case "Ecommerce Item Name + Item Group":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceItemName + " " + DbUtil.UppercaseFirst(item.ItemGroup));
                                row++;
                            }
                            break;
                        case "Ecommerce Item Type Keywords":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceItemTypeKeywords);
                                row++;
                            }
                            break;
                        case "Ecommerce Item Weight":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceItemWeight);
                                row++;
                            }
                            break;
                        case "Ecommerce Item Weight (milliliters)":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, DbUtil.ConvertToMilliliters(item.EcommerceItemWeight));
                                row++;
                            }
                            break;
                        case "Ecommerce Item Width":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceItemWidth);
                                row++;
                            }
                            break;
                        case "Ecommerce Model Name":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceModelName);
                                row++;
                            }
                            break;
                        case "Ecommerce Package Height":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommercePackageHeight);
                                row++;
                            }
                            break;
                        case "Ecommerce Package Length":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommercePackageLength);
                                row++;
                            }
                            break;
                        case "Ecommerce Package Weight":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommercePackageWeight);
                                row++;
                            }
                            break;
                        case "Ecommerce Package Width":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommercePackageWidth);
                                row++;
                            }
                            break;
                        case "Ecommerce Page Qty":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommercePageQty);
                                row++;
                            }
                            break;
                        case "Ecommerce Parent Asin":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceParentAsin);
                                row++;
                            }
                            break;
                        case "Ecommerce Product Category":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceProductCategory);
                                row++;
                            }
                            break;
                        case "Ecommerce Product Description":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceProductDescription);
                                row++;
                            }
                            break;
                        case "Ecommerce Product Subcategory":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceProductSubcategory);
                                row++;
                            }
                            break;
                        case "Ecommerce Manufacturer Name":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceManufacturerName);
                                row++;
                            }
                            break;
                        case "Ecommerce Msrp":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceMsrp);
                                row++;
                            }
                            break;
                        case "Ecommerce Search Terms":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, TrimSearchTerms(item.EcommerceSubjectKeywords, customer));
                                row++;
                            }
                            break;
                        case "Ecommerce Size":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceSize);
                                row++;
                            }
                            break;
                        case "Ecommerce Subject Keywords":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, TrimSearchTerms(item.EcommerceSubjectKeywords, customer));
                                row++;
                            }
                            break;
                        case "Ecommerce Upc":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.EcommerceUpc);
                                row++;
                            }
                            break;
                        case "Genre 1":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Genre1);
                                row++;
                            }
                            break;
                        case "Genre 2":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Genre2);
                                row++;
                            }
                            break;
                        case "Genre 3":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Genre3);
                                row++;
                            }
                            break;
                        case "GPC":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Gpc);
                                row++;
                            }
                            break;
                        case "Image Path":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ImagePath);
                                row++;
                            }
                            break;
                        case "Image Path 1":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ImagePath);
                                row++;
                            }
                            break;
                        case "Image Path 2":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.AltImageFile1);
                                row++;
                            }
                            break;
                        case "Image Path 3":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.AltImageFile2);
                                row++;
                            }
                            break;
                        case "Image Path 4":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.AltImageFile3);
                                row++;
                            }
                            break;
                        case "Image Path 5":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.AltImageFile4);
                                row++;
                            }
                            break;
                        case "In Stock Date":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.InStockDate);
                                row++;
                            }
                            break;
                        case "Innerpack Height":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.InnerpackHeight);
                                row++;
                            }
                            break;
                        case "Innerpack Length":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.InnerpackLength);
                                row++;
                            }
                            break;
                        case "Innerpack Weight":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.InnerpackWeight);
                                row++;
                            }
                            break;
                        case "Innerpack Width":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.InnerpackWidth);
                                row++;
                            }
                            break;
                        case "Innerpack Qty":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.InnerpackQuantity);
                                row++;
                            }
                            break;
                        case "ISBN":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Isbn);
                                row++;
                            }
                            break;
                        case "Item Category":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ItemCategory);
                                row++;
                            }
                            break;
                        case "Item Family":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ItemFamily);
                                row++;
                            }
                            break;
                        case "Item Group":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ItemGroup);
                                row++;
                            }
                            break;
                        case "Item Height":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Height);
                                row++;
                            }
                            break;
                        case "Item Keywords":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ItemKeywords);
                                row++;
                            }
                            break;
                        case "Item ID":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ItemId);
                                row++;
                            }
                            break;
                        case "Item ID + EC":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ItemId + "EC");
                                row++;
                            }
                            break;
                        case "Item Length":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Length);
                                row++;
                            }
                            break;
                        case "Item Weight":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Weight);
                                row++;
                            }
                            break;
                        case "Item Width":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Width);
                                row++;
                            }
                            break;
                        case "Label Color":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Color);
                                row++;
                            }
                            break;
                        case "Language":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Language);
                                row++;
                            }
                            break;
                        case "License":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.License);
                                row++;
                            }
                            break;
                        case "License Begin Date":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.LicenseBeginDate);
                                row++;
                            }
                            break;
                        case "List Price (CAD)":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ListPriceCad);
                                row++;
                            }
                            break;
                        case "List Price (MXN)":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ListPriceMxn);
                                row++;
                            }
                            break;
                        case "List Price (USD)":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ListPriceUsd);
                                row++;
                            }
                            break;
                        case "Meta Description":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.MetaDescription);
                                row++;
                            }
                            break;
                        case "Mfg Source":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.MfgSource);
                                row++;
                            }
                            break;
                        case "MSRP":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Msrp);
                                row++;
                            }
                            break;
                        case "MSRP CAD":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.MsrpCad);
                                row++;
                            }
                            break;
                        case "MSRP MXN":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.MsrpMxn);
                                row++;
                            }
                            break;
                        case "Price Group (Product)":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.PricingGroup);
                                row++;
                            }
                            break;
                        case "Print On Demand":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.PrintOnDemand);
                                row++;
                            }
                            break;
                        case "Product Format":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ProductFormat);
                                row++;
                            }
                            break;
                        case "Product Group":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ProductGroup);
                                row++;
                            }
                            break;
                        case "Product ID":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ItemId);
                                row++;
                            }
                            break;
                        case "Product Id Translation":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ReturnProductIdTranslations());
                                row++;
                            }
                            break;
                        case "Product Line":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ProductLine);
                                row++;
                            }
                            break;
                        case "Product Qty":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ProductQty);
                                row++;
                            }
                            break;
                        case "Property":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Property);
                                row++;
                            }
                            break;
                        case "PS Status":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.PsStatus);
                                row++;
                            }
                            break;
                        case "SAT Code":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SatCode);
                                row++;
                            }
                            break;
                        case "Sell On All Posters":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnAllPosters);
                                row++;
                            }
                            break;
                        case "Sell On Amazon":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnAmazon);
                                row++;
                            }
                            break;
                        case "Sell On Amazon Seller Central":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnAmazonSellerCentral);
                                row++;
                            }
                            break;
                        case "Sell On Ecommerce":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnEcommerce);
                                row++;
                            }
                            break;
                        case "Sell On Fanatics":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnFanatics);
                                row++;
                            }
                            break;
                        case "Sell On Guitar Center":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnGuitarCenter);
                                row++;
                            }
                            break;
                        case "Sell On Hayneedle":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnHayneedle);
                                row++;
                            }
                            break;
                        case "Sell On Houzz":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnHouzz);
                                row++;
                            }
                            break;
                        case "Sell On Shop Trends":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnTrs);
                                row++;
                            }
                            break;
                        case "Sell On Target":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnTarget);
                                row++;
                            }
                            break;
                        case "Sell On Trends":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnTrends);
                                row++;
                            }
                            break;
                        case "Sell On Walmart":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnWalmart);
                                row++;
                            }
                            break;
                        case "Sell On Wayfair":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.SellOnWayfair);
                                row++;
                            }
                            break;
                        case "Short Description":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.ShortDescription);
                                row++;
                            }
                            break;
                        case "Size":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Size);
                                row++;
                            }
                            break;
                        case "Stats Code":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.StatsCode);
                                row++;
                            }
                            break;
                        case "Stats Name":
                            foreach (ItemObject item in items)
                            {
                                if (GlobalData.StatsCodes.ContainsKey(item.StatsCode))
                                {
                                    WriteCell(row, columnCount, GlobalData.StatsCodes[item.StatsCode]);
                                }
                                else WriteCell(row, columnCount, "");
                                row++;
                            }
                            break;
                        case "Tariff Code":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.TariffCode);
                                row++;
                            }
                            break;
                        case "Territory":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Territory);
                                row++;
                            }
                            break;
                        case "Title":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Title);
                                row++;
                            }
                            break;
                        case "Title + Trends International":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Title + " Trends International");
                                row++;
                            }
                            break;
                        case "UDEX":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Udex);
                                row++;
                            }
                            break;
                        case "UPC":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Upc);
                                row++;
                            }
                            break;
                        case "Variant Attribute Name":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, ReturnVariantAttributeName(item, customer));
                                row++;
                            }
                            break;
                        case "Variant Group Id":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, ReturnVariantGroupId(item.ItemId));
                                row++;
                            }
                            break;
                        case "Warranty":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.Warranty);
                                row++;
                            }
                            break;
                        case "Warranty Check":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.WarrantyCheck);
                                row++;
                            }
                            break;
                        case "Website Price":
                            foreach (ItemObject item in items)
                            {
                                WriteCell(row, columnCount, item.WebsitePrice);
                                row++;
                            }
                            break;
                    }
                }
            }
            // this was added to avoid errors when empty custom value was added //
            else
            {
                foreach (ItemObject item in items)
                {
                    WriteCell(row, columnCount, "");
                    row++;
                }
            }
        }

        /// <summary>
        ///     Retrieve a list of sorted customers
        /// </summary>
        public List<string> RetrieveExcelCustomers()
        {
            List<string> customers = new List<string>();
            foreach (KeyValuePair<string, string> cust in GlobalData.Customers)
            {
                customers.Add(cust.Key);
            }
            customers.Sort();
            return customers;
        }

        /// <summary>
        ///     Retrieves the coresponding layout id for the given layout
        /// </summary>
        /// <param name="layoutName"></param>
        /// <returns></returns>
        public string RetrieveExcelLayoutCustomer(string layoutName)
        {
            foreach (Layout layout in LayoutList)
            {
                if (layout.Name == layoutName)
                {
                    return layout.Customer;
                }
            }
            return "";
        }

        /// <summary>
        ///     Retrieves the coresponding layout id for the given layout
        /// </summary>
        /// <param name="layoutName"></param>
        /// <returns></returns>
        public int RetrieveExcelLayoutId(string layoutName)
        {
            return TemplateRepository.RetrieveExcelLayoutId(layoutName);
        }

        /// <summary>
        ///     Retrieves the coresponding productType for the given layout
        /// </summary>
        /// <param name="layoutName"></param>
        /// <returns></returns>
        public string RetrieveExcelLayoutProductType(string layoutName)
        {
            foreach (Layout layout in LayoutList)
            {
                if (layout.Name == layoutName)
                {
                    return layout.ProductType;
                }
            }
            return "";
        }

        /// <summary>
        ///     Retrieves a list of excel columns for the given layout name
        /// </summary>
        /// <param name="layoutName"></param>
        /// <returns></returns>
        public ObservableCollection<ExcelCell> RetrieveExcelLayoutData(string layoutName)
        {
            return TemplateRepository.RetrieveExcelLayoutData(layoutName);
        }

        /// <summary>
        ///     Retrieves a sorted list of existing excel layout names
        /// </summary>
        /// <returns></returns>
        public List<Layout> RetrieveExcelLayouts()
        {
            List<Layout> result = TemplateRepository.RetrieveExcelLayouts();
            return result;
        }

        /// <summary>
        ///     Retrieves a list of all existing layout names. Filters out templates based on Admin & ** (for universal excel sheets)
        /// </summary>
        /// <returns></returns>
        public List<string> RetrieveExcelLayoutNames()
        {
            List<string> excelNames = new List<string>();
            try
            {
                foreach (Layout layout in RetrieveExcelLayouts())
                {
                    if (layout.Name.Substring(0, 2) != "**" || GlobalData.UserRoles.Contains("ADMIN"))
                    {
                        excelNames.Add(layout.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError("Odin was unable to retrieve the excel layout name from the database.", ex.ToString());
            }
            excelNames.Sort();
            excelNames.Insert(0, "-NEW-");
            return excelNames;
        }

        /// <summary>
        ///     Retrieves a sorted list of available field values. Adds "-TEXT-" & "- EMPTY - "
        /// </summary>
        /// <returns></returns>
        public List<string> RetrieveFieldValues()
        {
            List<string> values =  TemplateRepository.RetrieveFieldValues();
            values.Insert(0, "-TEXT-");
            values.Insert(0, "- EMPTY -");
            return values;
        }

        /// <summary>
        ///     Checks pricing group to determine if batteries are needed for this product
        /// </summary>
        /// <param name="pricingGroup"></param>
        /// <returns></returns>
        public string ReturnBatteriesNeeded(string pricingGroup)
        {
            switch (pricingGroup)
            {
                case "PENPRO1PK":
                    return "Yes";
                case "PENGLOW1PK":
                    return "Yes";
                case "DisplaysTI":
                    return "Yes";
            }
            return "No";
        }

        /// <summary>
        ///     Checks pricing group to determine the battery cell type. Returns "Nonstandard battery" or ""
        /// </summary>
        /// <param name="pricingGroup"></param>
        /// <returns></returns>
        public string ReturnBatteryCellType(string pricingGroup)
        {
            switch (pricingGroup)
            {
                case "PENPRO1PK":
                    return "Nonstandard battery";
                case "PENGLOW1PK":
                    return "Nonstandard battery";
                case "DisplaysTI":
                    return "Nonstandard battery";
            }
            return "";
        }

        /// <summary>
        ///     Determines the Brand Name based on the product line provided. Return brand name or blank.
        /// </summary>
        /// <param name="prodLine"></param>
        /// <returns></returns>
        public string ReturnBrandName(string prodLine)
        {
            switch (prodLine)
            {
                case "Bookmarks":
                    return "Antioch";
                case "Gift Wrap":
                    return "Sandylion (SADE7)";
                case "Inkworks":
                    return "InkWorks (INKWA)";
                case "Journals":
                    return "Trends Journals";
                case "Paper Art":
                    return "SandyLion (SADE7)";
                case "Decals":
                    return "SandyLion (SADE7)";
                case "Office & Education":
                    return "SandyLion (SADE7)";
                case "Kid's Stickers":
                    return "SandyLion (SADE7)";
                case "Tattoos":
                    return "SandyLion (SADE7)";
            }
            return "";
        }

        /// <summary>
        ///     Returns a browse keyword based on the Ecommerce Product Subcategory.
        /// </summary>
        /// <param name="prodSubcategory"></param>
        /// <returns></returns>
        public string ReturnBrowseKeyword(string prodSubcategory)
        {
            switch (prodSubcategory)
            {
                case "Paper Crafts":
                    return "art-paper-products";
                case "Scrapbooking":
                    return "scrapbooking-supplies";                
            }
            return prodSubcategory;
        }
        
        /// <summary>
        ///     Iterates through Alphabet and assigns the alpha collumn name of the number given
        /// </summary>
        /// <param name="i">the column for alpha name retrieval</param>
        /// <returns>alpha collumn name</returns>
        public string ReturnColumLetter(int columnNumber)
        {
            string value = string.Empty;
            if (columnNumber <= Alphabet.Count())
            {
                value = Alphabet[columnNumber - 1];
            }
            else
            {
                int set = columnNumber / 26;
                int letter = columnNumber - (set * 26);
                if (letter == 0) { letter = 26; }
                if (columnNumber % 26 == 0)
                {
                    value = Alphabet[set - 2].ToString() + Alphabet[letter - 1];
                }
                else
                {
                    value = Alphabet[set - 1].ToString() + Alphabet[letter - 1];
                }
            }
            return value;
        }
        
        /// <summary>
        ///     Returns a hard coded short desctiption for the parent poster item
        /// </summary>
        /// <returns></returns>
        public string ReturnMagento2PosterShortDescription()
        {
            string result = string.Empty;
            result += "<p>Our posters are officially licensed and printed on FSC certified paper.</p>";
            result += "<p> Poster Options(when available):</p> ";
            result += "<ul> ";
            result += "<li> Unframed Poster(22.375” x 34”) </li> ";
            result += "<li> Unframed Premium Poster(22.375” x 34”) – Poster is printed on premium 210 GSM photo art gloss paper.</ li > ";
            result += "<li> Framed Poster(24.25” x 35.75” x 1”) – Poster is adhered to a sturdy 3 / 16” lightweight backer board to keep poster flat and smooth. The mounted poster is framed and ready to hang. Metal sawtooth hangers included.</li> ";
            result += "</ul> ";
            return result;
        }

        /// <summary>
        ///     Reads the number of each component contained in an item and returns the total amount of items.
        /// </summary>
        /// <param name="components"></param>
        /// <returns></returns>
        public string ReturnNumberOfComponents(string components)
        {
            int componentCount = 0;
            string[] componentList = components.Split('^');
            foreach(string x in componentList)
            {
                if(x.Contains("("))
                {
                    string[] y = x.Split('(');
                    string z = y[1].Replace(")", "");
                    if (DbUtil.IsNumber(z))
                    {
                        componentCount += Convert.ToInt32(z);
                    }
                    else
                    {
                        componentCount += 1;
                    }
                }
                else
                {
                    componentCount += 1;
                }
            }

            return componentCount.ToString();
        }

        /// <summary>
        ///     Returns the shoptrends poster option that coresponds with the itemid
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public string ReturnPosterOption(string itemId)
        {
            if(itemId.Contains("BLK22X34"))
            {
                return "BLACKFRAME";
            }
            else if (itemId.Contains("SIL22X34"))
            {
                return "SILVERFRAME";
            }
            else if (itemId.Contains("POD"))
            {
                return "PREMIUM";
            }
            else if (itemId.Contains("RP"))
            {
                return "UNFRAMED";
            }
            return "";
        }

        /// <summary>
        ///     Combines the license and property values for website if property exists.
        /// </summary>
        /// <param name="license"></param>
        /// <param name="property"></param>
        /// <returns>Valid Magento property value</returns>
        public string ReturnProperty(string license, string property)
        {
            if (string.IsNullOrEmpty(property))
            {
                return "";
            }
            else
            {
                return license + ": " + property;
            }
        }

        public string ReturnRequestNum()
        {
            return Convert.ToString(RequestNum);
        }

        /// <summary>
        ///     Returns the Variant Attribute Name based on the item it prefix
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public string ReturnVariantAttributeName(ItemObject item, string customer)
        {
            if(item.ProductFormat== "Mount Bundle" || item.ProductFormat== "Clip Bundle")
            {
                if(item.ProductFormat=="Mount Bundle")
                {
                    return "Poster & Mount Bundle";
                }
                if (item.ProductFormat == "Clip Bundle")
                {
                    return "'Poster & Clip Bundle";
                }
            }

            if (item.ItemId.Substring(0, 2) == "FR")
            {
                if (item.ItemId.Contains("SIL"))
                {
                    return "Silver Framed Version";
                }
                else if (item.ItemId.Contains("BLK"))
                {
                    return "Black Framed Version";
                }
                else
                {
                    return "Framed Version";
                }
            }
            else if (item.ItemId.Substring(0, 3) == "POD")
            {
                if (item.ProductFormat == "POD Mount Bundle")
                {
                    return "Premium Poster & Clip Bundle";
                }
                else if (item.ProductFormat == "POD Clip Bundle")
                {
                    return "Premium Poster & Mount Bundle";
                }
                if (customer == "Walmart")
                {
                    return "Premium Unframed Version";
                }
                return "Premium Unframed";
            }
            else return  "Unframed Version";
        }

        /// <summary>
        ///     Strips the prefixes and suffixes off the item id and returns the varient group id
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public string ReturnVariantGroupId(string itemId)
        {
            List<string> filters = new List<string>();
            string result = itemId.Trim();
            foreach (string x in GlobalData.VariantGroupExclusionOptions)
            {
                filters.Add(x.Trim());
            }
            filters.Sort((a, b) => b.Length.CompareTo(a.Length));
            foreach (string x in filters)
            {
                result = result.Replace(x, "");
            }
            return result;
        }

        #endregion // Retrieval Methods
        
        /// <summary>
        ///     Submit data for a request submission
        /// </summary>
        /// <param name="Items"></param>
        public void SubmitRequest(ObservableCollection<ItemObject> items, string status, string website, string comment)
        {
            RequestNum = Convert.ToInt32(RequestRepository.RetrieveSubmitRequestNumber());
            RequestRepository.SubmitRequest(items, status, comment, website, RequestNum);
        }
        
        /// <summary>
        ///     Write item data into an excel cell with the given parameters
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        public void WriteCell(int row, int column, string value)
        {
                worksheet.Cells[row, column].NumberFormat = "@";
                worksheet.Cells[row, column] = value;
        }

        /// <summary>
        ///     Writes out the first row it headers into the template file
        /// </summary>
        /// <param name="headers"></param>
        public void WriteHeaders(List<string> headers)
        {
            int count = 1;
            foreach (string header in headers)
            {
                worksheet.Cells[1, count] = header;
                count++;
            }
        }

        /// <summary>
        ///     Writes out the data for the magento upload file
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="requestId"></param>
        /// <param name="requestType"></param>
        public void WriteMagentoCsv(ObservableCollection<ItemObject> itemList, string requestId, string requestType)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            List<string> CSV_Add = new List<string>();
            List<string> CSV_Add_Image = new List<string>();
            List<string> CSV_REMOVE = new List<string>();
            CSV_Add.Add(CreateMagentoHeaders());

            CSV_REMOVE.Add("sku,status");
            CSV_Add_Image.Add("sku, image, thumbnail, small_image");
            foreach (ItemObject item in itemList)
            {
                if ((requestType == "Add") || (requestType == "Update"))
                {
                    string[] imageSections = item.ImagePath.Split('\\');
                    string imageName = imageSections[imageSections.Length - 1];
                    List<string> cats = this.NewCategories(item.Category, item.Category2, item.Category3);

                    // If item is being setup on US store
                    if (item.Territory.Contains("USA") || item.Territory.Contains("WW"))
                    {
                        string newString = WriteMagentoMainLine(item, "USA", requestType);
                        CSV_Add.Add(newString);

                        // If item is also being setup on canadian store
                        if (item.Territory.Contains("CAN") || item.Territory.Contains("WW"))
                        {
                            string secondaryLine = WriteMagentoSecondaryLine(item, "CAN", requestType);
                            CSV_Add.Add(secondaryLine);
                        }
                        else
                        {
                            string canPrice = "";
                            canPrice += ","; /* "sku" */
                            canPrice += ","; /* "_store" */
                            canPrice += ","; /* "_attribute_set" */
                            canPrice += ","; /* _type */
                            canPrice += "\"All Products\",";/* _category */
                            canPrice += "\"Default Category\""; /* _root_category */
                            CSV_Add.Add(canPrice);
                        }
                    }
                    // If item is only being setup on canadian store
                    else if (item.Territory.Contains("CAN"))
                    {
                        string newString = WriteMagentoMainLine(item, "CAN", requestType);
                        CSV_Add.Add(newString);
                    }
                    if (cats.Count > 0)
                    {
                        int count = 0;
                        while (count < cats.Count)
                        {
                            string meow = cats[count].Replace("\"", "\'\'");
                            string catString = "";
                            catString += ","; /* "sku" */
                            catString += ","; /* "_store" */
                            catString += ","; /* "_attribute_set" */
                            catString += ","; /* _type */
                            catString += "\"" + DbUtil.ReplaceQuotes(meow) + "\",";/* _category */
                            catString += "\"Default Category\","; /* _root_category */

                            CSV_Add.Add(catString);
                            count++;
                        }
                    }
                    CSV_Add_Image.Add(item.ItemId + ", /" + imageName + ", /" + imageName + ", /" + imageName);
                }

                else if (requestType == "Remove")
                {
                    string newString = string.Empty;
                    newString = item.ItemId.Trim() + ",2";
                    CSV_REMOVE.Add(newString);
                }

                if (CSV_REMOVE.Count > 1)
                {
                    int RemoveLength = CSV_REMOVE.Count;
                    string filePath = desktop + @"\Request-" + "-" + requestId + "_Remove_New.csv";
                    string delimiter = ",";

                    StringBuilder sb = new StringBuilder();
                    for (int index = 0; index < RemoveLength; index++)
                        sb.AppendLine(string.Join(delimiter, CSV_REMOVE[index]));

                    File.WriteAllText(filePath, sb.ToString());
                }
                if (CSV_Add.Count > 1)
                {
                    string csvFilePath = desktop + @"\Request-" + "-" + requestId + "_Au_New.csv";
                    string csvImageFilePath = desktop + @"\Request-" + "-" + requestId + "_Au_Images.csv";
                    string delimiter = ",";
                    StringBuilder sb = new StringBuilder();
                    StringBuilder sbl = new StringBuilder();
                    StringBuilder sbi = new StringBuilder();
                    for (int index = 0; index < CSV_Add.Count; index++)
                    {
                        sb.AppendLine(string.Join(delimiter, CSV_Add[index]));
                    }
                    for (int index = 0; index < CSV_Add_Image.Count; index++)
                    {
                        sbi.AppendLine(string.Join(delimiter, CSV_Add_Image[index]));
                    }
                    File.WriteAllText(csvFilePath, sb.ToString());
                    File.WriteAllText(csvImageFilePath, sbi.ToString());
                }
            } // End foreach (Request request in requests)
        }

        /// <summary>
        ///     Writes out the data for the magento upload file
        /// </summary>
        /// <param name="itemList"></param>
        /// <param name="requestId"></param>
        /// <param name="requestType"></param>
        public void WriteMagento2Csv(ObservableCollection<ItemObject> itemList, string requestId, string requestType)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            List<string> CSV_Add = new List<string>
            {
                CreateMagento2Headers()
            };

            List<string> parentStrings = new List<string>();

            if ((requestType == "Add") || (requestType == "Update"))
            {
                List<string> idCores = new List<string>();
                foreach (ItemObject item in itemList)
                {
                    CSV_Add.Add(WriteMagento2ChildLine(item));
                    string title = (string.IsNullOrEmpty(item.EcommerceItemName)) ? DbUtil.UppercaseFirst(item.Description) : item.EcommerceItemName;
                    ItemService.InsertMarketplaceCustomerProducts(item.ItemId, title, "000000000146515");
                    ItemService.UpdateOnSite(item, "SHOPTRENDS.COM");
                }
                foreach (ItemObject item in itemList)
                {
                    string idCore = ItemService.RetrieveItemIdCore(item.ItemId);
                    if (!idCores.Contains(idCore))
                    {
                        List<string> childProducts = new List<string>();

                        // Check all existing active related products
                        foreach (string x in ItemService.RetrieveRelatedProductIds(item.ItemId))
                        {
                            // Add all products that are currently on the site or in the current upload
                            if (ItemService.CheckOnSite(x, "SHOPTRENDS.COM") || itemList.Any(y => y.ItemId == x))
                            {
                                if (!childProducts.Contains(x))
                                {
                                    childProducts.Add(x);
                                }
                            }
                        }
                        CSV_Add.Add(WriteMagento2ParentLine(item, childProducts));
                        idCores.Add(idCore);
                        ItemService.InsertMarketplaceCustomerProducts("POSTER" + idCore, item.Title, "000000000146515");
                    }
                }
                if (CSV_Add.Count > 1)
                {
                    string csvFilePath = desktop + @"\Request-" + "-" + requestId + "_Add.csv";
                    string delimiter = ",";
                    StringBuilder sb = new StringBuilder();
                    StringBuilder sbl = new StringBuilder();
                    StringBuilder sbi = new StringBuilder();
                    for (int index = 0; index < CSV_Add.Count; index++)
                    {
                        sb.AppendLine(string.Join(delimiter, CSV_Add[index]));
                    }
                    File.WriteAllText(csvFilePath, sb.ToString());
                }
            } // End foreach (Request request in requests)
        }

        public string WriteMagentoMainLine(ItemObject item, string territory, string requestType)
        {
            string result = string.Empty;
            string newdate = string.Empty;
            string store = string.Empty;
            string website = string.Empty;
            string msrp = string.Empty;
            string price = string.Empty;
            if (territory == "USA")
            {
                store = "default,"; /* "_store" */
                website = "\"base\","; /* _product_websites */
                msrp = "\"" + item.Msrp.Trim() + "\","; /* msrp */
                price = "\"" + ItemService.ReturnItemPrice(item.WebsitePrice.Trim(), item.ProductQty) + "\","; /* price */
            }
            else if(territory == "CAN")
            {
                store = "\"can_view\","; /* "_store" */
                website = "\"can_website\","; /* _product_websites */
                msrp = "\"" + item.MsrpCad.Trim() + "\","; /* msrp */;
                price = "\"" + ItemService.ReturnItemPrice(item.ListPriceCad.Trim(), item.ProductQty) + "\","; /* price */
            }


            List<string> cats = this.NewCategories(item.Category, item.Category2, item.Category3);
            // Replace size placeholder with blank
            if (item.Size == "'' x '' x ''")
            {
                item.Size = "";
            }

            if (!string.IsNullOrEmpty(item.NewDate))
            {
                DateTime dt = Convert.ToDateTime(item.NewDate);
                newdate = dt.Month.ToString() + "/" + dt.Day.ToString() + "/" + dt.Year.ToString() + " 12:00";
            }
            else
            {
                DateTime dt = DateTime.Now;
                newdate = dt.Month.ToString() + "/" + dt.Day.ToString() + "/" + dt.Year.ToString() + " 12:00";

            }
            string inStockDate = string.IsNullOrEmpty(item.InStockDate) ? newdate : item.InStockDate;
            //remove zeros at the end of dimensions
            string format = DbUtil.ReplaceCharacters(item.ProductFormat).Trim();
            string title = DbUtil.ReplaceQuotes(item.Title);
            string metaDescription = DbUtil.ReplaceQuotes(item.MetaDescription);

            result += "\"" + item.ItemId.Trim() + "\","; /* "sku" */
            result += store; /* "_store" */
            result += "\"" + ItemService.SetProductType(cats) + "\","; /* "_attribute_set" */
            result += "\"simple\","; /* _type */
            result += ",";/* _category */
            result += "\"Default Category\","; /* _root_category */
            result += website; /* _product_websites */
            result += ","; /* color */
            result += ","; /* cost */
            result += ","; /* country_of_manufacture */
            if (item.OnSite == "Y")
            {
                result += "\"" + newdate + "\","; /* created_at */
            }
            else
            {
                result += ","; /* created_at */
            }
            result += "\"0\","; /* has_options */
            result += "\"" + item.Height + "\","; /* height */
            result += "\"" + DbUtil.OrderLanguage(item.Language) + "\","; /* language */
            result += "\"" + item.Copyright + "\","; /* legal */
            result += "\"" + item.License.Trim() + "\","; /* license */
            result += "\"" + ReturnProperty(item.License.Trim(), item.Property.Trim()) + "\","; /* property */
            result += ","; /* "license_end_date" */
            result += "\"" + metaDescription.Trim() + "\","; /* meta_description */
            result += "\"" + ModifyKeywords(item.ItemId, item.ItemKeywords).Trim() + "\","; /* meta_keyword */
            result += "\"" + title.Trim() + "\","; /* meta_title */
            result += msrp; /* msrp */
            result += "\"" + item.MsrpCad.Trim() + "\","; /* msrpcan */
            result += "\"Use config\","; /* msrp_display_actual_price_type */
            result += "\"Use config\","; /* msrp_enabled */
            result += "\"" + title.Trim() + "\","; /* name */
            if (item.OnSite == "N")
            {
                result += "\"" + DateTime.Today.ToString() + "\","; /* date_added */
                result += "\"" + DateTime.Today.ToString() + "\","; /* news_from_date */
                result += "\"" + ItemService.RetrieveNewToDate("") + "\","; /* news_to_date */
            }
            else
            {
                result += ","; /* date_added */
                result += ","; /* news_from_date */
                result += ","; /* news_to_date */
            }
            result += "\"Product Info Column\","; /* options_container */
            result += price; /* price */
            result += "\"" + ItemService.ReturnItemPrice(item.ListPriceCad.Trim(), item.ProductQty) + "\","; /* pricecan */
            result += "\"0\","; /* required_options */
            result += "\"" + item.ShortDescription.Trim() + "\","; /* short_description */
            result += "\"1\","; /* status */
            result += "\"0\","; /* tax_class_id */
            result += "\"" + DbUtil.OrderTerritory(item.Territory.Trim()) + "\","; /* territory */
            result += "\"" + item.Upc.Trim() + "\","; /* upc */
            result += ","; /* updated_at */
            result += "\"" + item.ItemId.Trim() + "\","; /* url_key */
            result += "\"" + item.ItemId.Trim() + ".html\","; /* url_path */
            result += "\"4\",";/* visibility */
            result += "\"" + item.Weight.Trim() + "\","; /* weight */
            result += "\"" + item.Width + "\","; /* width */
            result += "\"" + item.Size.Replace("\"", "''").Trim() + "\","; /* size */
            result += "\"0\","; /* min_qty */
            result += "\"" + item.ProductQty.Trim() + "\",";  /* pack_qty */
            result += "\"1\","; /* use_config_min_qty */
            result += "\"0\","; /* is_qty_decimal */
            result += "\"0\","; /* backorders */
            result += "\"1\","; /* use_config_backorders */
            result += "\"1\","; /* min_sale_qty */
            result += "\"1\","; /* use_config_min_sale_qty */
            result += "\"0\","; /* max_sale_qty */
            result += "\"1\","; /* use_config_min_qty */
            result += "\"0\","; /* is_in_stock */
            result += "\"1\","; /* "notify_stock_qty" */
            result += "\"1\","; /* "use_config_notify_stock_qty" */
            result += "\"0\","; /* "manage_stock" */
            result += "\"1\","; /* "use_config_manage_stock" */
            result += "\"1\","; /* "stock_status_changed_auto" */
            result += "\"1\","; /* "use_config_qty_increments" */
            result += "\"0\","; /* qty_increments */
            result += "\"1\",";/* use_config_enable_qty_inc */
            result += "\"0\","; /* enable_qty_increments */
            result += "\"0\","; /*is_decimal_divided */
            result += "\"\","; /*_links_related_sku */
            result += "\"0\","; /*_links_related_position */
            result += "\"88\","; /* _media_attribute_id */
            result += "\"2\","; /* _media_position */
            result += "\"0\","; /* _media_is_disabled */
            if (item.OnSite != "Y")
            {
                result += "\"0\","; /* qty */
                result += "\"" + "1/1/1970" + "\""; /* "license_begin_date" */
            }
            return result;
        }

        /// <summary>
        ///     Writes the line for the child simple product
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string WriteMagento2ChildLine(ItemObject item)
        {
            string result = string.Empty;
            string title = (string.IsNullOrEmpty(item.EcommerceItemName)) ? item.Title : item.EcommerceItemName;
            title = (string.IsNullOrEmpty(item.TitleOverride)) ? item.Title : item.TitleOverride;
            string itemKeywords = (string.IsNullOrEmpty(item.ItemKeywordsOverride)) ? item.ItemKeywordsOverride : item.ItemKeywordsOverride;

            string dateAdded = item.Status == "Add" ? DateTime.Now.ToShortDateString() : "";

            result += "\"" + item.ItemId.Trim() + "\","; /* A */
            result += ","; /* B */
            result += "\"" + "Default" + "\","; /* C */
            result += "\"simple\","; /* D */
            result += ","; /* E */
            result += "\"base\","; /* F */
            result += "\"" + item.Title.Trim() + "\","; /* G */
            result += ","; /* H */
            result += ","; /* I */
            result += "\"" + item.Weight.Trim() + "\","; /* J */
            result += "\"" + "1" + "\","; /* K */
            result += "\"" + "Taxable Goods" + "\","; /* L */
            result += "\"" + "Not Visible Individually" + "\","; /* M */
            result += "\"" + item.DtcPrice + "\","; /* N */
            result += ","; /* O */
            result += ","; /* P */
            result += ","; /* Q */
            result += "\"" + FormatMagento2Url(title,item.ItemId) + "\","; /* R */
            result += "\"" + title + "\","; /* S */
            result += "\"" + FormatMagento2Keywords(item) + "\","; /* T */
            result += "\"" + title + "\","; /* U */
            result += "\"" + ItemService.ReturnImageName(item.ItemId, 1) + "\","; /* V */
            result += ","; /* W */
            result += "\"" + ItemService.ReturnImageName(item.ItemId, 1) + "\","; /* X */
            result += ","; /* Y */
            result += "\"" + ItemService.ReturnImageName(item.ItemId, 1) + "\","; /* Z */
            result += ","; /* AA */
            result += "\"" + ItemService.ReturnImageName(item.ItemId, 1) + "\","; /* AB */
            result += ","; /* AC */
            result += "\"" + dateAdded + "\","; /* AD */
            result += "\"" + DateTime.Now.ToShortDateString() + "\","; /* AE */
            result += ","; /* AF */
            result += ","; /* AG */
            result += "\"" + "Block after Info Column" + "\","; /* AH */
            result += ","; /* AI */
            result += ","; /* AJ */
            result += ","; /* AK */
            result += "\"" + "No" + "\","; /* AL */
            result += ","; /* AM */
            result += ","; /* AN */
            result += ","; /* AO */
            result += ","; /* AP */
            result += "\"" + "Product -- Full Width" + "\","; /* AQ */
            result += ","; /* AR */
            result += "\"" + "Use config" + "\","; /* AS */
            result += "\"" + "United States" + "\","; ; /* AT */
            result += "\"" + FormatAdditionalAttributes(item,true) + "\","; /* AU */
            result += "\"" + "1" + "\","; /* AV */
            result += "\"" + "0" + "\","; /* AW */
            result += "\"" + "1" + "\","; /* AX */
            result += "\"" + "0" + "\","; /* AY */
            result += "\"" + "0" + "\","; /* AZ */
            result += "\"" + "1" + "\","; /* BA */
            result += "\"" + "1" + "\","; /* BB */
            result += "\"" + "1" + "\","; /* BC */
            result += "\"" + "10000" + "\","; /* BD */
            result += "\"" + "1" + "\","; /* BE */
            result += "\"" + "1" + "\","; /* BF */
            result += "\"" + "1" + "\","; /* BG */
            result += "\"" + "1" + "\","; /* BH */
            result += "\"" + "1" + "\","; /* BI */
            result += "\"" + "1" + "\","; /* BJ */
            result += "\"" + "1" + "\","; /* BK */
            result += "\"" + "1" + "\","; /* BL */
            result += "\"" + "1" + "\","; /* BM */
            result += "\"" + "0" + "\","; /* BN */
            result += "\"" + "0" + "\","; ; /* BO */
            result += "\"" + "0" + "\","; ; /* BP */
            result += "\"" + "0" + "\","; /* BQ */
            result += "\"" + "1" + "\","; /* BR */
            result += ","; /* BS */
            result += ","; /* BT */
            result += ","; /* BU */
            result += ","; /* BV */
            result += ","; /* BW */
            result += ","; /* BX */
            result += "\"" + FormatAdditionalImages(item) + "\","; /* BY */
            result += ","; /* BZ */
            result += ","; /* CA */
            result += ","; /* CB */
            result += ","; /* CC */
            result += ","; /* CD */
            result += ","; /* CE */
            result += ","; /* CF */
            result += ","; /* CG */
            result += ","; /* CH */
            result += ","; /* CI" */
            result += ","; /* CJ" */
            result += ","; /* CK" */
            result += ","; /* CL" */
            result += ","; /* CM" */
            result += ","; /* CN" */
            result += ","; /* CO" */
            result += ","; /* CP" */
            result += ","; /* CQ" */
            result += ","; /* CR" */
            result += ","; /* CS" */
            result += ","; /* CT" */
            result += ","; /* CU" */
            result += ","; /* CV" */
            result += ","; /* CW" */
            result += ""; /* CX" */

            return result;
        }

        /// <summary>
        ///     Writes the line for the parent configurable product
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public string WriteMagento2ParentLine(ItemObject item, List<string> childProducts)
        {
            string result = string.Empty;
            string title = (string.IsNullOrEmpty(item.EcommerceItemName)) ? FormatMagento2Title(item.Title) : FormatMagento2Title(item.EcommerceItemName);
            if (!string.IsNullOrEmpty(item.TitleOverride))
            {
                title = item.TitleOverride;
            }
            string itemId = "POSTER" + ItemService.RetrieveItemIdCore(item.ItemId);
            string dateAdded = (item.Status == "Add") ? DateTime.Now.ToShortDateString() : "";

            result += "\"" + itemId.Trim() + "\","; /* A */
            result += ","; /* B */
            result += "\"" + "Default" + "\","; /* C */
            result += "\"configurable\","; /* D */
            result += "\"" + FormatMagento2Categories(item) + "\","; /* E */
            result += "\"base\","; /* F */
            result += "\"" + item.Title.Trim() + "\","; /* G */
            result += ","; /* H */
            result += "\"" + ReturnMagento2PosterShortDescription() + "\","; /* I */
            result += ","; /* J */
            result += "\"" + "1" + "\","; /* K */
            result += "\"" + "Taxable Goods" + "\","; /* L */
            result += "\"" + "Catalog, Search" + "\","; /* M */
            result += ","; /* N */
            result += ","; /* O */
            result += ","; /* P */
            result += ","; /* Q */
            result += "\"" + FormatMagento2Url(title, itemId) + "\","; /* R */
            result += "\"" + title + "\","; /* S */
            result += "\"" + FormatMagento2Keywords(item) + "\","; /* T */
            result += "\"" + title + "\","; /* U */
            result += "\"" + ItemService.ReturnImageName(itemId, 1) + "\","; /* V */
            result += ","; /* W */
            result += "\"" + ItemService.ReturnImageName(itemId, 1) + "\","; /* X */
            result += ","; /* Y */
            result += "\"" + ItemService.ReturnImageName(itemId, 1) + "\","; /* Z */
            result += ","; /* AA */
            result += "\"" + ItemService.ReturnImageName(itemId, 1) + "\","; /* AB */
            result += ","; /* AC */
            result += "\"" + dateAdded + "\","; /* AD */
            result += "\"" + DateTime.Now.ToShortDateString() + "\","; /* AE */
            result += ","; /* AF */
            result += ","; /* AG */
            result += "\"" + "Block after Info Column" + "\","; /* AH */
            result += ","; /* AI */
            result += ","; /* AJ */
            result += ","; /* AK */
            result += "\"" + "Use Config" + "\","; /* AL */
            result += ","; /* AM */
            result += ","; /* AN */
            result += ","; /* AO */
            result += ","; /* AP */
            result += "\"" + "Product -- Full Width" + "\","; /* AQ */
            result += ","; /* AR */
            result += "\"" + "Use Config" + "\","; /* AS */
            result += "\"" + "United States" + "\","; ; /* AT */
            result += "\"" + FormatAdditionalAttributes(item, false) + "\","; /* AU */
            result += "\"" + "0" + "\","; /* AV */
            result += "\"" + "0" + "\","; /* AW */
            result += "\"" + "1" + "\","; /* AX */
            result += "\"" + "0" + "\","; /* AY */
            result += "\"" + "0" + "\","; /* AZ */
            result += "\"" + "1" + "\","; /* BA */
            result += "\"" + "1" + "\","; /* BB */
            result += "\"" + "1" + "\","; /* BC */
            result += "\"" + "10000" + "\","; /* BD */
            result += "\"" + "1" + "\","; /* BE */
            result += "\"" + "1" + "\","; /* BF */
            result += "\"" + "1" + "\","; /* BG */
            result += "\"" + "1" + "\","; /* BH */
            result += "\"" + "1" + "\","; /* BI */
            result += "\"" + "1" + "\","; /* BJ */
            result += "\"" + "1" + "\","; /* BK */
            result += "\"" + "1" + "\","; /* BL */
            result += "\"" + "1" + "\","; /* BM */
            result += "\"" + "0" + "\","; /* BN */
            result += "\"" + "0" + "\","; /* BO */
            result += "\"" + "0" + "\","; /* BP */
            result += "\"" + "0" + "\","; /* BQ */
            result += "\"" + "1" + "\","; /* BR */
            result += ","; /* BS */
            result += ","; /* BT */
            result += ","; /* BU */
            result += ","; /* BV */
            result += ","; /* BW */
            result += ","; /* BX */
            result += ","; /* BY */
            result += ","; /* BZ */
            result += ","; /* CA */
            result += ","; /* CB */
            result += ","; /* CC */
            result += ","; /* CD */
            result += ","; /* CE */
            result += ","; /* CF */
            result += ","; /* CG */
            result += ","; /* CH */
            result += ","; /* CI" */
            result += ","; /* CJ" */
            result += ","; /* CK" */
            result += ","; /* CL" */
            result += ","; /* CM" */
            result += ","; /* CN" */
            result += ","; /* CO" */
            result += ","; /* CP" */
            result += ","; /* CQ" */
            result += ","; /* CR" */
            result += ","; /* CS" */
            result += ","; /* CT" */
            result += ","; /* CU" */
            result += ","; /* CV" */
            result += "\"" + SetConfigurableVariations(childProducts) + "\","; /* CW" */
            result += "\"" + SetConfigurableVariationLabels(item) + "\""; /* CX" */

            return result;
        }

        public string WriteMagentoSecondaryLine(ItemObject item, string territory, string requestType)
        {
            string result = string.Empty;
            string newdate = string.Empty;
            string store = string.Empty;
            string website = string.Empty;
            string msrp = string.Empty;
            string price = string.Empty;
            if (territory == "CAN")
            {
                store = "\"can_view\","; /* "_store" */
                website = "\"can_website\","; /* _product_websites */
                msrp = "\"" + item.MsrpCad.Trim() + "\","; /* msrp */;
                price = "\"" + ItemService.ReturnItemPrice(item.ListPriceCad.Trim(), item.ProductQty) + "\","; /* price */
            }
            result += ","; /* "sku" */
            result += store; /* "_store" */
            result += ","; /* "_attribute_set" */
            result += "\"simple\","; /* _type */
            result += "\"All Products\",";/* _category */
            result += "\"Default Category\","; /* _root_category */
            result += website; /* _product_websites */
            result += ","; /* color */
            result += ","; /* cost */
            result += ","; /* country_of_manufacture */
            result += ","; /* created_at */
            result += ","; /* has_options */
            result += ","; /* height */
            result += ","; /* language */
            result += ","; /* legal */
            result += ","; /* license */
            result += ","; /* property */
            result += ","; /* license_end_date */
            result += ","; /* meta_description */
            result += ","; /* meta_keyword */
            result += ","; /* meta_title */
            result += msrp; /* msrp */
            result += ","; /* msrpcan */
            result += ","; /* msrp_display_actual_price_type */
            result += ","; /* msrp_enabled */
            result += ","; /* name */
            result += ","; /* date_added */
            result += ","; /* news_from_date */
            result += ","; /* news_to_date */
            result += ","; /* options_container */
            result += price; /* price */
            result += "\"" + ItemService.ReturnItemPrice(item.ListPriceCad.Trim(), item.ProductQty) + "\","; /* pricecan */
            result += "\"0\","; /* required_options */
            result += "\"" + item.ShortDescription.Trim() + "\","; /* short_description */
            result += "\"1\","; /* status */
            result += "\"4\","; /* tax_class_id */
            return result;
        }

        #endregion // Methods

        #region Constructor

        /// <summary>
        ///     Constructs the excel service
        /// </summary>
        /// <param name="isTest"></param>
        /// <param name="itemService"></param>
        /// <param name="requestReposiory"></param>
        public ExcelService(bool isTest, ItemService itemService, OptionService optionService, ITemplateRepository templateRepository, IRequestRepository requestReposiory)
        {
            if (!GlobalData.FtpUserexceptions.Contains(GlobalData.UserName))
            {
                if (FtpService == null)
                {
                    this.FtpService = new FtpService();
                }
                this.ExistingFiles = this.FtpService.ReturnExistingImageFiles();                
            }
            else
            {
                this.FtpService = null;
            }
            this.ItemService = itemService ?? throw new ArgumentNullException("itemService");
            this.RequestRepository = requestReposiory ?? throw new ArgumentNullException("requestReposiory");
            this.OptionService = optionService ?? throw new ArgumentNullException("optionService");
            this.TemplateRepository = templateRepository ?? throw new ArgumentNullException("templateRepository");
            PopulateTemplateHeaders();
            this.LayoutList = RetrieveExcelLayouts();
        }

        #endregion // Constructor
    }
}
