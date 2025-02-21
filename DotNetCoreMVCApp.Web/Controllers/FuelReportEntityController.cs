using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCoreMVCApp.Controllers
{
    public class FuelReportEntityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FuelReportEntityController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(FuelReportFilter filter)
        {
            try
            {
                var viewModel = new FuelReportViewModel
                {
                    Filter = filter ?? new FuelReportFilter()
                };

                // Base query from the view
                var query = _context.FuelReportEntitySet.AsQueryable();

                // Apply date filters if provided
                if (filter?.StartDate.HasValue == true)
                {
                    var startDate = filter.StartDate.Value.Date; // Set to start of day
                    query = query.Where(x => x.SaleTime.Date >= startDate);
                }

                if (filter?.EndDate.HasValue == true)
                {
                    var endDate = filter.EndDate.Value.Date.AddDays(1).AddSeconds(-1); // Set to end of day
                    query = query.Where(x => x.SaleTime.Date <= filter.EndDate.Value.Date);
                }

                // Apply sorting
                query = query.OrderByDescending(x => x.SaleTime)
                           .ThenByDescending(x => x.ConsumptionDetailsId);

                // Execute query
                viewModel.Details = await query.ToListAsync();

                // Populate dropdown lists
                viewModel.DriversList = await _context.DriverSet
                    .OrderBy(d => d.Name)
                    .Select(d => new SelectListItem
                    {
                        Value = d.Id.ToString(),
                        Text = d.Name
                    })
                    .ToListAsync();

                viewModel.VehiclesList = await _context.VehicleSet
                    .OrderBy(v => v.LicensePlate)
                    .Select(v => new SelectListItem
                    {
                        Value = v.LicensePlate,
                        Text = v.LicensePlate
                    })
                    .ToListAsync();

                viewModel.VehicleTypesList = await _context.VehicleSet
                    .Select(v => v.VehicleType)
                    .Distinct()
                    .OrderBy(vt => vt)
                    .Select(vt => new SelectListItem
                    {
                        Value = vt,
                        Text = vt
                    })
                    .ToListAsync();

                viewModel.ProductNamesList = await _context.ConsumptionDetails
                    .Select(c => c.ProductName)
                    .Distinct()
                    .OrderBy(p => p)
                    .Select(p => new SelectListItem
                    {
                        Value = p,
                        Text = p
                    })
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception
                return View(new FuelReportViewModel());
            }
        }
    

// Helper method to format currency
private string FormatCurrency(decimal amount)
            {
                return $"QAR {amount:N2}";
            }
        }
    }