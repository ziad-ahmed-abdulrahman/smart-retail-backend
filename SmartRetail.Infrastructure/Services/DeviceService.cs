using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SmartRetail.Application.DTOs.Device;
using SmartRetail.Application.Interfaces.Services;
using SmartRetail.Domain.Entities;
using SmartRetail.Infrastructure.Persistence.Contexts;

namespace SmartRetail.Infrastructure.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly AppDbContext _context;

        public DeviceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DeviceDto>> GetStoreDevicesAsync(int storeId, string ownerId)
        {
            var storeExists = await _context.Stores
                .AnyAsync(s => s.Id == storeId && (s.OwnerId == ownerId || ownerId == "admin"));

            if (!storeExists) return Enumerable.Empty<DeviceDto>();

            var devices = await _context.Devices
                .Where(d => d.StoreId == storeId)
                .OrderBy(d => d.Id)
                .Select(d => new DeviceDto
                {
                    Id = d.Id,
                    DeviceCode = d.DeviceCode,
                    Name = d.Name,
                    Type = d.Type.ToString(),
                    IsOnline = d.IsOnline,
                    LastPing = d.LastPing,
                    StoreId = d.StoreId
                })
                .ToListAsync();

            return devices;
        }

        public async Task<DeviceDto?> GetDeviceByIdAsync(int id, string ownerId)
        {
            var device = await _context.Devices
                .Include(d => d.Store)
                .FirstOrDefaultAsync(d => d.Id == id && (d.Store.OwnerId == ownerId || ownerId == "admin"));

            if (device == null) return null;

            return new DeviceDto
            {
                Id = device.Id,
                DeviceCode = device.DeviceCode,
                Name = device.Name,
                Type = device.Type.ToString(),
                IsOnline = device.IsOnline,
                LastPing = device.LastPing,
                StoreId = device.StoreId
            };
        }

        public async Task<DeviceDto> CreateDeviceAsync(string ownerId, CreateDeviceDto dto)
        {
            var store = await _context.Stores
                .FirstOrDefaultAsync(s => s.Id == dto.StoreId && (s.OwnerId == ownerId || ownerId == "admin"));

            if (store == null)
            {
                throw new InvalidOperationException("المتجر غير موجود أو غير مخصص لك.");
            }

            if (!Enum.TryParse<DeviceType>(dto.Type, true, out var deviceType))
            {
                deviceType = DeviceType.SmartCamera;
            }

            var device = new Device
            {
                DeviceCode = dto.DeviceCode,
                Name = dto.Name,
                Type = deviceType,
                IsOnline = dto.IsOnline,
                LastPing = DateTime.UtcNow,
                StoreId = dto.StoreId
            };

            _context.Devices.Add(device);
            await _context.SaveChangesAsync();

            return new DeviceDto
            {
                Id = device.Id,
                DeviceCode = device.DeviceCode,
                Name = device.Name,
                Type = device.Type.ToString(),
                IsOnline = device.IsOnline,
                LastPing = device.LastPing,
                StoreId = device.StoreId
            };
        }

        public async Task<DeviceDto?> UpdateDeviceAsync(int id, string ownerId, UpdateDeviceDto dto)
        {
            var device = await _context.Devices
                .Include(d => d.Store)
                .FirstOrDefaultAsync(d => d.Id == id && (d.Store.OwnerId == ownerId || ownerId == "admin"));

            if (device == null) return null;

            if (!Enum.TryParse<DeviceType>(dto.Type, true, out var deviceType))
            {
                deviceType = device.Type;
            }

            device.Name = dto.Name;
            device.Type = deviceType;
            device.IsOnline = dto.IsOnline;
            device.LastPing = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new DeviceDto
            {
                Id = device.Id,
                DeviceCode = device.DeviceCode,
                Name = device.Name,
                Type = device.Type.ToString(),
                IsOnline = device.IsOnline,
                LastPing = device.LastPing,
                StoreId = device.StoreId
            };
        }

        public async Task<bool> DeleteDeviceAsync(int id, string ownerId)
        {
            var device = await _context.Devices
                .Include(d => d.Store)
                .FirstOrDefaultAsync(d => d.Id == id && (d.Store.OwnerId == ownerId || ownerId == "admin"));

            if (device == null) return false;

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateStatusAsync(int id, bool isOnline)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return false;

            device.IsOnline = isOnline;
            device.LastPing = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
