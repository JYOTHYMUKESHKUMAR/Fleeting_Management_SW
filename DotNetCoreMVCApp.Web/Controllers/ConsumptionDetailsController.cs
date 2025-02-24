using DotNetCoreMVCApp.Models.Repository;
using DotNetCoreMVCApp.Models.Web;
using DotNetCoreMVCApp.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using OfficeOpenXml; // For Excel
using iTextSharp.text;
using iTextSharp.text.pdf;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using DotNetCoreMVCApp.Models;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace DotNetCoreMVCApp.Web.Controllers
{
    public class ConsumptionDetailsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ConsumptionDetailsController> _logger;

        public ConsumptionDetailsController(ApplicationDbContext context, ILogger<ConsumptionDetailsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: ConsumptionDetails/Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var consumptionDetails = await _context.ConsumptionDetails
                    .Include(c => c.Driver)
                    .Where(c => !c.IsDeleted)
                    .OrderByDescending(c => c.SaleTime)
                    .Select(c => new ConsumptionDetailsViewModel
                    {
                        Id = c.Id,
                        LicensePlate = c.LicensePlate,
                        WoqodLicensePlate = c.WoqodLicensePlate,
                        DriverId = c.DriverId,
                        DriverName = c.DriverName,
                        ProductName = c.ProductName,
                        OfflineLimit = c.OfflineLimit,
                        FuelQuantity = c.FuelQuantity,
                        UnitPrice = c.UnitPrice,
                        TotalCost = c.TotalCost,
                        SaleTime = c.SaleTime,
                        InvoiceMonth = c.InvoiceMonth,
                        StationName = c.StationName,
                        GroupName = c.GroupName,
                        FleetName = c.FleetName,
                        CostCenter = c.CostCenter,
                        OdometerReading = c.OdometerReading,
                        CreatedBy = c.CreatedBy,
                        CreatedOn = c.CreatedOn,
                        UpdatedBy = c.UpdatedBy,
                        UpdatedOn = c.UpdatedOn
                    })
                    .ToListAsync();

                return View(consumptionDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching consumption details");
                return StatusCode(500, "An error occurred while fetching consumption details. Please try again later.");
            }
        }

        // GET: ConsumptionDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var consumptionDetails = await _context.ConsumptionDetails
                    .Include(c => c.Driver)
                    .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

                if (consumptionDetails == null)
                {
                    return NotFound();
                }

                return View(consumptionDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching consumption details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Error loading details. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: ConsumptionDetails/Create
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
                    // Fallback: manually add 3 hours for Qatar time
                    qatarTime = DateTime.UtcNow.AddHours(3);
                    _logger.LogWarning("Qatar timezone not found, using UTC+3 offset instead");
                }

                var viewModel = new ConsumptionDetailsCreateViewModel
                {
                    SaleTime = qatarTime,
                    InvoiceMonth = qatarTime.ToString("yyyyMM"),
                    DriverList = await GetDriverListAsync(),
                    ProductNameList = GetProductNameList(),
                    LicensePlateList = await GetVehicleLicensePlateListAsync()
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

        // POST: ConsumptionDetails/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConsumptionDetailsCreateViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    model.DriverList = await GetDriverListAsync();
                    model.ProductNameList = GetProductNameList();
                    model.LicensePlateList = await GetVehicleLicensePlateListAsync();
                    return View(model);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var vehicle = await _context.VehicleSet
                        .FirstOrDefaultAsync(v => v.LicensePlate == model.LicensePlate && !v.IsDeleted);

                    if (vehicle == null)
                    {
                        ModelState.AddModelError("LicensePlate", "Invalid license plate selected.");
                        model.DriverList = await GetDriverListAsync();
                        model.ProductNameList = GetProductNameList();
                        model.LicensePlateList = await GetVehicleLicensePlateListAsync();
                        return View(model);
                    }

                    var driver = await _context.DriverSet
                        .FirstOrDefaultAsync(d => d.Id == model.DriverId && !d.IsDeleted);

                    if (driver == null)
                    {
                        ModelState.AddModelError("DriverId", "Invalid driver selected.");
                        model.DriverList = await GetDriverListAsync();
                        model.ProductNameList = GetProductNameList();
                        model.LicensePlateList = await GetVehicleLicensePlateListAsync();
                        return View(model);
                    }

                    if (model.FuelQuantity > model.OfflineLimit)
                    {
                        ModelState.AddModelError("FuelQuantity", "Fuel quantity cannot exceed offline limit.");
                        model.DriverList = await GetDriverListAsync();
                        model.ProductNameList = GetProductNameList();
                        model.LicensePlateList = await GetVehicleLicensePlateListAsync();
                        return View(model);
                    }

                    var consumptionDetails = new ConsumptionDetails
                    {
                        LicensePlate = model.LicensePlate.Trim(),
                        WoqodLicensePlate = vehicle.WoqodLicensePlate.Trim(),
                        DriverId = model.DriverId,
                        DriverName = driver.Name.Trim(),
                        ProductName = model.ProductName.Trim(),
                        OfflineLimit = model.OfflineLimit,
                        FuelQuantity = model.FuelQuantity,
                        UnitPrice = model.UnitPrice,
                        TotalCost = model.FuelQuantity * model.UnitPrice,
                        SaleTime = model.SaleTime,
                        InvoiceMonth = model.InvoiceMonth,
                        StationName = model.StationName?.Trim(),
                        GroupName = model.GroupName?.Trim(),
                        FleetName = model.FleetName?.Trim(),
                        CostCenter = model.CostCenter?.Trim(),
                        OdometerReading = model.OdometerReading,
                        Remarks = model.Remarks?.Trim(),
                        CreatedBy = User.Identity?.Name ?? "System",
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    _context.ConsumptionDetails.Add(consumptionDetails);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Consumption details created successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while saving consumption details");
                    ModelState.AddModelError("", "An error occurred while saving the consumption details. Please try again.");
                    model.DriverList = await GetDriverListAsync();
                    model.ProductNameList = GetProductNameList();
                    model.LicensePlateList = await GetVehicleLicensePlateListAsync();
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
        /*[Authorize(Roles = "Admin")]*/
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Starting Edit action for ID: {Id}", id);

            try
            {
                // First, get the consumption details without including Driver
                var consumptionDetails = await _context.ConsumptionDetails
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (consumptionDetails == null)
                {
                    _logger.LogWarning("Consumption details not found for ID: {Id}", id);
                    TempData["ErrorMessage"] = "Record not found";
                    return RedirectToAction(nameof(Index));
                }

                // Separately get the driver to avoid navigation property issues
                var driver = await _context.DriverSet
                    .FirstOrDefaultAsync(d => d.Id == consumptionDetails.DriverId);

                // Load all the necessary dropdown data first
                var driverList = await _context.DriverSet
                    .Where(d => !d.IsDeleted)
                    .OrderBy(d => d.Name)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    })
                    .ToListAsync();

                var licensePlateList = await _context.VehicleSet
                    .Where(v => !v.IsDeleted)
                    .OrderBy(v => v.LicensePlate)
                    .Select(v => new SelectListItem
                    {
                        Value = v.LicensePlate,
                        Text = $"{v.LicensePlate} - {v.VehicleBrand} {v.VehicleModel}"
                    })
                    .ToListAsync();

                var productNameList = ProductNameHelper.GetAllTypes()
                    .Select(type => new SelectListItem
                    {
                        Value = type,
                        Text = type
                    })
                    .ToList();

                // Create the view model with null-safe assignments
                var viewModel = new ConsumptionDetailsCreateViewModel
                {
                    Id = consumptionDetails.Id,
                    LicensePlate = consumptionDetails.LicensePlate ?? string.Empty,
                    WoqodLicensePlate = consumptionDetails.WoqodLicensePlate ?? string.Empty,
                    DriverId = consumptionDetails.DriverId,
                    DriverName = driver?.Name ?? consumptionDetails.DriverName ?? string.Empty,
                    ProductName = consumptionDetails.ProductName ?? string.Empty,
                    OfflineLimit = consumptionDetails.OfflineLimit,
                    FuelQuantity = consumptionDetails.FuelQuantity,
                    UnitPrice = consumptionDetails.UnitPrice,
                    TotalCost = consumptionDetails.TotalCost,
                    SaleTime = consumptionDetails.SaleTime,
                    InvoiceMonth = consumptionDetails.InvoiceMonth ?? DateTime.Now.ToString("yyyyMMdd"),
                    StationName = consumptionDetails.StationName ?? string.Empty,
                    GroupName = consumptionDetails.GroupName ?? string.Empty,
                    FleetName = consumptionDetails.FleetName ?? string.Empty,
                    CostCenter = consumptionDetails.CostCenter ?? string.Empty,
                    OdometerReading = consumptionDetails.OdometerReading,
                    Remarks = consumptionDetails.Remarks ?? string.Empty,

                    // Add the pre-loaded dropdown lists
                    DriverList = driverList,
                    ProductNameList = productNameList,
                    LicensePlateList = licensePlateList
                };

                _logger.LogInformation("Successfully prepared Edit view model for ID: {Id}", id);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing Edit view for ID: {Id}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the edit form. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ConsumptionDetailsCreateViewModel model)
        {
            _logger.LogInformation("Starting Edit POST action for ID: {Id}", id);

            if (id != model.Id)
            {
                _logger.LogWarning("ID mismatch in Edit POST. Route ID: {RouteId}, Model ID: {ModelId}", id, model.Id);
                return BadRequest("Invalid request");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state in Edit POST for ID: {Id}", id);
                await PrepareViewModelDropdowns(model);
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get existing record
                var existingRecord = await _context.ConsumptionDetails
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (existingRecord == null)
                {
                    _logger.LogWarning("Record not found for ID: {Id}", id);
                    TempData["ErrorMessage"] = "Record not found";
                    return RedirectToAction(nameof(Index));
                }

                // Validate vehicle
                var vehicle = await _context.VehicleSet
                    .FirstOrDefaultAsync(v => v.LicensePlate == model.LicensePlate && !v.IsDeleted);

                if (vehicle == null)
                {
                    _logger.LogWarning("Invalid vehicle license plate: {LicensePlate}", model.LicensePlate);
                    ModelState.AddModelError("LicensePlate", "Invalid license plate selected");
                    await PrepareViewModelDropdowns(model);
                    return View(model);
                }

                // Validate driver
                var driver = await _context.DriverSet
                    .FirstOrDefaultAsync(d => d.Id == model.DriverId && !d.IsDeleted);

                if (driver == null)
                {
                    _logger.LogWarning("Invalid driver ID: {DriverId}", model.DriverId);
                    ModelState.AddModelError("DriverId", "Invalid driver selected");
                    await PrepareViewModelDropdowns(model);
                    return View(model);
                }

                // Validate fuel quantity
                if (model.FuelQuantity <= 0)
                {
                    ModelState.AddModelError("FuelQuantity", "Fuel quantity must be greater than zero");
                    await PrepareViewModelDropdowns(model);
                    return View(model);
                }

                if (model.FuelQuantity > model.OfflineLimit)
                {
                    ModelState.AddModelError("FuelQuantity", "Fuel quantity cannot exceed offline limit");
                    await PrepareViewModelDropdowns(model);
                    return View(model);
                }

                // Validate unit price
                if (model.UnitPrice <= 0)
                {
                    ModelState.AddModelError("UnitPrice", "Unit price must be greater than zero");
                    await PrepareViewModelDropdowns(model);
                    return View(model);
                }

                // Update existing record
                existingRecord.LicensePlate = model.LicensePlate.Trim();
                existingRecord.WoqodLicensePlate = vehicle.WoqodLicensePlate?.Trim() ?? string.Empty;
                existingRecord.DriverId = model.DriverId;
                existingRecord.DriverName = driver.Name.Trim();
                existingRecord.ProductName = model.ProductName.Trim();
                existingRecord.OfflineLimit = model.OfflineLimit;
                existingRecord.FuelQuantity = model.FuelQuantity;
                existingRecord.UnitPrice = model.UnitPrice;
                existingRecord.TotalCost = Math.Round(model.FuelQuantity * model.UnitPrice, 2);
                existingRecord.SaleTime = model.SaleTime;
                existingRecord.InvoiceMonth = model.InvoiceMonth;
                existingRecord.StationName = model.StationName?.Trim() ?? string.Empty;
                existingRecord.GroupName = model.GroupName?.Trim() ?? string.Empty;
                existingRecord.FleetName = model.FleetName?.Trim() ?? string.Empty;
                existingRecord.CostCenter = model.CostCenter?.Trim() ?? string.Empty;
                existingRecord.OdometerReading = model.OdometerReading;
                existingRecord.Remarks = model.Remarks?.Trim() ?? string.Empty;
                existingRecord.UpdatedBy = User.Identity?.Name ?? "System";
                existingRecord.UpdatedOn = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully updated consumption details for ID: {Id}", id);
                    TempData["SuccessMessage"] = "Record updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Concurrency error while updating record ID: {Id}", id);
                    ModelState.AddModelError("", "This record has been modified by another user. Please refresh and try again.");
                    await PrepareViewModelDropdowns(model);
                    return View(model);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving changes for record ID: {Id}", id);
                    ModelState.AddModelError("", "An error occurred while saving changes. Please try again.");
                    await PrepareViewModelDropdowns(model);
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Unhandled error in Edit POST for ID: {Id}", id);
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to prepare dropdown lists
        private async Task PrepareViewModelDropdowns(ConsumptionDetailsCreateViewModel model)
        {
            try
            {
                model.DriverList = await _context.DriverSet
                    .Where(d => !d.IsDeleted)
                    .OrderBy(d => d.Name)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    })
                    .ToListAsync();

                model.ProductNameList = ProductNameHelper.GetAllTypes()
                    .Select(type => new SelectListItem
                    {
                        Value = type,
                        Text = type
                    })
                    .ToList();

                model.LicensePlateList = await _context.VehicleSet
                    .Where(v => !v.IsDeleted)
                    .OrderBy(v => v.LicensePlate)
                    .Select(v => new SelectListItem
                    {
                        Value = v.LicensePlate,
                        Text = $"{v.LicensePlate} - {v.VehicleBrand} {v.VehicleModel}"
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error preparing dropdown lists");
                model.DriverList = new List<SelectListItem>();
                model.ProductNameList = new List<SelectListItem>();
                model.LicensePlateList = new List<SelectListItem>();
            }
        }
        /*[Authorize(Roles = "Admin")]*/
        // GET: ConsumptionDetails/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var consumptionDetail = await _context.ConsumptionDetails
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (consumptionDetail == null)
                {
                    return NotFound();
                }

                return View(consumptionDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while preparing Delete view for ID: {Id}", id);
                return RedirectToAction(nameof(Index));
            }
        }
        
        // POST: ConsumptionDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var consumptionDetail = await _context.ConsumptionDetails
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (consumptionDetail == null)
                {
                    return NotFound();
                }

                _context.ConsumptionDetails.Remove(consumptionDetail);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Record deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting consumption detail. ID: {Id}", id);
                TempData["ErrorMessage"] = "Error deleting record.";
                return RedirectToAction(nameof(Index));
            }
        }
        // API endpoint to get vehicle details
        [HttpGet]
        public async Task<IActionResult> GetVehicleDetails(string licensePlate)
        {
            try
            {
                var vehicle = await _context.VehicleSet
                    .Where(v => v.LicensePlate == licensePlate && !v.IsDeleted)
                    .Select(v => new
                    {
                        v.WoqodLicensePlate,
                        v.FuelLimit
                    })
                    .FirstOrDefaultAsync();

                if (vehicle == null)
                {
                    return NotFound();
                }

                return Json(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching vehicle details for license plate: {LicensePlate}", licensePlate);
                return StatusCode(500, "Error fetching vehicle details");
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
                var data = await _context.ConsumptionDetails
                    .Include(c => c.Driver)
                    .Select(c => new ConsumptionDetailsViewModel
                    {
                        LicensePlate = c.LicensePlate,
                        WoqodLicensePlate = c.WoqodLicensePlate,
                        DriverName = c.DriverName,
                        ProductName = c.ProductName,
                        OfflineLimit = c.OfflineLimit,
                        FuelQuantity = c.FuelQuantity,
                        UnitPrice = c.UnitPrice,
                        TotalCost = c.TotalCost,
                        SaleTime = c.SaleTime,
                        InvoiceMonth = c.InvoiceMonth,
                        StationName = c.StationName,
                        GroupName = c.GroupName,
                        FleetName = c.FleetName,
                        CostCenter = c.CostCenter,
                        OdometerReading = c.OdometerReading ?? 0,
                        Remarks = c.Remarks
                    })
                    .ToListAsync();

                var fileName = $"ConsumptionDetails_{DateTime.Now:yyyyMMddHHmmss}";

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

        private FileResult ExportToExcel(List<ConsumptionDetailsViewModel> data, string fileName)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("ConsumptionDetails");

            // Add headers
            var headers = new[]
            {
        "License Plate", "Woqod License Plate", "Driver Name", "Product Name",
        "Offline Limit", "Fuel Quantity", "Unit Price", "Total Cost",
        "Sale Time", "Invoice Month", "Station Name", "Group Name",
        "Fleet Name", "Cost Center", "Odometer Reading", "Remarks"
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
                worksheet.Cells[row + 2, 2].Value = item.WoqodLicensePlate;
                worksheet.Cells[row + 2, 3].Value = item.DriverName;
                worksheet.Cells[row + 2, 4].Value = item.ProductName;
                worksheet.Cells[row + 2, 5].Value = item.OfflineLimit;
                worksheet.Cells[row + 2, 6].Value = item.FuelQuantity;
                worksheet.Cells[row + 2, 7].Value = item.UnitPrice;
                worksheet.Cells[row + 2, 8].Value = item.TotalCost;
                worksheet.Cells[row + 2, 9].Value = item.SaleTime;
                worksheet.Cells[row + 2, 10].Value = item.InvoiceMonth;
                worksheet.Cells[row + 2, 11].Value = item.StationName;
                worksheet.Cells[row + 2, 12].Value = item.GroupName;
                worksheet.Cells[row + 2, 13].Value = item.FleetName;
                worksheet.Cells[row + 2, 14].Value = item.CostCenter;
                worksheet.Cells[row + 2, 15].Value = item.OdometerReading;
                worksheet.Cells[row + 2, 16].Value = item.Remarks;
            }

            worksheet.Cells.AutoFitColumns();

            var content = package.GetAsByteArray();
            return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{fileName}.xlsx");
        }

        private FileResult ExportToPdf(List<ConsumptionDetailsViewModel> data, string fileName)
        {
            using var ms = new MemoryStream();
            var document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            var writer = PdfWriter.GetInstance(document, ms);

            document.Open();

            // Add title
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
            var title = new Paragraph("Consumption Details Report", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(title);

            // Create table
            var table = new PdfPTable(16)
            {
                WidthPercentage = 100
            };

            // Add headers
            var headers = new[]
            {
        "License Plate", "Woqod License Plate", "Driver Name", "Product Name",
        "Offline Limit", "Fuel Quantity", "Unit Price", "Total Cost",
        "Sale Time", "Invoice Month", "Station Name", "Group Name",
        "Fleet Name", "Cost Center", "Odometer Reading", "Remarks"
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
                table.AddCell(new PdfPCell(new Phrase(item.WoqodLicensePlate, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.DriverName, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.ProductName, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.OfflineLimit.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.FuelQuantity.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.UnitPrice.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.TotalCost.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.SaleTime.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.InvoiceMonth, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.StationName, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.GroupName, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.FleetName, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.CostCenter, cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.OdometerReading.ToString(), cellFont)));
                table.AddCell(new PdfPCell(new Phrase(item.Remarks, cellFont)));
            }

            document.Add(table);
            document.Close();

            return File(ms.ToArray(), "application/pdf", $"{fileName}.pdf");
        }

        private FileResult ExportToCsv(List<ConsumptionDetailsViewModel> data, string fileName)
        {
            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, Encoding.UTF8);
            using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

            // Write headers
            csv.WriteHeader<ConsumptionDetailsViewModel>();
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
                // Write headers matching the model exactly
                csv.WriteField("License Plate");
                csv.WriteField("WLicense Plate");
                csv.WriteField("Driver ID");
                csv.WriteField("Driver Name");
                csv.WriteField("Product Name");
                csv.WriteField("Offline Limit");
                csv.WriteField("Liters(Lt)");
                csv.WriteField("Unit Price (QAR/Lt)");
                csv.WriteField("Total Amount (Qar)");
                csv.WriteField("Sale Time");
                csv.WriteField("Invoice Month");
                csv.WriteField("Station Name");
                csv.WriteField("Group Name");
                csv.WriteField("Fleet Name");
                csv.WriteField("Cost Center");
                csv.WriteField("Odometer Reading");
                csv.WriteField("Remarks");
                csv.NextRecord();

                // Write sample data
                csv.WriteField("280179"); // License Plate
                csv.WriteField("09280179"); // WLicense Plate
                csv.WriteField("1"); // Driver ID
                csv.WriteField("John Doe"); // Driver Name
                csv.WriteField("SILVER 91"); // Product Name
                csv.WriteField("1.9"); // Offline Limit
                csv.WriteField("92.06"); // Liters
                csv.WriteField("1.9"); // Unit Price
                csv.WriteField("174.91"); // Total Amount
                csv.WriteField("11/23/2024 9:46"); // Sale Time
                csv.WriteField("20241102"); // Invoice Month
                csv.WriteField("New Industrial Area"); // Station Name
                csv.WriteField("QATAR BUILDING COMPANY"); // Group Name
                csv.WriteField("QABUCO.WOQODe"); // Fleet Name
                csv.WriteField("garage"); // Cost Center
                csv.WriteField("12345.67"); // Odometer Reading
                csv.WriteField("Sample remarks"); // Remarks
            }

            return File(memoryStream.ToArray(), "text/csv", "ConsumptionDetailsTemplate.csv");
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
                    using var reader = new StreamReader(file.OpenReadStream());
                    using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HasHeaderRecord = true,
                        HeaderValidated = null,
                        MissingFieldFound = null,
                        TrimOptions = TrimOptions.Trim,
                        PrepareHeaderForMatch = args => args.Header
                            .Replace(" ", "")
                            .Replace("(", "")
                            .Replace(")", "")
                            .Replace("/", "")
                            .Replace(".", "")
                            .ToLower()
                    });

                    var importedCount = 0;
                    var errors = new List<string>();
                    var records = new List<dynamic>();

                    while (csv.Read())
                    {
                        records.Add(csv.GetRecord<dynamic>());
                    }

                    foreach (var record in records)
                    {
                        try
                        {
                            var row = record as IDictionary<string, object>;

                            // Get WLicense Plate and generate License Plate if needed
                            string woqodLicensePlate = GetValue(row, "WLicensePlate", "WoqodLicensePlate", "WLicensePlate")?.Trim();
                            string licensePlate = GetValue(row, "LicensePlate")?.Trim();

                            if (string.IsNullOrWhiteSpace(licensePlate) &&
                                !string.IsNullOrWhiteSpace(woqodLicensePlate) &&
                                woqodLicensePlate.Length >= 6)
                            {
                                licensePlate = woqodLicensePlate.Substring(
                                    Math.Max(0, woqodLicensePlate.Length - 6));
                            }

                            // Format Invoice Month
                            string invoiceMonth = GetValue(row, "InvoiceMonth", "Invoice Month");
                            if (invoiceMonth?.Length == 8 && int.TryParse(invoiceMonth, out _))
                            {
                                string year = invoiceMonth.Substring(0, 4);
                                string month = invoiceMonth.Substring(4, 2);
                                invoiceMonth = $"{month}/{year}";
                            }

                            // Handle sale time
                            string saleTimeStr = GetValue(row, "SaleTime", "Sale Time");
                            if (!DateTime.TryParse(saleTimeStr, out DateTime saleTime))
                            {
                                throw new Exception($"Invalid Sale Time format: {saleTimeStr}");
                            }

                            var consumptionDetails = new ConsumptionDetails
                            {
                                LicensePlate = licensePlate,
                                WoqodLicensePlate = woqodLicensePlate,
                                DriverName = GetValue(row, "DriverName", "Driver Name")?.Trim(),
                                ProductName = GetValue(row, "ProductName", "Product Name")?.Trim() ??
                                    throw new Exception("Product Name is required"),
                                OfflineLimit = decimal.Parse(GetValue(row, "OfflineLimit", "Offline Limit") ?? "0"),
                                FuelQuantity = decimal.Parse(GetValue(row, "FuelQuantity", "Liters", "LitersLt") ?? "0"),
                                UnitPrice = decimal.Parse(GetValue(row, "UnitPrice", "UnitPriceQARLt") ?? "0"),
                                TotalCost = decimal.Parse(GetValue(row, "TotalCost", "TotalAmount", "TotalAmountQar") ?? "0"),
                                SaleTime = saleTime,
                                InvoiceMonth = invoiceMonth,
                                StationName = GetValue(row, "StationName", "Station Name")?.Trim(),
                                GroupName = GetValue(row, "GroupName", "Group Name")?.Trim(),
                                FleetName = GetValue(row, "FleetName", "Fleet Name")?.Trim(),
                                CostCenter = GetValue(row, "CostCenter", "Cost Center")?.Trim(),
                                OdometerReading = decimal.TryParse(GetValue(row, "OdometerReading", "Odometer Reading"), out decimal odometerReading) ? odometerReading : null,
                                Remarks = GetValue(row, "Remarks")?.Trim(),
                                CreatedBy = User.Identity?.Name ?? "System",
                                CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            };

                            // Try to find matching driver
                            if (!string.IsNullOrWhiteSpace(consumptionDetails.DriverName))
                            {
                                var driver = await _context.DriverSet
                                    .FirstOrDefaultAsync(d => d.Name.Contains(consumptionDetails.DriverName));
                                if (driver != null)
                                {
                                    consumptionDetails.DriverId = driver.Id;
                                }
                            }

                            // Validate required fields
                            if (string.IsNullOrWhiteSpace(consumptionDetails.ProductName))
                            {
                                throw new Exception("Product Name is required");
                            }

                            _context.ConsumptionDetails.Add(consumptionDetails);
                            importedCount++;
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Error processing record: {ex.Message}");
                        }
                    }

                    if (importedCount > 0)
                    {
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                    }

                    if (errors.Any())
                    {
                        TempData["WarningMessage"] = $"Imported {importedCount} records with {errors.Count} errors. Errors: {string.Join("; ", errors)}";
                    }
                    else
                    {
                        TempData["SuccessMessage"] = $"Successfully imported {importedCount} records.";
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while importing data");
                    TempData["ErrorMessage"] = $"Error importing data: {ex.Message}";
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

        // Helper method to get values from dynamic row with multiple possible header names
        private string GetValue(IDictionary<string, object> row, params string[] possibleNames)
        {
            foreach (var name in possibleNames)
            {
                var normalizedName = name.Replace(" ", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("/", "")
                    .Replace(".", "")
                    .ToLower();
                if (row.TryGetValue(normalizedName, out var value))
                {
                    return value?.ToString();
                }
            }
            return null;
        }

        // Helper methods
        private async Task<List<SelectListItem>> GetDriverListAsync()
        {
            return await _context.DriverSet
                .Where(d => !d.IsDeleted)
                .OrderBy(d => d.Name)
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
                .ToListAsync();
        }

        private static List<SelectListItem> GetProductNameList()
        {
            return ProductNameHelper.GetAllTypes()
                .Select(type => new SelectListItem
                {
                    Value = type,
                    Text = type
                })
                .ToList();
        }

        private async Task<List<SelectListItem>> GetVehicleLicensePlateListAsync()
        {
            return await _context.VehicleSet
                .Where(v => !v.IsDeleted)
                .OrderBy(v => v.LicensePlate)
                .Select(v => new SelectListItem
                {
                    Value = v.LicensePlate,
                    Text = $"{v.LicensePlate} - {v.VehicleBrand} {v.VehicleModel}"
                })
                .ToListAsync();
        }

        // Helper method to check if consumption details exists
        private bool ConsumptionDetailsExists(int id)
        {
            return _context.ConsumptionDetails.Any(c => c.Id == id && !c.IsDeleted);
        }


    }
}