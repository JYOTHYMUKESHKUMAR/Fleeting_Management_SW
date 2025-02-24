using CsvHelper.Configuration;
using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Repository;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsvHelper;


namespace DotNetCoreMVCApp.Web.Controllers
{
    public class VehicleController : Controller
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<VehicleController> _logger;

        public VehicleController(ApplicationDbContext dbContext, ILogger<VehicleController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var vehicles = await _dbContext.VehicleSet
                    .Where(v => !v.IsDeleted)
                    .ToListAsync();
                return View(vehicles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching vehicles");
                return StatusCode(500, "An error occurred while fetching vehicles. Please try again later.");
            }
        }

        public IActionResult Create()
        {
            var vehicle = new Vehicle
            {
                RegistrationExpiryDate = DateTime.Today.AddYears(1),
                InsurancePolicyExpiry = DateTime.Today.AddYears(1)
            };
            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehicle model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Check for duplicates
                if (await _dbContext.VehicleSet.AnyAsync(v => !v.IsDeleted &&
                    (v.VehicleId == model.VehicleId ||
                     v.LicensePlate == model.LicensePlate ||
                     v.WoqodLicensePlate == model.WoqodLicensePlate ||
                     v.GPSCode == model.GPSCode)))
                {
                    ModelState.AddModelError("", "Vehicle ID, License Plate, Woqod License Plate, or GPS Code already exists.");
                    return View(model);
                }

                // Validate fuel limit against tank capacity
                if (model.FuelLimit > model.FuelTankCapacity)
                {
                    ModelState.AddModelError("FuelLimit", "Fuel limit cannot exceed fuel tank capacity.");
                    return View(model);
                }

                model.VehicleId = model.VehicleId.Trim().ToUpper();
                model.LicensePlate = model.LicensePlate.Trim().ToUpper();
                model.WoqodLicensePlate = model.WoqodLicensePlate.Trim().ToUpper();
                model.GPSCode = model.GPSCode.Trim();
                model.VehicleBrand = model.VehicleBrand.Trim();
                model.VehicleModel = model.VehicleModel.Trim();
                model.InsurancePolicyNo = model.InsurancePolicyNo.Trim();
                model.CreatedBy = User.Identity?.Name ?? "System";
                model.CreatedOn = DateTime.UtcNow;
                model.IsDeleted = false;

                _dbContext.VehicleSet.Add(model);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Vehicle created with ID: {VehicleId}", model.VehicleId);
                TempData["SuccessMessage"] = "Vehicle created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while creating vehicle");
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(string id)
        {
            var vehicle = await _dbContext.VehicleSet
                .FirstOrDefaultAsync(v => v.VehicleId == id && !v.IsDeleted);

            if (vehicle == null)
            {
                return NotFound();
            }

            return View(vehicle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, Vehicle model)
        {
            if (id != model.VehicleId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var vehicle = await _dbContext.VehicleSet
                    .FirstOrDefaultAsync(v => v.VehicleId == id && !v.IsDeleted);

                if (vehicle == null)
                {
                    return NotFound();
                }

                // Check for duplicates (excluding current vehicle)
                if (await _dbContext.VehicleSet.AnyAsync(v =>
                    v.VehicleId != id &&
                    !v.IsDeleted &&
                    (v.LicensePlate == model.LicensePlate.Trim().ToUpper() ||
                     v.WoqodLicensePlate == model.WoqodLicensePlate.Trim().ToUpper() ||
                     v.GPSCode == model.GPSCode.Trim())))
                {
                    ModelState.AddModelError("", "License Plate, Woqod License Plate, or GPS Code already exists.");
                    return View(model);
                }

                // Validate fuel limit against tank capacity
                if (model.FuelLimit > model.FuelTankCapacity)
                {
                    ModelState.AddModelError("FuelLimit", "Fuel limit cannot exceed fuel tank capacity.");
                    return View(model);
                }

                vehicle.LicensePlate = model.LicensePlate.Trim().ToUpper();
                vehicle.WoqodLicensePlate = model.WoqodLicensePlate.Trim().ToUpper();
                vehicle.GPSCode = model.GPSCode.Trim();
                vehicle.VehicleType = model.VehicleType;
                vehicle.VehicleBrand = model.VehicleBrand.Trim();
                vehicle.VehicleModel = model.VehicleModel.Trim();
                vehicle.Year = model.Year;
                vehicle.FuelTankCapacity = model.FuelTankCapacity;
                vehicle.FuelLimit = model.FuelLimit;
                vehicle.RegistrationExpiryDate = model.RegistrationExpiryDate;
                vehicle.InsurancePolicyNo = model.InsurancePolicyNo.Trim();
                vehicle.InsurancePolicyExpiry = model.InsurancePolicyExpiry;
                vehicle.UpdatedBy = User.Identity?.Name ?? "System";
                vehicle.UpdatedOn = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Vehicle updated successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                if (!VehicleExists(id))
                {
                    return NotFound();
                }
                ModelState.AddModelError("", "The record was modified by another user. Please refresh and try again.");
                return View(model);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while updating vehicle with ID: {VehicleId}", id);
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            try
            {
                var vehicle = await _dbContext.VehicleSet
                    .AsNoTracking()
                    .FirstOrDefaultAsync(v => v.VehicleId == id);

                if (vehicle == null)
                {
                    return NotFound();
                }

                return View(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle for delete. ID: {Id}", id);
                TempData["ErrorMessage"] = "Error fetching vehicle details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var vehicle = await _dbContext.VehicleSet
                    .FirstOrDefaultAsync(v => v.VehicleId == id);

                if (vehicle == null)
                {
                    return NotFound();
                }

                // Hard delete - remove from database
                _dbContext.VehicleSet.Remove(vehicle);
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Vehicle deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error deleting vehicle. ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while deleting the vehicle.";
                return RedirectToAction(nameof(Index));
            }
        }
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
                var vehicles = await _dbContext.VehicleSet
                    .Where(v => !v.IsDeleted)
                    .OrderBy(v => v.VehicleId)
                    .ToListAsync();

                byte[] fileContents;
                string contentType;
                string fileName = $"Vehicles_{DateTime.Now:yyyyMMdd}";

                switch (format?.ToLower())
                {
                    case "excel":
                        fileContents = await ExportToExcel(vehicles);
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        fileName += ".xlsx";
                        break;

                    case "csv":
                        fileContents = await ExportToCsv(vehicles);
                        contentType = "text/csv";
                        fileName += ".csv";
                        break;

                    case "pdf":
                        fileContents = await ExportToPdf(vehicles);
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

        private async Task<byte[]> ExportToExcel(List<Vehicle> vehicles)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Vehicles");

            // Headers
            var headers = new[]
            {
        "Vehicle ID", "License Plate", "Woqod License Plate", "GPS Code",
        "Vehicle Type", "Vehicle Brand", "Vehicle Model", "Year",
        "Fuel Tank Capacity (L)", "Fuel Limit (L)",
        "Registration Expiry Date", "Insurance Policy No", "Insurance Policy Expiry"
    };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Data
            for (int i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                int row = i + 2;

                worksheet.Cells[row, 1].Value = vehicle.VehicleId;
                worksheet.Cells[row, 2].Value = vehicle.LicensePlate;
                worksheet.Cells[row, 3].Value = vehicle.WoqodLicensePlate;
                worksheet.Cells[row, 4].Value = vehicle.GPSCode;
                worksheet.Cells[row, 5].Value = vehicle.VehicleType;
                worksheet.Cells[row, 6].Value = vehicle.VehicleBrand;
                worksheet.Cells[row, 7].Value = vehicle.VehicleModel;
                worksheet.Cells[row, 8].Value = vehicle.Year;
                worksheet.Cells[row, 9].Value = vehicle.FuelTankCapacity;
                worksheet.Cells[row, 10].Value = vehicle.FuelLimit;
                worksheet.Cells[row, 11].Value = vehicle.RegistrationExpiryDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 12].Value = vehicle.InsurancePolicyNo;
                worksheet.Cells[row, 13].Value = vehicle.InsurancePolicyExpiry.ToString("yyyy-MM-dd");
            }

            worksheet.Cells.AutoFitColumns();
            return await package.GetAsByteArrayAsync();
        }

        private async Task<byte[]> ExportToCsv(List<Vehicle> vehicles)
        {
            using var memoryStream = new MemoryStream();
            using var writer = new StreamWriter(memoryStream);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Write headers
            csv.WriteField("Vehicle ID");
            csv.WriteField("License Plate");
            csv.WriteField("Woqod License Plate");
            csv.WriteField("GPS Code");
            csv.WriteField("Vehicle Type");
            csv.WriteField("Vehicle Brand");
            csv.WriteField("Vehicle Model");
            csv.WriteField("Year");
            csv.WriteField("Fuel Tank Capacity (L)");
            csv.WriteField("Fuel Limit (L)");
            csv.WriteField("Registration Expiry Date");
            csv.WriteField("Insurance Policy No");
            csv.WriteField("Insurance Policy Expiry");
            csv.NextRecord();

            // Write data
            foreach (var vehicle in vehicles)
            {
                csv.WriteField(vehicle.VehicleId);
                csv.WriteField(vehicle.LicensePlate);
                csv.WriteField(vehicle.WoqodLicensePlate);
                csv.WriteField(vehicle.GPSCode);
                csv.WriteField(vehicle.VehicleType);
                csv.WriteField(vehicle.VehicleBrand);
                csv.WriteField(vehicle.VehicleModel);
                csv.WriteField(vehicle.Year);
                csv.WriteField(vehicle.FuelTankCapacity);
                csv.WriteField(vehicle.FuelLimit);
                csv.WriteField(vehicle.RegistrationExpiryDate.ToString("yyyy-MM-dd"));
                csv.WriteField(vehicle.InsurancePolicyNo);
                csv.WriteField(vehicle.InsurancePolicyExpiry.ToString("yyyy-MM-dd"));
                csv.NextRecord();
            }

            await writer.FlushAsync();
            return memoryStream.ToArray();
        }

        private async Task<byte[]> ExportToPdf(List<Vehicle> vehicles)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Vehicles List", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(title);

            // Create table
            var table = new PdfPTable(13) // Number of columns
            {
                WidthPercentage = 100,
                SpacingBefore = 10f,
                SpacingAfter = 10f
            };

            // Set column widths
            float[] widths = new float[] { 8f, 8f, 8f, 8f, 8f, 8f, 8f, 4f, 7f, 7f, 8f, 8f, 8f };
            table.SetWidths(widths);

            // Add headers
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8);
            string[] headers = {
        "Vehicle ID", "License Plate", "Woqod Plate", "GPS Code",
        "Type", "Brand", "Model", "Year", "Tank Cap.", "Fuel Limit",
        "Reg. Expiry", "Insurance No", "Ins. Expiry"
    };

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
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            foreach (var vehicle in vehicles)
            {
                table.AddCell(new PdfPCell(new Phrase(vehicle.VehicleId, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.LicensePlate, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.WoqodLicensePlate, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.GPSCode, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.VehicleType, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.VehicleBrand, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.VehicleModel, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.Year.ToString(), normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.FuelTankCapacity.ToString("F2"), normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.FuelLimit.ToString("F2"), normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.RegistrationExpiryDate.ToString("yyyy-MM-dd"), normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.InsurancePolicyNo, normalFont)));
                table.AddCell(new PdfPCell(new Phrase(vehicle.InsurancePolicyExpiry.ToString("yyyy-MM-dd"), normalFont)));
            }

            document.Add(table);
            document.Close();

            return memoryStream.ToArray();
        }
        public IActionResult Import()
        {
            return View();
        }

        public IActionResult DownloadTemplate()
        {
            try
            {
                var templateContent =
                    "VehicleId,LicensePlate,WoqodLicensePlate,GPSCode,VehicleType,VehicleBrand,VehicleModel,Year,FuelTankCapacity,FuelLimit,RegistrationExpiryDate,InsurancePolicyNo,InsurancePolicyExpiry\n" +
                    $"V001,ABC123,WQD123,GPS001,{VehicleTypeHelper.ConcreteMixer},Volvo,FM12,2023,1000,800,2024-12-31,INS001,2024-12-31\n" +
                    $"V002,XYZ789,WQD789,GPS002,{VehicleTypeHelper.ConcretePump},Mercedes,Actros,2023,1500,1200,2024-12-31,INS002,2024-12-31";

                var byteArray = Encoding.UTF8.GetBytes(templateContent);
                return File(byteArray, "text/csv", "VehicleImportTemplate.csv");
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

            try
            {
                var records = new List<Vehicle>();
                using (var reader = new StreamReader(file.OpenReadStream()))
                using (var csv = new CsvHelper.CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    HeaderValidated = null,
                    MissingFieldFound = null
                }))
                {
                    records = csv.GetRecords<Vehicle>().ToList();
                }

                int importedCount = 0;
                int failedCount = 0;

                foreach (var record in records)
                {
                    try
                    {
                        using var transaction = await _dbContext.Database.BeginTransactionAsync();

                        // Basic data cleanup
                        record.VehicleId = record.VehicleId?.Replace(" ", "-").Trim();
                        record.IsDeleted = false;
                        record.CreatedBy = User.Identity?.Name ?? "System";
                        record.CreatedOn = DateTime.UtcNow;

                        // Check if record exists
                        var exists = await _dbContext.VehicleSet
                            .AnyAsync(v => v.VehicleId == record.VehicleId);

                        if (!exists)
                        {
                            _dbContext.VehicleSet.Add(record);
                            await _dbContext.SaveChangesAsync();
                            await transaction.CommitAsync();
                            importedCount++;
                        }
                        else
                        {
                            failedCount++;
                            _logger.LogWarning($"Vehicle with ID {record.VehicleId} already exists and was skipped.");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedCount++;
                        _logger.LogError(ex, $"Failed to import vehicle {record.VehicleId}. Error: {ex.Message}");
                        continue;
                    }
                }

                // Set success message with import results
                if (importedCount > 0)
                {
                    TempData["SuccessMessage"] = $"Successfully imported {importedCount} vehicles." +
                        (failedCount > 0 ? $" {failedCount} records failed to import." : "");
                }
                else if (failedCount > 0)
                {
                    TempData["ErrorMessage"] = $"Import failed. All {failedCount} records could not be imported.";
                }
                else
                {
                    TempData["WarningMessage"] = "No records were found to import.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import failed");
                TempData["ErrorMessage"] = $"Import failed: {ex.Message}";
                return View();
            }
        }
        private bool VehicleExists(string id)
        {
            return _dbContext.VehicleSet.Any(v => v.VehicleId == id && !v.IsDeleted);
        }

        private static List<SelectListItem> GetVehicleTypeList()
        {
            return VehicleTypeHelper.GetAllTypes()
                .Select(type => new SelectListItem
                {
                    Value = type,
                    Text = type
                })
                .ToList();
        }
    }
}