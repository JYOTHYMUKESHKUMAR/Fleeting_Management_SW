using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using DotNetCoreMVCApp.Models;
using System.Collections.Generic;

namespace DotNetCoreMVCApp.Web.Controllers
{
    public class OilInformationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OilInformationController> _logger;

        public OilInformationController(ApplicationDbContext context, ILogger<OilInformationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: OilInformation/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var oilInformation = await _context.OilInformationSet
                    .Where(o => !o.IsDeleted)
                    .OrderByDescending(o => o.OilChangeDate)
                    .Select(o => new OilInformationViewModel
                    {
                        Id = o.Id,
                        LicensePlate = o.LicensePlate.Trim(),
                        OilType = o.OilType,
                        OilBrand = o.OilBrand,
                        OilPrice = o.OilPrice,
                        CurrentOdometer = o.CurrentOdometer,
                        NextOilChangeOdometer = o.NextOilChangeOdometer,
                        OilChangeDate = o.OilChangeDate,
                        Notes = o.Notes,
                        CreatedBy = o.CreatedBy,
                        CreatedOn = o.CreatedOn,
                        UpdatedBy = o.UpdatedBy,
                        UpdatedOn = o.UpdatedOn
                    })
                    .ToListAsync();

                return View(oilInformation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching oil information");
                return StatusCode(500, "An error occurred while fetching oil information. Please try again later.");
            }
        }

        // GET: OilInformation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var oilInfo = await _context.OilInformationSet
                    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

                if (oilInfo == null)
                {
                    return NotFound();
                }

                var viewModel = new OilInformationViewModel
                {
                    Id = oilInfo.Id,
                    LicensePlate = oilInfo.LicensePlate.Trim(), 
                    OilType = oilInfo.OilType,
                    OilBrand = oilInfo.OilBrand,
                    OilPrice = oilInfo.OilPrice,
                    CurrentOdometer = oilInfo.CurrentOdometer,
                    NextOilChangeOdometer = oilInfo.NextOilChangeOdometer,
                    OilChangeDate = oilInfo.OilChangeDate,
                    Notes = oilInfo.Notes,
                    CreatedBy = oilInfo.CreatedBy,
                    CreatedOn = oilInfo.CreatedOn,
                    UpdatedBy = oilInfo.UpdatedBy,
                    UpdatedOn = oilInfo.UpdatedOn,
                    OilTypeList = GetOilTypeList()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching oil information details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Error loading details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: OilInformation/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                DateTime qatarTime;
                try
                {
                    TimeZoneInfo qatarTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arab Standard Time");
                    qatarTime = TimeZoneInfo.ConvertTime(DateTime.Now, qatarTimeZone);
                }
                catch (TimeZoneNotFoundException)
                {
                    qatarTime = DateTime.UtcNow.AddHours(3);
                    _logger.LogWarning("Qatar timezone not found, using UTC+3 offset instead");
                }

                var viewModel = new OilInformationViewModel
                {
                    OilChangeDate = qatarTime,
                    OilTypeList = GetOilTypeList(),
                    LicensePlateList = await GetLicensePlateListAsync() // Add this line
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing Create view");
                TempData["ErrorMessage"] = "Error loading form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: OilInformation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OilInformationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.OilTypeList = GetOilTypeList();
                    return View(model);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (!OilTypeHelper.GetAllTypes().Contains(model.OilType))
                    {
                        ModelState.AddModelError("OilType", "Invalid oil type selected.");
                        model.OilTypeList = GetOilTypeList();
                        return View(model);
                    }

                    var oilInfo = new OilInformation
                    {
                        LicensePlate = model.LicensePlate.Trim(),
                        OilType = model.OilType.Trim(),
                        OilBrand = model.OilBrand.Trim(),
                        OilPrice = model.OilPrice,
                        CurrentOdometer = model.CurrentOdometer,
                        NextOilChangeOdometer = model.NextOilChangeOdometer,
                        OilChangeDate = model.OilChangeDate,
                        Notes = model.Notes?.Trim(),
                        CreatedBy = User.Identity?.Name ?? "System",
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    _context.OilInformationSet.Add(oilInfo);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Oil information created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while saving oil information");
                    ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                    model.OilTypeList = GetOilTypeList();
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in Create action");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }


        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var oilInfo = await _context.OilInformationSet
                    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

                if (oilInfo == null)
                {
                    return NotFound();
                }

                var viewModel = new OilInformationViewModel
                {
                    Id = oilInfo.Id,
                    LicensePlate = oilInfo.LicensePlate.Trim(),
                    OilType = oilInfo.OilType,
                    OilBrand = oilInfo.OilBrand,
                    OilPrice = oilInfo.OilPrice,
                    CurrentOdometer = oilInfo.CurrentOdometer,
                    NextOilChangeOdometer = oilInfo.NextOilChangeOdometer,
                    OilChangeDate = oilInfo.OilChangeDate,
                    Notes = oilInfo.Notes,
                    OilTypeList = GetOilTypeList(),
                    LicensePlateList = await GetLicensePlateListAsync() // Add this line
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing Edit view for ID: {Id}", id);
                TempData["ErrorMessage"] = "Error loading edit form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, OilInformationViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    model.OilTypeList = GetOilTypeList();
                    return View(model);
                }

                if (!OilTypeHelper.GetAllTypes().Contains(model.OilType))
                {
                    ModelState.AddModelError("OilType", "Invalid oil type selected.");
                    model.OilTypeList = GetOilTypeList();
                    return View(model);
                }

                var oilInfo = await _context.OilInformationSet
                    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

                if (oilInfo == null)
                {
                    return NotFound();
                }

                oilInfo.LicensePlate = model.LicensePlate.Trim();
                oilInfo.OilType = model.OilType.Trim();
                oilInfo.OilBrand = model.OilBrand.Trim();
                oilInfo.OilPrice = model.OilPrice;
                oilInfo.CurrentOdometer = model.CurrentOdometer;
                oilInfo.NextOilChangeOdometer = model.NextOilChangeOdometer;
                oilInfo.OilChangeDate = model.OilChangeDate;
                oilInfo.Notes = model.Notes?.Trim();
                oilInfo.UpdatedBy = User.Identity?.Name ?? "System";
                oilInfo.UpdatedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Oil information updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating oil information for ID: {Id}", id);
                ModelState.AddModelError("", "An error occurred while saving changes. Please try again.");
                model.OilTypeList = GetOilTypeList();
                return View(model);
            }
        }

        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var oilInfo = await _context.OilInformationSet
                    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

                if (oilInfo == null)
                {
                    return NotFound();
                }

                return View(oilInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing Delete view for ID: {Id}", id);
                TempData["ErrorMessage"] = "Error loading delete form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var oilInfo = await _context.OilInformationSet
                    .FirstOrDefaultAsync(o => o.Id == id && !o.IsDeleted);

                if (oilInfo == null)
                {
                    return NotFound();
                }

                oilInfo.IsDeleted = true;
                oilInfo.DeletedBy = User.Identity?.Name ?? "System";
                oilInfo.DeletedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Oil information deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting oil information for ID: {Id}", id);
                TempData["ErrorMessage"] = "Error deleting record. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private List<SelectListItem> GetOilTypeList()
        {
            return OilTypeHelper.GetAllTypes()
                .Select(type => new SelectListItem
                {
                    Value = type,
                    Text = type
                })
                .ToList();
        }

        private bool OilInformationExists(int id)
        {
            return _context.OilInformationSet.Any(o => o.Id == id && !o.IsDeleted);
        }

        public IActionResult Export()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportData(string format)
        {
            try
            {
                var data = await _context.OilInformationSet
                    .Where(o => !o.IsDeleted)
                    .Select(o => new OilInformationViewModel
                    {
                        LicensePlate = o.LicensePlate.Trim(),
                        OilType = o.OilType,
                        OilBrand = o.OilBrand,
                        OilPrice = o.OilPrice,
                        CurrentOdometer = o.CurrentOdometer,
                        NextOilChangeOdometer = o.NextOilChangeOdometer,
                        OilChangeDate = o.OilChangeDate,
                        Notes = o.Notes
                    })
                    .ToListAsync();

                var fileName = $"OilInformation_{DateTime.Now:yyyyMMddHHmmss}";

                switch (format.ToLower())
                {
                    case "excel":
                        return ExportToExcel(data, fileName);
                    case "pdf":
                        return ExportToPdf(data, fileName);
                    case "csv":
                        return ExportToCsv(data, fileName);
                    default:
                        TempData["ErrorMessage"] = "Invalid export format selected";
                        return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while exporting data");
                TempData["ErrorMessage"] = "Error exporting data. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        private FileResult ExportToExcel(List<OilInformationViewModel> data, string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("OilInformation");

            // Add headers
            var headers = new[]
            {
                "Car Information", "Oil Type", "Oil Brand", "Oil Price",
                "Current Odometer", "Next Oil Change Odometer", "Oil Change Date", "Notes"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Add data
            for (int row = 0; row < data.Count; row++)
            {
                var item = data[row];
                worksheet.Cells[row + 2, 1].Value = item.LicensePlate;
                worksheet.Cells[row + 2, 2].Value = item.OilType;
                worksheet.Cells[row + 2, 3].Value = item.OilBrand;
                worksheet.Cells[row + 2, 4].Value = item.OilPrice;
                worksheet.Cells[row + 2, 5].Value = item.CurrentOdometer;
                worksheet.Cells[row + 2, 6].Value = item.NextOilChangeOdometer;
                worksheet.Cells[row + 2, 7].Value = item.OilChangeDate;
                worksheet.Cells[row + 2, 8].Value = item.Notes;
            }

            worksheet.Cells.AutoFitColumns();

            var content = package.GetAsByteArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
        }

        private FileResult ExportToPdf(List<OilInformationViewModel> data, string fileName)
        {
            using var ms = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            var writer = PdfWriter.GetInstance(document, ms);

            document.Open();

            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Oil Information Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(title);

            // Create table
            var table = new PdfPTable(8)
            {
                WidthPercentage = 100
            };

            // Add headers
            var headers = new[]
            {
                "Car Information", "Oil Type", "Oil Brand", "Oil Price",
                "Current Odometer", "Next Oil Change Odometer", "Oil Change Date", "Notes"
            };

            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            foreach (var header in headers)
            {
                table.AddCell(new PdfPCell(new Phrase(header, headerFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    BackgroundColor = BaseColor.LIGHT_GRAY,
                    Padding = 5
                });
            }

            // Add data
            var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var item in data)
            {
                table.AddCell(new PdfPCell(new Phrase(item.LicensePlate, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.OilType, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.OilBrand, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.OilPrice.ToString("N2"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.CurrentOdometer.ToString("N2"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.NextOilChangeOdometer.ToString("N2"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.OilChangeDate.ToString("dd/MM/yyyy"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Notes ?? "", cellFont)));
            }

            document.Add(table);
            document.Close();

            return File(ms.ToArray(), "application/pdf", $"{fileName}.pdf");
        }

        private FileResult ExportToCsv(List<OilInformationViewModel> data, string fileName)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, Encoding.UTF8);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Write headers
            csv.WriteHeader<OilInformationViewModel>();
            csv.NextRecord();

            // Write data
            foreach (var record in data)
            {
                csv.WriteRecord(record);
                csv.NextRecord();
            }

            writer.Flush();
            var result = ms.ToArray();
            return File(result, "text/csv", $"{fileName}.csv");
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                // Write headers
                csv.WriteField("Car Information");
                csv.WriteField("Oil Type");
                csv.WriteField("Oil Brand");
                csv.WriteField("Oil Price");
                csv.WriteField("Current Odometer");
                csv.WriteField("Next Oil Change Odometer");
                csv.WriteField("Oil Change Date");
                csv.WriteField("Notes");
                csv.NextRecord();

                // Write sample data
                csv.WriteField("1234567"); // Car Information
                csv.WriteField("Synthetic"); // Oil Type
                csv.WriteField("Mobil 1"); // Oil Brand
                csv.WriteField("250.00"); // Oil Price
                csv.WriteField("50000.00"); // Current Odometer
                csv.WriteField("55000.00"); // Next Oil Change Odometer
                csv.WriteField("23/02/2025"); // Oil Change Date
                csv.WriteField("Regular maintenance"); // Notes
            }

            return File(memoryStream.ToArray(), "text/csv", "OilInformationTemplate.csv");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to import.";
                return RedirectToAction(nameof(Import));
            }

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Please upload a valid CSV file.";
                return RedirectToAction(nameof(Import));
            }

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var records = new List<OilInformation>();
                    var importedCount = 0;
                    var errors = new List<string>();

                    using var reader = new StreamReader(file.OpenReadStream());
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        HeaderValidated = null,
                        MissingFieldFound = null,
                        BadDataFound = null,
                        TrimOptions = TrimOptions.Trim
                    };

                    using var csv = new CsvReader(reader, config);

                    // Read header row
                    csv.Read();
                    csv.ReadHeader();

                    // Process each row
                    while (csv.Read())
                    {
                        try
                        {
                            var licensePlate = csv.GetField("Car Information")?.Trim();
                            var oilType = csv.GetField("Oil Type")?.Trim();
                            var oilBrand = csv.GetField("Oil Brand")?.Trim();
                            var oilPriceStr = csv.GetField("Oil Price")?.Trim();
                            var currentOdometerStr = csv.GetField("Current Odometer")?.Trim();
                            var nextOdometerStr = csv.GetField("Next Oil Change Odometer")?.Trim();
                            var oilChangeDateStr = csv.GetField("Oil Change Date")?.Trim();
                            var notes = csv.GetField("Notes")?.Trim();

                            // Validate required fields
                            if (string.IsNullOrWhiteSpace(licensePlate) ||
                                string.IsNullOrWhiteSpace(oilType) ||
                                string.IsNullOrWhiteSpace(oilBrand))
                            {
                                throw new Exception("Car Information, Oil Type, and Oil Brand are required fields");
                            }

                            // Parse numeric values
                            if (!decimal.TryParse(oilPriceStr, out decimal oilPrice))
                                throw new Exception($"Invalid Oil Price format: {oilPriceStr}");

                            if (!decimal.TryParse(currentOdometerStr, out decimal currentOdometer))
                                throw new Exception($"Invalid Current Odometer format: {currentOdometerStr}");

                            if (!decimal.TryParse(nextOdometerStr, out decimal nextOdometer))
                                throw new Exception($"Invalid Next Oil Change Odometer format: {nextOdometerStr}");

                            // Parse date using specific formats
                            DateTime oilChangeDate;
                            string[] dateFormats = { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd", "dd-MM-yyyy" };

                            if (!DateTime.TryParseExact(oilChangeDateStr,
                                dateFormats,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out oilChangeDate))
                            {
                                throw new Exception($"Invalid date format. Date should be in dd/MM/yyyy format: {oilChangeDateStr}");
                            }

                            var oilInfo = new OilInformation
                            {
                                LicensePlate = licensePlate,
                                OilType = oilType,
                                OilBrand = oilBrand,
                                OilPrice = oilPrice,
                                CurrentOdometer = currentOdometer,
                                NextOilChangeOdometer = nextOdometer,
                                OilChangeDate = oilChangeDate,
                                Notes = notes,
                                CreatedBy = User.Identity?.Name ?? "System",
                                CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            };

                            records.Add(oilInfo);
                            importedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Row {importedCount + 1}: {ex.Message}");
                            _logger.LogWarning(ex, "Error processing row {RowNumber}", importedCount + 1);
                        }
                    }

                    if (importedCount > 0)
                    {
                        await _context.OilInformationSet.AddRangeAsync(records);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        var message = $"Successfully imported {importedCount} records.";
                        if (errors.Any())
                        {
                            message += $" Errors: {string.Join("; ", errors)}";
                        }
                        TempData["SuccessMessage"] = message;
                    }
                    else
                    {
                        throw new Exception($"No records were imported. {string.Join("; ", errors)}");
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error during import process");
                    TempData["ErrorMessage"] = $"Import failed: {ex.Message}";
                    return RedirectToAction(nameof(Import));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in Import action");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction(nameof(Import));
            }
        }
        private async Task<List<SelectListItem>> GetLicensePlateListAsync()
        {
            var licensePlates = await _context.ConsumptionDetails
                .Where(c => !c.IsDeleted && !string.IsNullOrEmpty(c.LicensePlate))
                .Select(c => c.LicensePlate)
                .Distinct()
                .OrderBy(l => l)
                .Select(l => new SelectListItem
                {
                    Value = l.Trim(),
                    Text = l.Trim()
                })
                .ToListAsync();

            return licensePlates;
        }

        // Helper method to get values from dynamic row with multiple possible header names
        private string GetValue(IDictionary<string, object> row, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                if (row.TryGetValue(name, out var value))
                {
                    return value?.ToString();
                }
            }
            return null;
        }
    }
}