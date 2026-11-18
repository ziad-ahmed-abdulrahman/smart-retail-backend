using System;
using System.ComponentModel.DataAnnotations;
using SmartRetail.Domain.Entities;

namespace SmartRetail.Application.DTOs.Device
{
    public class DeviceDto
    {
        public int Id { get; set; }
        public string DeviceCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsOnline { get; set; }
        public DateTime? LastPing { get; set; }
        public int StoreId { get; set; }
    }

    public class CreateDeviceDto
    {
        [Required(ErrorMessage = "معرف الجهاز مطلوب")]
        [MaxLength(50)]
        public string DeviceCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "اسم الجهاز مطلوب")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع الجهاز مطلوب")]
        public string Type { get; set; } = "SmartCamera"; // SmartCamera or PosTerminal

        public bool IsOnline { get; set; } = true;

        [Required(ErrorMessage = "معرف المتجر مطلوب")]
        public int StoreId { get; set; }
    }

    public class UpdateDeviceDto
    {
        [Required(ErrorMessage = "اسم الجهاز مطلوب")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "نوع الجهاز مطلوب")]
        public string Type { get; set; } = "SmartCamera";

        public bool IsOnline { get; set; }
    }

    public class UpdateDeviceStatusDto
    {
        public bool IsOnline { get; set; }
    }
}
