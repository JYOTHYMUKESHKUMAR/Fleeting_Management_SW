using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using DotNetCoreMVCApp.Models;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.IO;
using System.Text;
using CsvHelper;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authorization;

namespace DotNetCoreMVCApp.Web.Controllers
{
    public class DriverController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DriverController> _logger;

        public DriverController(ApplicationDbContext context, ILogger<DriverController> logger)
        {
            _context = context;
            _logger = logger;
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var drivers = await _context.DriverSet
                    .Where(d => !d.IsDeleted)
                    .Select(d => new DriverViewModel
                    {
                        Id = d.Id,
                        Name = d.Name,
                        QatarId = d.QatarId,
                        ContactNumber = d.ContactNumber,
                        QIDExpiryDate = d.QIDExpiryDate,
                        Nationality = d.Nationality,
                        DOB = d.DOB,
                        CreatedBy = d.CreatedBy,
                        CreatedOn = d.CreatedOn,
                        UpdatedBy = d.UpdatedBy,
                        UpdatedOn = d.UpdatedOn,
                        IsDeleted = d.IsDeleted
                    })
                    .ToListAsync();
                return View(drivers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching drivers");
                return StatusCode(500, "An error occurred while fetching drivers. Please try again later.");
            }
        }

        public IActionResult Create()
        {
            return View(new DriverCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DriverCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (await _context.DriverSet.AnyAsync(d => d.QatarId == model.QatarId && !d.IsDeleted))
                {
                    ModelState.AddModelError(nameof(model.QatarId), "Qatar ID already exists");
                    return View(model);
                }

                var driver = new Driver
                {
                    Id = model.Id,
                    Name = model.Name,
                    QatarId = model.QatarId,
                    ContactNumber = model.ContactNumber,
                    QIDExpiryDate = model.QIDExpiryDate,
                    Nationality = model.Nationality,
                    DOB = model.DOB,
                    CreatedBy = User.Identity?.Name ?? "System",
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.DriverSet.Add(driver);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation($"Driver created: {driver.Id}");
                TempData["SuccessMessage"] = "Driver created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Database error occurred while creating driver");
                if (ex.InnerException?.Message.Contains("IX_Driver_QatarId", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    ModelState.AddModelError(nameof(model.QatarId), "This Qatar ID is already registered.");
                }
                else
                {
                    ModelState.AddModelError("", "A database error occurred. Please try again.");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while creating driver");
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var driver = await _context.DriverSet
                .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

            if (driver == null)
            {
                return NotFound();
            }

            var viewModel = new DriverViewModel
            {
                Id = driver.Id,
                Name = driver.Name,
                QatarId = driver.QatarId,
                ContactNumber = driver.ContactNumber,
                QIDExpiryDate = driver.QIDExpiryDate,
                Nationality = driver.Nationality,
                DOB = driver.DOB,
                CreatedBy = driver.CreatedBy,
                CreatedOn = driver.CreatedOn,
                UpdatedBy = driver.UpdatedBy,
                UpdatedOn = driver.UpdatedOn
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DriverViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var driver = await _context.DriverSet
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

                if (driver == null)
                {
                    return NotFound();
                }

                if (await _context.DriverSet.AnyAsync(d =>
                    d.QatarId == model.QatarId &&
                    d.Id != id &&
                    !d.IsDeleted))
                {
                    ModelState.AddModelError(nameof(model.QatarId), "This Qatar ID is already registered with another driver");
                    return View(model);
                }

                driver.Name = model.Name;
                driver.QatarId = model.QatarId;
                driver.ContactNumber = model.ContactNumber;
                driver.QIDExpiryDate = model.QIDExpiryDate;
                driver.Nationality = model.Nationality;
                driver.DOB = model.DOB;
                driver.UpdatedBy = User.Identity?.Name ?? "System";
                driver.UpdatedOn = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Driver updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Database error occurred while updating driver {id}");
                if (ex.InnerException?.Message.Contains("IX_Driver_QatarId", StringComparison.OrdinalIgnoreCase) ?? false)
                {
                    ModelState.AddModelError(nameof(model.QatarId), "This Qatar ID is already registered with another driver.");
                }
                else
                {
                    ModelState.AddModelError("", "A database error occurred. Please try again.");
                }
                return View(model);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error updating driver: {id}");
                ModelState.AddModelError("", "An error occurred while updating the driver.");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var driver = await _context.DriverSet.FirstOrDefaultAsync(d => d.Id == id);

            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var driver = await _context.DriverSet.FirstOrDefaultAsync(d => d.Id == id);

                if (driver == null)
                {
                    return NotFound();
                }

                // Hard delete - remove the record from database
                _context.DriverSet.Remove(driver);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Driver deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error occurred while deleting driver {id}");
                TempData["ErrorMessage"] = "An error occurred while deleting the driver";
                return RedirectToAction(nameof(Index));
            }
        }
        // Add these methods to the DriverController class

        [HttpGet]
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
                var drivers = await _context.DriverSet
                    .Where(d => !d.IsDeleted)
                    .OrderBy(d => d.Name)
                    .ToListAsync();

                byte[] fileContents;
                string contentType;
                string fileName = $"Drivers_{DateTime.Now:yyyyMMdd}";

                switch (format?.ToLower())
                {
                    case "excel":
                        fileContents = await ExportToExcel(drivers);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileName += ".xlsx";
                        break;

                    case "csv":
                        fileContents = await ExportToCsv(drivers);
                        contentType = "text/csv";
                        fileName += ".csv";
                        break;

                    case "pdf":
                        fileContents = await ExportToPdf(drivers);
                        contentType = "application/pdf";
                        fileName += ".pdf";
                        break;

                    default:
                        return BadRequest("Invalid export format");
                }

                return File(fileContents, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Export failed");
                TempData["ErrorMessage"] = "Export failed";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<byte[]> ExportToExcel(List<Driver> drivers)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Drivers");

            // Headers
            var headers = new[]
            {
        "ID", "Name", "Qatar ID", "Contact Number", "QID Expiry Date",
        "Nationality", "Date of Birth"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Data
            for (int i = 0; i < drivers.Count; i++)
            {
                var driver = drivers[i];
                int row = i + 2;

                worksheet.Cells[row, 1].Value = driver.Id;
                worksheet.Cells[row, 2].Value = driver.Name;
                worksheet.Cells[row, 3].Value = driver.QatarId;
                worksheet.Cells[row, 4].Value = driver.ContactNumber;
                worksheet.Cells[row, 5].Value = driver.QIDExpiryDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 6].Value = driver.Nationality;
                worksheet.Cells[row, 7].Value = driver.DOB.ToString("yyyy-MM-dd");
            }

            worksheet.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        private async Task<byte[]> ExportToCsv(List<Driver> drivers)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Write headers
            csv.WriteField("ID");
            csv.WriteField("Name");
            csv.WriteField("Qatar ID");
            csv.WriteField("Contact Number");
            csv.WriteField("QID Expiry Date");
            csv.WriteField("Nationality");
            csv.WriteField("Date of Birth");
            csv.NextRecord();

            // Write data
            foreach (var driver in drivers)
            {
                csv.WriteField(driver.Id);
                csv.WriteField(driver.Name);
                csv.WriteField(driver.QatarId);
                csv.WriteField(driver.ContactNumber);
                csv.WriteField(driver.QIDExpiryDate.ToString("yyyy-MM-dd"));
                csv.WriteField(driver.Nationality);
                csv.WriteField(driver.DOB.ToString("yyyy-MM-dd"));
                csv.NextRecord();
            }

            await writer.FlushAsync();
            return memoryStream.ToArray();
        }

        private async Task<byte[]> ExportToPdf(List<Driver> drivers)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Drivers List", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(title);

            // Create table
            var table = new PdfPTable(7) // Number of columns
            {
                WidthPercentage = 100,
                SpacingBefore = 10f,
                SpacingAfter = 10f
            };

            // Set column widths
            float[] widths = new float[] { 5f, 15f, 15f, 12f, 12f, 12f, 12f };
            table.SetWidths(widths);

            // Add headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            string[] headers = { "ID", "Name", "Qatar ID", "Contact", "QID Expiry", "Nationality", "DOB" };
            foreach (string header in headers)
            {
                var cell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = new BaseColor(240, 240, 240),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 5f
                };
                table.AddCell(cell);
            }

            // Add data
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            foreach (var driver in drivers)
            {
                table.AddCell(new PdfPCell(new Phrase(driver.Id.ToString(), normalFont)));
                table.AddCell(new PdfPCell(new Phrase(driver.Name, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(driver.QatarId, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(driver.ContactNumber ?? "", normalFont)));
                table.AddCell(new PdfPCell(new Phrase(driver.QIDExpiryDate.ToString("yyyy-MM-dd"), normalFont)));
                table.AddCell(new PdfPCell(new Phrase(driver.Nationality, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(driver.DOB.ToString("yyyy-MM-dd"), normalFont)));
            }

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }
        public IActionResult Import()
        {
            return View();
        }

        [HttpGet]
        public IActionResult DownloadTemplate()
        {
            try
            {
                var templateContent = "Id,Name,QatarId,ContactNumber,QIDExpiryDate,Nationality,DOB\n" +
                                    "1001,John Doe,12345678901,12345678,2025-12-31,Qatari,1990-01-01\n" +
                                    "1002,Jane Smith,98765432109,87654321,2025-12-31,Indian,1992-05-15";

                var byteArray = Encoding.UTF8.GetBytes(templateContent);
                return File(byteArray, "text/csv", "DriverImportTemplate.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating template file");
                return RedirectToAction(nameof(Import));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to import.";
                return View();
            }

            if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "Please upload a valid CSV file.";
                return View();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                using var reader = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    HeaderValidated = null,
                    MissingFieldFound = null
                });

                var records = csv.GetRecords<DriverImportViewModel>().ToList();
                var existingQatarIds = await _context.DriverSet
                    .Where(d => !d.IsDeleted)
                    .Select(d => d.QatarId)
                    .ToListAsync();

                int createdCount = 0;
                int updatedCount = 0;
                var errors = new List<string>();
                var processedQatarIds = new HashSet<string>();

                foreach (var record in records)
                {
                    try
                    {
                        if (!System.Text.RegularExpressions.Regex.IsMatch(record.QatarId, @"^\d{11}$"))
                        {
                            errors.Add($"Invalid Qatar ID format for record with ID {record.Id}. Qatar ID must be exactly 11 digits.");
                            continue;
                        }

                        if (!processedQatarIds.Add(record.QatarId))
                        {
                            errors.Add($"Duplicate Qatar ID {record.QatarId} found in import file for ID {record.Id}");
                            continue;
                        }

                        if (!int.TryParse(record.Id, out int driverId))
                        {
                            errors.Add($"Invalid ID format for record: {record.Id}");
                            continue;
                        }

                        var driver = await _context.DriverSet
                            .FirstOrDefaultAsync(d => d.Id == driverId && !d.IsDeleted);

                        if (driver != null)
                        {
                            if (existingQatarIds.Contains(record.QatarId) && driver.QatarId != record.QatarId)
                            {
                                errors.Add($"Qatar ID {record.QatarId} is already registered with another driver (ID: {record.Id})");
                                continue;
                            }

                            driver.Name = record.Name;
                            driver.QatarId = record.QatarId;
                            driver.ContactNumber = record.ContactNumber;
                            driver.QIDExpiryDate = record.QIDExpiryDate;
                            driver.Nationality = record.Nationality;
                            driver.DOB = record.DOB;
                            driver.UpdatedBy = User.Identity?.Name ?? "System";
                            driver.UpdatedOn = DateTime.UtcNow;
                            updatedCount++;
                        }
                        else
                        {
                            if (existingQatarIds.Contains(record.QatarId))
                            {
                                errors.Add($"Qatar ID {record.QatarId} is already registered with another driver (ID: {record.Id})");
                                continue;
                            }

                            driver = new Driver
                            {
                                Id = driverId,
                                Name = record.Name,
                                QatarId = record.QatarId,
                                ContactNumber = record.ContactNumber,
                                QIDExpiryDate = record.QIDExpiryDate,
                                Nationality = record.Nationality,
                                DOB = record.DOB,
                                CreatedBy = User.Identity?.Name ?? "System",
                                CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            _context.DriverSet.Add(driver);
                            createdCount++;
                            existingQatarIds.Add(record.QatarId);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error processing record with ID {record.Id}: {ex.Message}");
                    }
                }

                if (errors.Any())
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = string.Join("\n", errors);
                    return View();
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Successfully imported {createdCount} new drivers and updated {updatedCount} existing drivers.";
                return RedirectToAction(nameof(Index));
                }
            catch (CsvHelper.HeaderValidationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Invalid CSV header format");
                TempData["ErrorMessage"] = "The CSV file format is invalid. Please use the template provided.";
                return View();
            }
            catch (CsvHelper.ValidationException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "CSV validation error");
                TempData["ErrorMessage"] = "One or more records in the CSV file are invalid. Please check the data format.";
                return View();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error importing drivers");
                TempData["ErrorMessage"] = "An unexpected error occurred while importing drivers. Please try again.";
                return View();
            }
        }

        private bool DriverExists(int id)
        {
            return _context.DriverSet.Any(e => e.Id == id && !e.IsDeleted);
        }

        private bool IsValidQatarId(string qatarId)
        {
            return !string.IsNullOrEmpty(qatarId) && 
                   System.Text.RegularExpressions.Regex.IsMatch(qatarId, @"^\d{11}$");
        }

        private async Task<bool> IsQatarIdUnique(string qatarId, int? excludeId = null)
        {
            var query = _context.DriverSet.Where(d => !d.IsDeleted && d.QatarId == qatarId);
            
            if (excludeId.HasValue)
            {
                query = query.Where(d => d.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }
    }
}