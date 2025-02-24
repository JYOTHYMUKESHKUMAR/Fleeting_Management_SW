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
    public class TireInformationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TireInformationController> _logger;

        public TireInformationController(ApplicationDbContext context, ILogger<TireInformationController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var tireInformation = await _context.TireInformationSet
                    .Where(t => !t.IsDeleted)
                    .OrderByDescending(t => t.InstallationDate)
                    .Select(t => new TireInformationViewModel
                    {
                        Id = t.Id,
                        TireSerialNumber = t.TireSerialNumber,
                        TireSize = t.TireSize,
                        TireBrand = t.TireBrand,
                        TirePrice = t.TirePrice,
                        Supplier = t.Supplier,
                        LicensePlate = t.LicensePlate,
                        InstallationDate = t.InstallationDate,
                        InstallationOdometer = t.InstallationOdometer,
                        RemovalDate = t.RemovalDate,
                        RemovalOdometer = t.RemovalOdometer,
                        IsDeactivated = t.IsDeactivated,
                        Notes = t.Notes,
                        CreatedBy = t.CreatedBy,
                        CreatedOn = t.CreatedOn,
                        UpdatedBy = t.UpdatedBy,
                        UpdatedOn = t.UpdatedOn
                    })
                    .ToListAsync();

                return View(tireInformation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tire information");
                return StatusCode(500, "An error occurred while fetching tire information. Please try again later.");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var tireInfo = await _context.TireInformationSet
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

                if (tireInfo == null)
                {
                    return NotFound();
                }

                var viewModel = new TireInformationViewModel
                {
                    Id = tireInfo.Id,
                    TireSerialNumber = tireInfo.TireSerialNumber,
                    TireSize = tireInfo.TireSize,
                    TireBrand = tireInfo.TireBrand,
                    TirePrice = tireInfo.TirePrice,
                    Supplier = tireInfo.Supplier,
                    LicensePlate = tireInfo.LicensePlate,
                    InstallationDate = tireInfo.InstallationDate,
                    InstallationOdometer = tireInfo.InstallationOdometer,
                    RemovalDate = tireInfo.RemovalDate,
                    RemovalOdometer = tireInfo.RemovalOdometer,
                    IsDeactivated = tireInfo.IsDeactivated,
                    Notes = tireInfo.Notes,
                    CreatedBy = tireInfo.CreatedBy,
                    CreatedOn = tireInfo.CreatedOn,
                    UpdatedBy = tireInfo.UpdatedBy,
                    UpdatedOn = tireInfo.UpdatedOn,
                    LicensePlateList = await GetLicensePlateListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching tire information details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Error loading details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

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

                var viewModel = new TireInformationViewModel
                {
                    InstallationDate = qatarTime,
                    LicensePlateList = await GetLicensePlateListAsync()
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TireInformationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.LicensePlateList = await GetLicensePlateListAsync();
                    return View(model);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var tireInfo = new TireInformation
                    {
                        TireSerialNumber = model.TireSerialNumber.Trim(),
                        TireSize = model.TireSize.Trim(),
                        TireBrand = model.TireBrand.Trim(),
                        TirePrice = model.TirePrice,
                        Supplier = model.Supplier.Trim(),
                        LicensePlate = model.LicensePlate.Trim(),
                        InstallationDate = model.InstallationDate,
                        InstallationOdometer = model.InstallationOdometer,
                        RemovalDate = model.RemovalDate,
                        RemovalOdometer = model.RemovalOdometer,
                        IsDeactivated = model.IsDeactivated,
                        Notes = model.Notes?.Trim(),
                        CreatedBy = User.Identity?.Name ?? "System",
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    _context.TireInformationSet.Add(tireInfo);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Tire information created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while saving tire information");
                    ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                    model.LicensePlateList = await GetLicensePlateListAsync();
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
                var tireInfo = await _context.TireInformationSet
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

                if (tireInfo == null)
                {
                    return NotFound();
                }

                var viewModel = new TireInformationViewModel
                {
                    Id = tireInfo.Id,
                    TireSerialNumber = tireInfo.TireSerialNumber,
                    TireSize = tireInfo.TireSize,
                    TireBrand = tireInfo.TireBrand,
                    TirePrice = tireInfo.TirePrice,
                    Supplier = tireInfo.Supplier,
                    LicensePlate = tireInfo.LicensePlate,
                    InstallationDate = tireInfo.InstallationDate,
                    InstallationOdometer = tireInfo.InstallationOdometer,
                    RemovalDate = tireInfo.RemovalDate,
                    RemovalOdometer = tireInfo.RemovalOdometer,
                    IsDeactivated = tireInfo.IsDeactivated,
                    Notes = tireInfo.Notes,
                    LicensePlateList = await GetLicensePlateListAsync()
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
        public async Task<IActionResult> Edit(int id, TireInformationViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    model.LicensePlateList = await GetLicensePlateListAsync();
                    return View(model);
                }

                var tireInfo = await _context.TireInformationSet
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

                if (tireInfo == null)
                {
                    return NotFound();
                }

                tireInfo.TireSerialNumber = model.TireSerialNumber.Trim();
                tireInfo.TireSize = model.TireSize.Trim();
                tireInfo.TireBrand = model.TireBrand.Trim();
                tireInfo.TirePrice = model.TirePrice;
                tireInfo.Supplier = model.Supplier.Trim();
                tireInfo.LicensePlate = model.LicensePlate.Trim();
                tireInfo.InstallationDate = model.InstallationDate;
                tireInfo.InstallationOdometer = model.InstallationOdometer;
                tireInfo.RemovalDate = model.RemovalDate;
                tireInfo.RemovalOdometer = model.RemovalOdometer;
                tireInfo.IsDeactivated = model.IsDeactivated;
                tireInfo.Notes = model.Notes?.Trim();
                tireInfo.UpdatedBy = User.Identity?.Name ?? "System";
                tireInfo.UpdatedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tire information updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating tire information for ID: {Id}", id);
                ModelState.AddModelError("", "An error occurred while saving changes. Please try again.");
                model.LicensePlateList = await GetLicensePlateListAsync();
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
                var tireInfo = await _context.TireInformationSet
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

                if (tireInfo == null)
                {
                    return NotFound();
                }

                return View(tireInfo);
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
                var tireInfo = await _context.TireInformationSet
                    .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);

                if (tireInfo == null)
                {
                    return NotFound();
                }

                tireInfo.IsDeleted = true;
                tireInfo.DeletedBy = User.Identity?.Name ?? "System";
                tireInfo.DeletedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Tire information deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting tire information for ID: {Id}", id);
                TempData["ErrorMessage"] = "Error deleting record. Please try again.";
                return RedirectToAction(nameof(Index));
            }
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
                csv.WriteField("Serial Number");
                csv.WriteField("Size");
                csv.WriteField("Brand");
                csv.WriteField("Price");
                csv.WriteField("Supplier");
                csv.WriteField("License Plate");
                csv.WriteField("Installation Date");
                csv.WriteField("Installation Odometer");
                csv.WriteField("Removal Date");
                csv.WriteField("Removal Odometer");
                csv.WriteField("Is Deactivated");
                csv.WriteField("Notes");
                csv.NextRecord();

                // Write sample data
                csv.WriteField("TIR123456");
                csv.WriteField("205/55R16");
                csv.WriteField("Michelin");
                csv.WriteField("500.00");
                csv.WriteField("AutoParts Store");
                csv.WriteField("123456");
                csv.WriteField(DateTime.Now.ToString("dd/MM/yyyy"));
                csv.WriteField("50000.00");
                csv.WriteField("");
                csv.WriteField("");
                csv.WriteField("false");
                csv.WriteField("New tire installation");
            }

            return File(memoryStream.ToArray(), "text/csv", "TireInformationTemplate.csv");
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

            try
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var records = new List<TireInformation>();
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
                            var serialNumber = csv.GetField("Serial Number")?.Trim();
                            var size = csv.GetField("Size")?.Trim();
                            var brand = csv.GetField("Brand")?.Trim();
                            var priceStr = csv.GetField("Price")?.Trim();
                            var supplier = csv.GetField("Supplier")?.Trim();
                            var licensePlate = csv.GetField("License Plate")?.Trim();
                            var installationDateStr = csv.GetField("Installation Date")?.Trim();
                            var installationOdometerStr = csv.GetField("Installation Odometer")?.Trim();
                            var removalDateStr = csv.GetField("Removal Date")?.Trim();
                            var removalOdometerStr = csv.GetField("Removal Odometer")?.Trim();
                            var isDeactivatedStr = csv.GetField("Is Deactivated")?.Trim();
                            var notes = csv.GetField("Notes")?.Trim();

                            // Validate required fields
                            if (string.IsNullOrWhiteSpace(serialNumber))
                                throw new Exception("Serial Number is required");
                            if (string.IsNullOrWhiteSpace(size))
                                throw new Exception("Size is required");
                            if (string.IsNullOrWhiteSpace(brand))
                                throw new Exception("Brand is required");
                            if (string.IsNullOrWhiteSpace(supplier))
                                throw new Exception("Supplier is required");
                            if (string.IsNullOrWhiteSpace(licensePlate))
                                throw new Exception("License Plate is required");

                            // Parse numeric values
                            if (!decimal.TryParse(priceStr, out decimal price))
                                throw new Exception($"Invalid Price format: {priceStr}");
                            if (!decimal.TryParse(installationOdometerStr, out decimal installationOdometer))
                                throw new Exception($"Invalid Installation Odometer format: {installationOdometerStr}");

                            // Parse dates
                            if (!DateTime.TryParseExact(
                                installationDateStr,
                                new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" },
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.None,
                                out DateTime installationDate))
                            {
                                throw new Exception($"Invalid Installation Date format: {installationDateStr}. Use dd/MM/yyyy format.");
                            }

                            // Parse optional removal date
                            DateTime? removalDate = null;
                            if (!string.IsNullOrWhiteSpace(removalDateStr))
                            {
                                if (!DateTime.TryParseExact(
                                    removalDateStr,
                                    new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" },
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out DateTime parsedRemovalDate))
                                {
                                    throw new Exception($"Invalid Removal Date format: {removalDateStr}. Use dd/MM/yyyy format.");
                                }
                                removalDate = parsedRemovalDate;
                            }

                            // Parse optional removal odometer
                            decimal? removalOdometer = null;
                            if (!string.IsNullOrWhiteSpace(removalOdometerStr))
                            {
                                if (!decimal.TryParse(removalOdometerStr, out decimal parsedRemovalOdometer))
                                {
                                    throw new Exception($"Invalid Removal Odometer format: {removalOdometerStr}");
                                }
                                removalOdometer = parsedRemovalOdometer;
                            }

                            // Parse is deactivated
                            bool isDeactivated = false;
                            if (!string.IsNullOrWhiteSpace(isDeactivatedStr))
                            {
                                if (!bool.TryParse(isDeactivatedStr, out isDeactivated))
                                {
                                    throw new Exception($"Invalid Is Deactivated format: {isDeactivatedStr}. Use 'true' or 'false'.");
                                }
                            }

                            var tireInfo = new TireInformation
                            {
                                TireSerialNumber = serialNumber,
                                TireSize = size,
                                TireBrand = brand,
                                TirePrice = price,
                                Supplier = supplier,
                                LicensePlate = licensePlate,
                                InstallationDate = installationDate,
                                InstallationOdometer = installationOdometer,
                                RemovalDate = removalDate,
                                RemovalOdometer = removalOdometer,
                                IsDeactivated = isDeactivated,
                                Notes = notes,
                                CreatedBy = User.Identity?.Name ?? "System",
                                CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            };

                            records.Add(tireInfo);
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
                        await _context.TireInformationSet.AddRangeAsync(records);
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
                var data = await _context.TireInformationSet
                    .Where(t => !t.IsDeleted)
                    .Select(t => new TireInformationViewModel
                    {
                        TireSerialNumber = t.TireSerialNumber,
                        TireSize = t.TireSize,
                        TireBrand = t.TireBrand,
                        TirePrice = t.TirePrice,
                        Supplier = t.Supplier,
                        LicensePlate = t.LicensePlate,
                        InstallationDate = t.InstallationDate,
                        InstallationOdometer = t.InstallationOdometer,
                        RemovalDate = t.RemovalDate,
                        RemovalOdometer = t.RemovalOdometer,
                        IsDeactivated = t.IsDeactivated,
                        Notes = t.Notes
                    })
                    .ToListAsync();

                var fileName = $"TireInformation_{DateTime.Now:yyyyMMddHHmmss}";

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

        private FileResult ExportToExcel(List<TireInformationViewModel> data, string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("TireInformation");

            // Add headers
            var headers = new[]
            {
        "Serial Number", "Size", "Brand", "Price", "Supplier",
        "License Plate", "Installation Date", "Installation Odometer",
        "Removal Date", "Removal Odometer", "Is Deactivated", "Notes"
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
                worksheet.Cells[row + 2, 1].Value = item.TireSerialNumber;
                worksheet.Cells[row + 2, 2].Value = item.TireSize;
                worksheet.Cells[row + 2, 3].Value = item.TireBrand;
                worksheet.Cells[row + 2, 4].Value = item.TirePrice;
                worksheet.Cells[row + 2, 5].Value = item.Supplier;
                worksheet.Cells[row + 2, 6].Value = item.LicensePlate;
                worksheet.Cells[row + 2, 7].Value = item.InstallationDate.ToString("dd/MM/yyyy");
                worksheet.Cells[row + 2, 8].Value = item.InstallationOdometer;
                worksheet.Cells[row + 2, 9].Value = item.RemovalDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row + 2, 10].Value = item.RemovalOdometer;
                worksheet.Cells[row + 2, 11].Value = item.IsDeactivated;
                worksheet.Cells[row + 2, 12].Value = item.Notes;
            }

            worksheet.Cells.AutoFitColumns();

            var content = package.GetAsByteArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
        }

        private FileResult ExportToPdf(List<TireInformationViewModel> data, string fileName)
        {
            using var ms = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            var writer = PdfWriter.GetInstance(document, ms);

            document.Open();

            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Tire Information Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(title);

            // Create table
            var table = new PdfPTable(12)
            {
                WidthPercentage = 100
            };

            // Add headers
            var headers = new[]
            {
        "Serial Number", "Size", "Brand", "Price", "Supplier",
        "License Plate", "Installation Date", "Installation Odometer",
        "Removal Date", "Removal Odometer", "Is Deactivated", "Notes"
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
                table.AddCell(new PdfPCell(new Phrase(item.TireSerialNumber, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.TireSize, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.TireBrand, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.TirePrice.ToString("N2"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Supplier, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.LicensePlate, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.InstallationDate.ToString("dd/MM/yyyy"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.InstallationOdometer.ToString("N2"), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.RemovalDate?.ToString("dd/MM/yyyy") ?? "", cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.RemovalOdometer?.ToString("N2") ?? "", cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.IsDeactivated.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Notes ?? "", cellFont)));
            }

            document.Add(table);
            document.Close();

            return File(ms.ToArray(), "application/pdf", $"{fileName}.pdf");
        }

        private FileResult ExportToCsv(List<TireInformationViewModel> data, string fileName)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, Encoding.UTF8);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Write headers
            csv.WriteField("Serial Number");
            csv.WriteField("Size");
            csv.WriteField("Brand");
            csv.WriteField("Price");
            csv.WriteField("Supplier");
            csv.WriteField("License Plate");
            csv.WriteField("Installation Date");
            csv.WriteField("Installation Odometer");
            csv.WriteField("Removal Date");
            csv.WriteField("Removal Odometer");
            csv.WriteField("Is Deactivated");
            csv.WriteField("Notes");
            csv.NextRecord();

            // Write data
            foreach (var item in data)
            {
                csv.WriteField(item.TireSerialNumber);
                csv.WriteField(item.TireSize);
                csv.WriteField(item.TireBrand);
                csv.WriteField(item.TirePrice.ToString("N2"));
                csv.WriteField(item.Supplier);
                csv.WriteField(item.LicensePlate);
                csv.WriteField(item.InstallationDate.ToString("dd/MM/yyyy"));
                csv.WriteField(item.InstallationOdometer.ToString("N2"));
                csv.WriteField(item.RemovalDate?.ToString("dd/MM/yyyy"));
                csv.WriteField(item.RemovalOdometer?.ToString("N2"));
                csv.WriteField(item.IsDeactivated);
                csv.WriteField(item.Notes);
                csv.NextRecord();
            }

            writer.Flush();
            var result = ms.ToArray();
            return File(result, "text/csv", $"{fileName}.csv");
        }

        private async Task<List<SelectListItem>> GetLicensePlateListAsync()
        {
            return await _context.ConsumptionDetails
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
        }

        private bool TireInformationExists(int id)
        {
            return _context.TireInformationSet.Any(t => t.Id == id && !t.IsDeleted);
        }
    }
}