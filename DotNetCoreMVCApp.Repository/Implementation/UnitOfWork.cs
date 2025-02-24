using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetCoreMVCApp.Models;
using DotNetCoreMVCApp.Models.Entities;
using DotNetCoreMVCApp.Models.Repository;

namespace DotNetCoreMVCApp.Repository.Implementation
{
    public class UnitOfWork : IDisposable
    {
        private ApplicationDbContext _context;
        private GenericRepository<Driver> _driverRepository;
        
        private GenericRepository<Vehicle> _vehicleRepository;
        private GenericRepository<ConsumptionDetails> _consumptiondetailsRepository;
        private GenericRepository<OilInformation> _oilinformationRepository;
        private GenericRepository<TireInformation> _tireinformationRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

        }

        public GenericRepository<Driver> DriverRepository
        {
            get
            {

                if (_driverRepository == null)
                {
                    _driverRepository = new GenericRepository<Driver>(_context);
                }
                return _driverRepository;
            }
        }
        
        public GenericRepository<Vehicle> VehicleRepository
        {
            get
            {

                if (_vehicleRepository == null)
                {
                    _vehicleRepository = new GenericRepository<Vehicle>(_context);
                }
                return _vehicleRepository;
            }
        }

        public GenericRepository<ConsumptionDetails> ConsumptionDetailsRepository
        {
            get
            {

                if (_consumptiondetailsRepository == null)
                {
                    _consumptiondetailsRepository = new GenericRepository<ConsumptionDetails>(_context);
                }
                return _consumptiondetailsRepository;
            }
        }

        public GenericRepository<TireInformation> TireInformationRepository
        {
            get
            {

                if (_tireinformationRepository == null)
                {
                    _tireinformationRepository = new GenericRepository<TireInformation>(_context);
                }
                return _tireinformationRepository;
            }
        }
        public GenericRepository<OilInformation> OilInformationRepository
        {
            get
            {

                if (_oilinformationRepository == null)
                {
                    _oilinformationRepository = new GenericRepository<OilInformation>(_context);
                }
                return _oilinformationRepository;
            }
        }
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }

}
